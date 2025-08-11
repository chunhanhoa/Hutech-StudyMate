using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Check.Models;

namespace Check.Services;

public class AdviceService : IAdviceService
{
    // CẢNH BÁO: Không nên hard-code API key trong production / repo public.
    private const string HardcodedApiKey = "AIzaSyDkUUyAVLfVCqYqlPpQFT9Vn2DOmIAvgu0";
    private const string DefaultGeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models";

    private readonly IProgramService _programService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _cfg;

    public AdviceService(IProgramService programService, IHttpClientFactory httpClientFactory, IConfiguration cfg)
    {
        _programService = programService;
        _httpClientFactory = httpClientFactory;
        _cfg = cfg;
    }

    public Task<string> GenerateAdviceAsync(string studentId, IEnumerable<ParsedGrade> grades, CancellationToken ct) =>
        CallModelAsync(studentId, grades, null, ct);

    public Task<string> ChatAsync(string studentId, IEnumerable<ParsedGrade> grades, IEnumerable<(string role, string content)> messages, CancellationToken ct) =>
        CallModelAsync(studentId, grades, messages, ct);

    private static readonly string[] GeminiFallbackModels = new[]
    {
        "gemini-1.5-flash-latest",
        "gemini-1.5-flash",
        "gemini-1.5-pro-latest",
        "gemini-pro"
    };

    private async Task<string> CallModelAsync(string studentId, IEnumerable<ParsedGrade> grades,
        IEnumerable<(string role,string content)>? chatMessages, CancellationToken ct)
    {
        var endpoint = _cfg["AI:Endpoint"];
        var apiKey  = _cfg["AI:ApiKey"];
        var model   = _cfg["AI:Model"];

        if (string.IsNullOrWhiteSpace(endpoint))
            endpoint = DefaultGeminiEndpoint;
        if (string.IsNullOrWhiteSpace(apiKey))
            apiKey = HardcodedApiKey;

        var isGemini = !string.IsNullOrWhiteSpace(endpoint) &&
                       endpoint.Contains("generativelanguage.googleapis.com", StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(model))
            model = isGemini ? "gemini-1.5-flash-latest" : "gpt-4o-mini";

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(model))
            return "AI chưa được cấu hình (thiếu AI:Endpoint hoặc AI:Model).";

