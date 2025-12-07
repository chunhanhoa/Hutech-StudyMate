using OfficeOpenXml;
using Check.Models;
using System.Text.RegularExpressions;
using System.Globalization;
using ExcelDataReader; // thêm
using System.Text; // thêm
using System.Net; // thêm

namespace Check.Services;

public class ExcelGradeParser : IExcelGradeParser
{
    // Cho phép mã kiểu COS120, CMP1074, LAW123, NDF210, ENC101...
    private static readonly Regex CodeRegex = new(@"^[A-Z]{2,}[A-Z0-9]{2,}$", RegexOptions.Compiled);

    public async Task<IReadOnlyList<ParsedGrade>> ParseAsync(Stream excelStream, CancellationToken ct = default)
    {
        // Đọc vào bộ nhớ để kiểm tra định dạng
        if (excelStream.CanSeek) excelStream.Position = 0;
        var msAll = new MemoryStream();
        await excelStream.CopyToAsync(msAll, ct);
        if (msAll.CanSeek) msAll.Position = 0;

        // 1. XLS (OLE)
        if (IsXls(msAll))
        {
            if (msAll.CanSeek) msAll.Position = 0;
            try { return ParseWithExcelDataReader(msAll, ct); }
            catch { /* sẽ fallback tiếp */ }
            if (msAll.CanSeek) msAll.Position = 0;
        }

        // 2. Thử XLSX bằng EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var pkg = new ExcelPackage();
        if (msAll.CanSeek) msAll.Position = 0;
        bool xlsxLoaded = false;
        try
        {
            await pkg.LoadAsync(msAll, ct);
            xlsxLoaded = true;
        }
        catch
        {
            // 3. Fallback thử ExcelDataReader lần nữa (trường hợp .xls giả đuôi .xlsx)
            if (msAll.CanSeek) msAll.Position = 0;
            try
            {
                var r = ParseWithExcelDataReader(msAll, ct);
                if (r.Count > 0) return r;
            }
            catch { /* bỏ qua để thử text */ }
        }

        if (xlsxLoaded)
        {
            if (pkg.Workbook == null || pkg.Workbook.Worksheets.Count == 0)
                return Array.Empty<ParsedGrade>();

            var list = new List<ParsedGrade>();
            var seenX = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var ws in pkg.Workbook.Worksheets)
            {
                if (ws == null) continue;
                var dim = ws.Dimension;
                if (dim == null || dim.End.Row < 1 || dim.End.Column < 1) continue;

                int maxRow = dim.End.Row;
                int maxCol = Math.Min(dim.End.Column, 40);

                for (int r = 1; r <= maxRow; r++)
                {
                    ct.ThrowIfCancellationRequested();
                    bool allEmpty = true;
                    var cells = new string[maxCol];
                    for (int c = 1; c <= maxCol; c++)
                    {
                        string text;
                        try
                        {
                            var val = ws.Cells[r, c].Value;
                            text = (val switch
                            {
                                DateTime dt => dt.ToString("yyyy-MM-dd"),
                                _ => Convert.ToString(val, CultureInfo.InvariantCulture)
                            }) ?? string.Empty;
                        }
                        catch { text = string.Empty; }
                        text = text.Trim();
                        if (text.Length > 0) allEmpty = false;
                        cells[c - 1] = text;
                    }
                    if (allEmpty) continue;
                    ProcessRow(cells, seenX, list);
                }
            }
            if (list.Count > 0) return list;
        }

        // 4. Fallback cuối: parse văn bản (CSV/TSV/HTML)
        if (msAll.CanSeek) msAll.Position = 0;
        var textGrades = ParseDelimited(msAll, ct);
        if (textGrades.Count > 0) return textGrades;

