using System.Text.Json;

namespace Check.Services;

public class ProgramService : IProgramService
{
    private readonly Dictionary<string, JsonDocument> _byProgramCode = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, JsonDocument> _byIdentifier = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _courseNameByCode = new(StringComparer.OrdinalIgnoreCase);

    public ProgramService(IConfiguration cfg, IWebHostEnvironment env)
    {
        var dirName = cfg.GetValue<string>("CurriculumDirectory") ?? "ProgramJson";
        var full = Path.Combine(env.ContentRootPath, dirName);
        if (!Directory.Exists(full)) return;

        foreach (var file in Directory.EnumerateFiles(full, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var json = File.ReadAllText(file);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("program_code", out var pc))
                {
                    var code = pc.GetString();
                    if (!string.IsNullOrWhiteSpace(code))
                        _byProgramCode[code] = doc;
                }
                if (doc.RootElement.TryGetProperty("program_identifier", out var pid))
                {
                    var id = pid.GetString();
                    if (!string.IsNullOrWhiteSpace(id))
                        _byIdentifier[id] = doc;
                }
                // Thu thập mã/tên môn
                CollectCourses(doc.RootElement);
            }
            catch { /* bỏ qua file lỗi */ }
        }
    }

    private void CollectCourses(JsonElement root)
    {
        void Walk(JsonElement e)
        {
            switch (e.ValueKind)
            {
                case JsonValueKind.Object:
                    if (e.TryGetProperty("code", out var cProp) &&
                        e.TryGetProperty("name", out var nProp))
                    {
                        var code = cProp.GetString();
                        var name = nProp.GetString();
                        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
                            _courseNameByCode[code] = name;
                    }
                    foreach (var p in e.EnumerateObject()) Walk(p.Value);
                    break;
                case JsonValueKind.Array:
                    foreach (var item in e.EnumerateArray()) Walk(item);
                    break;
            }
        }
        Walk(root);
    }

    public (string? programCode, JsonDocument? doc) GetCurriculumByStudentId(string studentId)
    {
        // Heuristic MSSV -> program code
        // TODO: Điều chỉnh theo chuẩn MSSV thực tế
        // Ví dụ: nếu MSSV chứa 7480201 => CNTT
        if (string.IsNullOrWhiteSpace(studentId))
            return (null, null);

        if (studentId.Contains("7480201"))
        {
            if (_byProgramCode.TryGetValue("7480201", out var doc))
                return ("7480201", doc);
        }

        foreach (var kv in _byIdentifier)
        {
            if (studentId.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                return (kv.Value.RootElement.GetProperty("program_code").GetString(), kv.Value);
        }

        // Fallback: nếu chỉ có 1 chương trình thì dùng luôn
        if (_byProgramCode.Count == 1)
        {
            var first = _byProgramCode.First();
            return (first.Key, first.Value);
        }

        return (null, null);
    }

    public IEnumerable<string> ListProgramCodes() => _byProgramCode.Keys;

    public string? GetCourseName(string code) =>
        string.IsNullOrWhiteSpace(code) ? null :
        (_courseNameByCode.TryGetValue(code, out var n) ? n : null);

    public bool IsCourseCode(string code) =>
        !string.IsNullOrWhiteSpace(code) && _courseNameByCode.ContainsKey(code);

    public IReadOnlyCollection<string> AllCourseCodes => _courseNameByCode.Keys.ToList().AsReadOnly();

    // NEW: Trả về tuple (code, name)
    public IEnumerable<(string code, string name)> EnumerateCourses() =>
        _courseNameByCode.Select(kv => (kv.Key, kv.Value));
}