        List<ParsedGrade> gradeList;
        HashSet<string> completed;
        List<string> remaining;
        StringBuilder sb;
        string systemPrompt;
        string userContent;
        List<Dictionary<string,string>> messages;
        try
        {
            gradeList = (grades ?? Array.Empty<ParsedGrade>()).Where(g => g != null).ToList();
            completed = new HashSet<string>(
                gradeList.Select(g => g.CourseCode).Where(c => !string.IsNullOrWhiteSpace(c))!,
                StringComparer.OrdinalIgnoreCase);

            var allCourses = (_programService as ProgramService)?.AllCourseCodes ?? Array.Empty<string>();
            remaining = allCourses.Where(c => !completed.Contains(c)).Take(300).ToList();

            sb = new StringBuilder();
            sb.AppendLine($"Sinh viên: {studentId}");
            sb.AppendLine("Các môn đã học (mã | tín chỉ | điểm10 | GPA4):");
            foreach (var g in gradeList.OrderBy(g => g.CourseCode))
            {
                if (g == null) continue;
                var code = g.CourseCode ?? "-";
                int creditsVal = g.Credits ?? 0;
                string score10Text = g.Score10.HasValue ? g.Score10.Value.ToString("0.00") : "-";
                string gpaText = "-";
                try
                {
                    // Ưu tiên thuộc tính Gpa4, fallback Gpa (reflection an toàn)
                    var t = g.GetType();
                    var p = t.GetProperty("Gpa4") ?? t.GetProperty("Gpa");
                    if (p?.GetValue(g) is double d) gpaText = d.ToString("0.00");
                    else if (p?.GetValue(g) is float f) gpaText = f.ToString("0.00");
                    else if (p?.GetValue(g) is decimal m) gpaText = ((double)m).ToString("0.00");
                }
                catch { /* bỏ qua lỗi đơn lẻ */ }
                sb.AppendLine($"{code} | {creditsVal} | {score10Text} | {gpaText}");
            }
            sb.AppendLine();
            sb.AppendLine("Các môn còn thiếu (ước lượng theo CTĐT đã nạp):");
            sb.AppendLine(string.Join(", ", remaining.Take(80)));
            if (remaining.Count > 80) sb.AppendLine($"... (+{remaining.Count - 80} môn khác)");

            systemPrompt = @"Bạn là trợ lý học tập đại học, trả lời tiếng Việt, ngắn gọn, mạch lạc.
Nhiệm vụ:
1. Phân tích điểm mạnh/yếu theo nhóm môn (dựa vào GPA4 và điểm 10).
2. Gợi ý các môn nên ưu tiên đăng ký kỳ tới (giải thích ngắn).
3. Chỉ ra nguy cơ nợ/tắc do môn tiên quyết (nếu biết, nếu không có dữ liệu tiên quyết thì nêu giả định chung).
4. Chiến lược nâng GPA (tập trung môn nào, cân bằng khối lượng, mẹo retake nếu cần).
5. Định dạng:
- Tổng quan
- Ưu tiên
- Rủi ro tiên quyết
- Chiến lược GPA
- Kế hoạch đề xuất (liệt kê 1-2 kỳ tiếp).";

            userContent = chatMessages == null
                ? $"Dữ liệu:\n{sb}\nHãy tư vấn tổng quan."
                : $"Ngữ cảnh chung:\n{sb}\nCâu hỏi / tiếp tục hội thoại.";

            messages = new List<Dictionary<string,string>>
            {
                new() { ["role"]="system", ["content"]= systemPrompt }
            };

            if (chatMessages != null)
            {
                foreach (var (role, content) in chatMessages)
                {
                    if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(role)) continue;
                    var r = role == "assistant" ? "assistant" : (role == "system" ? "system" : "user");
                    messages.Add(new() { ["role"]= r, ["content"]= content });
                }
            }
            messages.Add(new() { ["role"]="user", ["content"]= userContent });
        }
        catch (Exception ex)
        {
            return "Lỗi nội bộ AI (chuẩn bị dữ liệu): " + ex.Message;
        }

        // --- Nhánh OpenAI ---
        if (!isGemini)
        {
            var payload = new
            {
                model,
                messages,
                temperature = 0.7
            };
            try
            {
                var client = _httpClientFactory.CreateClient("ai");
                if (!string.IsNullOrWhiteSpace(apiKey))
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                using var resp = await client.PostAsJsonAsync(endpoint, payload, cancellationToken: ct);
                var txt = await resp.Content.ReadAsStringAsync(ct);
                if (!resp.IsSuccessStatusCode)
                    return $"Lỗi gọi AI: {(int)resp.StatusCode} {resp.ReasonPhrase} - {txt}";

                using var doc = JsonDocument.Parse(txt);
                if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var choice0 = choices[0];
                    if (choice0.TryGetProperty("message", out var msgObj) &&
                        msgObj.TryGetProperty("content", out var contentEl))
                        return contentEl.GetString() ?? "Không nhận được phản hồi.";
                }
                return "Không nhận được phản hồi (JSON không đúng kỳ vọng).";
            }
            catch (Exception ex)
            {
                return "Lỗi nội bộ khi gọi AI: " + ex.Message;
            }
        }

        // --- Nhánh Gemini (fallback nhiều model) ---
        string BuildUrl(string baseEndpoint, string modelName, string key)
        {
            var root = baseEndpoint.TrimEnd('/');
            if (root.EndsWith("/models", StringComparison.OrdinalIgnoreCase))
                return $"{root}/{modelName}:generateContent?key={key}";
            if (root.Contains("/models/", StringComparison.OrdinalIgnoreCase))
            {
                var idx = root.IndexOf("/models/", StringComparison.OrdinalIgnoreCase);
                root = root[..idx];
            }
            if (root.EndsWith("/v1beta", StringComparison.OrdinalIgnoreCase) ||
                root.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
                return $"{root}/models/{modelName}:generateContent?key={key}";
            return $"{root}/v1beta/models/{modelName}:generateContent?key={key}";
        }

        var convBuilder = new StringBuilder();
        convBuilder.AppendLine(systemPrompt);
        if (chatMessages != null)
        {
            foreach (var (r, ctt) in chatMessages)
            {
                if (string.IsNullOrWhiteSpace(ctt)) continue;
                convBuilder.AppendLine((r == "assistant" ? "AI" : "Người dùng") + ": " + ctt);
            }
        }
        convBuilder.AppendLine("Người dùng: " + (chatMessages == null ? "Hãy tư vấn tổng quan dựa trên dữ liệu." : "Tiếp tục trả lời dựa trên ngữ cảnh."));
        convBuilder.AppendLine();
        convBuilder.AppendLine("DỮ LIỆU:");
        convBuilder.AppendLine(sb.ToString());

        var tried = new List<string>();
        var modelsToTry = new List<string>();
        if (!string.IsNullOrWhiteSpace(model)) modelsToTry.Add(model);
        foreach (var mfb in GeminiFallbackModels)
            if (!modelsToTry.Contains(mfb, StringComparer.OrdinalIgnoreCase))
                modelsToTry.Add(mfb);
        if (modelsToTry.Count == 0)
            return "Không có model Gemini để thử.";

        foreach (var m in modelsToTry)
        {
            tried.Add(m);
            var url = BuildUrl(endpoint, m, apiKey);
            var geminiContent = new
            {
                contents = new[]
                {
                    new {
                        role = "user",
                        parts = new[] { new { text = convBuilder.ToString() } }
                    }
                }
            };
            try
            {
                var client = _httpClientFactory.CreateClient("ai");
                using var resp = await client.PostAsJsonAsync(url, geminiContent, cancellationToken: ct);
                var txt = await resp.Content.ReadAsStringAsync(ct);
                if (!resp.IsSuccessStatusCode)
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                        continue; // thử model khác
                    return $"Lỗi gọi Gemini (model {m}): {(int)resp.StatusCode} {resp.ReasonPhrase} - {txt}";
                }

                using var doc = JsonDocument.Parse(txt);
                if (doc.RootElement.TryGetProperty("candidates", out var cands) &&
                    cands.ValueKind == JsonValueKind.Array && cands.GetArrayLength() > 0)
                {
                    var cand = cands[0];
                    if (cand.TryGetProperty("content", out var contentObj) &&
                        contentObj.TryGetProperty("parts", out var parts) &&
                        parts.ValueKind == JsonValueKind.Array && parts.GetArrayLength() > 0)
                    {
                        var first = parts[0];
                        if (first.TryGetProperty("text", out var tEl))
                            return tEl.GetString() ?? $"(Model {m}) Không nhận được phản hồi.";
                    }
                }
                return $"(Model {m}) Không nhận được phản hồi (cấu trúc JSON khác).";
            }
            catch (Exception ex)
            {
                if (m != modelsToTry.Last())
                    continue;
                return $"Lỗi nội bộ khi gọi Gemini (model {m}): {ex.Message}";
            }
        }

        return "Không tìm được model Gemini phù hợp. Đã thử: " + string.Join(", ", tried);
    }
}