        throw new InvalidOperationException("Không nhận diện được định dạng Excel (.xlsx/.xls) hoặc file không chứa dữ liệu hợp lệ. Hãy kiểm tra: (1) File có mở được trong Excel? (2) Định dạng có đúng .xlsx hoặc .xls? (3) Nếu là xuất web, hãy lưu lại thành Excel chuẩn trước khi tải lên.");
    }

    // NEW: đọc .xls (và fallback) dùng ExcelDataReader
    private IReadOnlyList<ParsedGrade> ParseWithExcelDataReader(Stream stream, CancellationToken ct)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        if (stream.CanSeek) stream.Position = 0;
        using var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);
        var all = new List<ParsedGrade>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        do
        {
            while (reader.Read())
            {
                ct.ThrowIfCancellationRequested();
                var cells = new string[40];
                bool allEmpty = true;
                for (int i = 0; i < 40; i++)
                {
                    if (i >= reader.FieldCount) { cells[i] = string.Empty; continue; }
                    var val = reader.GetValue(i);
                    var text = (val switch
                    {
                        DateTime dt => dt.ToString("yyyy-MM-dd"),
                        _ => Convert.ToString(val, CultureInfo.InvariantCulture)
                    }) ?? string.Empty;
                    text = text.Trim();
                    if (text.Length > 0) allEmpty = false;
                    cells[i] = text;
                }
                if (allEmpty) continue;
                ProcessRow(cells, seen, all);
            }
        } while (reader.NextResult());
        return all;
    }

    // Tách logic xử lý 1 dòng (dùng chung)
    private void ProcessRow(string[] cells, HashSet<string> seen, List<ParsedGrade> target)
    {
        var joinedLower = string.Join(' ', cells).ToLowerInvariant();
        if (joinedLower.Contains("điểm trung bình")
            || joinedLower.Contains("số tín chỉ")
            || joinedLower.StartsWith("stt ")
            || joinedLower.StartsWith("stt\t")) return;

        string? code = null;
        var codeIndex = -1;
        for (int i = 0; i < cells.Length; i++)
        {
            var v = cells[i];
            if (v.Length >= 4 && v.Length <= 20 && CodeRegex.IsMatch(v))
            {
                code = v;
                codeIndex = i;
                break;
            }
        }
        if (code == null) return;

        string? courseName = null;
        if (codeIndex >= 0 && codeIndex + 1 < cells.Length)
        {
            var nameCell = cells[codeIndex + 1];
            if (!string.IsNullOrWhiteSpace(nameCell))
                courseName = nameCell;
        }

        int? credits = null;
        if (codeIndex >= 0 && codeIndex + 2 < cells.Length &&
            int.TryParse(cells[codeIndex + 2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var tc) &&
            tc >= 0 && tc <= 10)
            credits = tc;

        double? gpa4 = null;
        string? letter = null;
        double? score10 = null;
        bool IsLetter(string s) => s.Length is > 0 and <= 3 && Regex.IsMatch(s, @"^(A|B|C|D|F)(\+|-)?$", RegexOptions.IgnoreCase);

        for (int i = cells.Length - 1; i > codeIndex + 4; i--)
        {
            var raw = cells[i];
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var norm = raw.Trim().ToUpperInvariant();

            if (gpa4 == null && TryParseGpa(norm, out var g4))
            {
                gpa4 = g4;
                continue;
            }
            if (letter == null && IsLetter(norm))
            {
                letter = norm;
                continue;
            }
            if (score10 == null &&
                double.TryParse(norm.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var s10) &&
                s10 >= 0 && s10 <= 10 &&
                !(gpa4 != null && Math.Abs(s10 - gpa4.Value) < 0.0001 && s10 <= 4.0))
            {
                score10 = s10;
                continue;
            }
            if (gpa4 != null && score10 != null) break;
        }
        
        // Kiểm tra môn Quốc phòng (mã NDF + 3 số)
        bool isNationalDefense = code.Length == 6 && 
                                 code.StartsWith("NDF", StringComparison.OrdinalIgnoreCase) &&
                                 code.Substring(3).All(char.IsDigit);

        // Xác định môn có bị rớt không (điểm hệ 10 < 4 HOẶC điểm hệ 4 < 1)
        bool isFailed = (score10.HasValue && score10.Value < 4.0) || 
                        (gpa4.HasValue && gpa4.Value < 1.0);

        if (isNationalDefense)
        {
            // Môn Quốc phòng: chỉ cần score10 >= 0 (bao gồm cả lần rớt)
            // if (score10 == null || score10.Value < 0) return;
            // KHÔNG kiểm tra seen nữa - giữ tất cả các lần học
            target.Add(new ParsedGrade(code, courseName, credits, score10, null, null, isFailed));
        }
        else
        {
            // Môn thông thường: phải có điểm hợp lệ
            //if (score10 == null || score10.Value <= 0) return;
            
            // KHÔNG kiểm tra seen nữa - giữ tất cả các lần học
            target.Add(new ParsedGrade(code, courseName, credits, score10, letter, gpa4, isFailed));
        }
    }

    private static bool IsXls(Stream s)
    {
        if (!s.CanSeek) return false;
        var buf = new byte[8];
        var read = s.Read(buf, 0, 8);
        return read == 8 &&
               buf[0] == 0xD0 && buf[1] == 0xCF && buf[2] == 0x11 && buf[3] == 0xE0 &&
               buf[4] == 0xA1 && buf[5] == 0xB1 && buf[6] == 0x1A && buf[7] == 0xE1;
    }

    private static bool TryParseGpa(string raw, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        raw = raw.Replace(',', '.').Trim();
        if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return false;
        if (v < 0 || v > 4.0) return false;
        value = v;
        return true;
    }

    // NEW: Parser văn bản (CSV / TSV / HTML table)
    private IReadOnlyList<ParsedGrade> ParseDelimited(Stream stream, CancellationToken ct)
    {
        var list = new List<ParsedGrade>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (stream.CanSeek) stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 8192, leaveOpen: true);
        var text = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(text)) return list;

        // HTML table?
        if (text.IndexOf("<table", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            var rowRx = new Regex("<tr[\\s\\S]*?</tr>", RegexOptions.IgnoreCase);
            var cellRx = new Regex("<t[hd][^>]*>(.*?)</t[hd]>", RegexOptions.IgnoreCase);
            foreach (Match rm in rowRx.Matches(text))
            {
                ct.ThrowIfCancellationRequested();
                var cellsList = new List<string>();
                foreach (Match cm in cellRx.Matches(rm.Value))
                {
                    var raw = WebUtility.HtmlDecode(Regex.Replace(cm.Groups[1].Value, "<.*?>", "").Trim());
                    cellsList.Add(raw);
                }
                if (cellsList.Count == 0) continue;
                ProcessRow(cellsList.ToArray(), seen, list);
            }
            return list;
        }

        // CSV / TSV
        var lines = text.Split('\n');
        foreach (var rawLine in lines)
        {
            ct.ThrowIfCancellationRequested();
            var line = rawLine.TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line)) continue;
            // Ưu tiên tab; nếu không có tab dùng dấu phẩy
            string[] cells;
            if (line.Contains('\t'))
                cells = line.Split('\t');
            else
                cells = line.Split(',');
            if (cells.Length == 0) continue;
            // Chuẩn hóa
            for (int i = 0; i < cells.Length; i++)
                cells[i] = cells[i].Trim();
            ProcessRow(cells, seen, list);
        }
        return list;
    }
}
