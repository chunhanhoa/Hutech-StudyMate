using OfficeOpenXml;
using Check.Models;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Check.Services;

public class ExcelGradeParser : IExcelGradeParser
{
    // Cho phép mã kiểu COS120, CMP1074, LAW123, NDF210, ENC101...
    private static readonly Regex CodeRegex = new(@"^[A-Z]{2,}[A-Z0-9]{2,}$", RegexOptions.Compiled);

    public async Task<IReadOnlyList<ParsedGrade>> ParseAsync(Stream excelStream, CancellationToken ct = default)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var pkg = new ExcelPackage();

        if (excelStream.CanSeek) excelStream.Position = 0;
        try
        {
            await pkg.LoadAsync(excelStream, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("File không phải định dạng Excel hợp lệ: " + ex.Message, ex);
        }

        if (pkg.Workbook == null || pkg.Workbook.Worksheets.Count == 0)
            return Array.Empty<ParsedGrade>();

        var all = new List<ParsedGrade>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                    catch
                    {
                        text = string.Empty; // lỗi ô -> bỏ qua
                    }
                    text = text.Trim();
                    if (text.Length > 0) allEmpty = false;
                    cells[c - 1] = text;
                }
                if (allEmpty) continue;

                var joinedLower = string.Join(' ', cells).ToLowerInvariant();
                // Bỏ header / tổng hợp
                if (joinedLower.Contains("điểm trung bình")
                    || joinedLower.Contains("số tín chỉ")
                    || joinedLower.StartsWith("stt ")
                    || joinedLower.StartsWith("stt\t")) continue;

                // tìm mã + index
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
                if (code == null) continue;

                // tên môn (ô ngay sau mã)
                string? courseName = null;
                if (codeIndex >= 0 && codeIndex + 1 < cells.Length)
                {
                    var nameCell = cells[codeIndex + 1];
                    if (!string.IsNullOrWhiteSpace(nameCell))
                        courseName = nameCell;
                }

                // tín chỉ
                int? credits = null;
                if (codeIndex >= 0 && codeIndex + 2 < cells.Length &&
                    int.TryParse(cells[codeIndex + 2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var tc) &&
                    tc >= 0 && tc <= 10)
                    credits = tc;

                // quét cuối dòng
                double? gpa4 = null;
                string? letter = null;
                double? score10 = null;
                bool IsLetter(string s) => s.Length is > 0 and <= 3 && Regex.IsMatch(s, @"^(A|B|C|D|F)(\+|-)?$", RegexOptions.IgnoreCase);

                for (int i = cells.Length - 1; i >= 0; i--)
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

                if (gpa4 == null) continue;

                if (seen.Add(code))
                    all.Add(new ParsedGrade(code, courseName, credits, score10, letter, gpa4));
            }
        }
        return all;
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
}
