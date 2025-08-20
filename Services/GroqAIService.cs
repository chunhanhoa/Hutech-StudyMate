using System.Text;
using System.Text.Json;

namespace Check.Services;

public class GroqAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GroqAIService> _logger;
    private readonly string _apiKey;
    
    // Danh sách models mới nhất của Groq
    private static readonly string[] AvailableModels = new[]
    {
        "meta-llama/llama-4-scout-17b-16e-instruct",   // Main: mạnh nhất cho phân tích bảng điểm + CTĐT
        "Qwen2.5-14B-Instruct",                        // Reasoning backup, cũng giỏi xử lý dữ liệu có cấu trúc
        "EleutherAI/gpt-neox-20b",                     // GPT-OSS 20B, open-source hoàn toàn, fallback
        "gpt-4o-mini",                                 // Fast, realtime, chat nhanh
        "llama-3.1-8b-instant",                        // Fast, cân bằng tốc độ & chất lượng
        "llama-3.2-1b-preview",                        // Lightweight, siêu nhẹ
        "Qwen2.5-VL-7B-Instruct",                      // Multimodal (text + image)
        "gemma2-9b-it"                                 // Backup ổn định
    };

    public GroqAIService(IHttpClientFactory httpClientFactory, ILogger<GroqAIService> logger, IConfiguration config)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;

        // Đọc API key từ file riêng hoặc biến môi trường
        var keyPath = Path.Combine(AppContext.BaseDirectory, "groq.key");
        if (File.Exists(keyPath))
        {
            _apiKey = File.ReadAllText(keyPath).Trim();
        }
        else
        {
            // Đọc từ biến môi trường GROQ_API_KEY (chuẩn cho Render)
            _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "";
        }

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Hutech-StudyMate-AI/1.0");
    }

    public async Task<string> GetStudyAdviceAsync(string studentMessage, object studyData, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "⚠️ Chưa cấu hình API key cho trợ lý AI.";
            }

            var systemPrompt = BuildSystemPrompt(studyData);
            var truncatedPrompt = TruncatePrompt(systemPrompt, studentMessage);
            
            foreach (var model in AvailableModels)
            {
                try
                {
                    var result = await TryWithModel(model, truncatedPrompt.systemPrompt, truncatedPrompt.userMessage, ct);
                    if (!string.IsNullOrEmpty(result))
                    {
                        _logger.LogDebug("Thành công với model: {Model}", model);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Model {Model} thất bại: {Error}", model, ex.Message);
                    continue;
                }
            }

            return "❌ Xin lỗi, hiện tại tất cả models AI đều không khả dụng. Vui lòng thử lại sau.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq AI API");
            return "❌ Đã xảy ra lỗi khi xử lý yêu cầu. Vui lòng thử lại.";
        }
    }

    private (string systemPrompt, string userMessage) TruncatePrompt(string systemPrompt, string userMessage)
    {
        var estimatedTokens = (systemPrompt.Length + userMessage.Length) / 4;
        
        if (estimatedTokens <= 4000) // Giảm buffer để đảm bảo an toàn
        {
            return (systemPrompt, userMessage);
        }

        // Cắt bớt dữ liệu nếu quá dài
        var lines = systemPrompt.Split('\n');
        var truncatedLines = new List<string>();
        var isDataSection = false;
        var dataLines = 0;
        const int maxDataLines = 30; // Giảm số dòng data

        foreach (var line in lines)
        {
            if (line.Contains("DỮ LIỆU SINH VIÊN:"))
            {
                isDataSection = true;
                truncatedLines.Add(line);
                continue;
            }

            if (line.Contains("NHIỆM VỤ:"))
            {
                isDataSection = false;
                if (dataLines > maxDataLines)
                {
                    truncatedLines.Add("...(dữ liệu đã được rút gọn)...");
                }
                truncatedLines.Add(line);
                continue;
            }

            if (isDataSection)
            {
                dataLines++;
                if (dataLines <= maxDataLines)
                {
                    truncatedLines.Add(line);
                }
            }
            else
            {
                truncatedLines.Add(line);
            }
        }

        var newSystemPrompt = string.Join('\n', truncatedLines);
        var newUserMessage = userMessage.Length > 500 
            ? userMessage.Substring(0, 500) + "..."
            : userMessage;

        return (newSystemPrompt, newUserMessage);
    }

    private async Task<string?> TryWithModel(string modelName, string systemPrompt, string userMessage, CancellationToken ct)
    {
        var payload = new
        {
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            model = modelName,
            temperature = 0.3, // Giảm để response ổn định hơn
            max_tokens = 600,  // Giảm tokens để ngắn gọn hơn
            stream = false
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Model {Model} error: {StatusCode} - {Error}", modelName, response.StatusCode, error);
            
            if (error.Contains("decommissioned") || 
                error.Contains("model") && error.Contains("not") && error.Contains("found") ||
                error.Contains("Request too large") ||
                error.Contains("rate_limit_exceeded"))
            {
                throw new InvalidOperationException($"Model {modelName} không khả dụng");
            }
            
            return null;
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
        
        if (result.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            var firstChoice = choices[0];
            if (firstChoice.TryGetProperty("message", out var message) && 
                message.TryGetProperty("content", out var contentProp))
            {
                return contentProp.GetString();
            }
        }

        return null;
    }

    private string BuildSystemPrompt(object studyData)
    {
        var smartData = ExtractSmartData(studyData);
        
        // Kiểm tra xem đây có phải lần đầu tiên không
        bool isFirstInteraction = true;
        try 
        {
            var json = JsonSerializer.Serialize(studyData);
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            if (element.TryGetProperty("IsFirstInteraction", out var firstProp))
            {
                isFirstInteraction = firstProp.GetBoolean();
            }
        }
        catch 
        {
            // Mặc định là lần đầu tiên nếu không parse được
            isFirstInteraction = true;
        }
        
        if (isFirstInteraction)
        {
            // System prompt cho lần đầu tiên - đưa ra đánh giá tổng quan
            return $@"Bạn là trợ lý AI tư vấn học tập HUTECH chuyên nghiệp.

DỮ LIỆU SINH VIÊN:
{smartData}

NHIỆM VỤ LẦN ĐẦU:
- Đưa ra ĐÁNH GIÁ TỔNG QUAN về tình hình học tập
- Phân tích điểm mạnh/yếu từ dữ liệu thực tế
- Gợi ý hướng phát triển chính
- PHẢI đề cập đến 12 TC tự chọn và 2 hướng lựa chọn

QUY TẮC TRẢ LỜI LẦN ĐẦU:
1. ✅ TRẢ LỜI BẰNG TIẾNG VIỆT
2. ✅ CHI TIẾT HỢP LÝ (250-300 từ)
3. ✅ XUỐNG DÒNG rõ ràng, dễ đọc
4. ✅ DỰA VÀO DỮ LIỆU CỤ THỂ được cung cấp
5. ✅ SỬ DỤNG EMOJI để dễ nhìn
6. ✅ CHỈ in đậm **1-2 ý chính nhất** trong toàn bộ tin nhắn
7. ✅ LIỆT KÊ TÊN MÔN HỌC (không chỉ mã môn)
8. ✅ LUÔN đề cập đến 12 TC tự chọn và 2 lựa chọn

CÁCH TRẢ LỜI LẦN ĐẦU:
📊 Tình hình học tập:
[Đánh giá tổng quan về số môn, GPA, tín chỉ]

🎯 Điểm mạnh:
[Những gì đã làm tốt]

⚠️ Điểm yếu:
[Những môn còn thiếu - liệt kê TÊN MÔN đầy đủ]
Đặc biệt: Còn thiếu 12 TC tự chọn

💡 **Lựa chọn hoàn thành TC tự chọn:**
1. Đồ án tốt nghiệp (12 TC)
2. 4 môn thay thế (3TC x 4 = 12TC) - tùy chuyên ngành

❓ Câu hỏi quan trọng:
Bạn muốn chọn chuyên ngành nào? (An toàn thông tin, Khoa học dữ liệu, v.v.)

QUY TẮC ĐẶC BIỆT:
- LUÔN đề cập đến 12 TC tự chọn trong phần điểm yếu
- LUÔN giải thích 2 lựa chọn: đồ án vs 4 môn
- LUÔN hỏi về chuyên ngành để tư vấn cụ thể
- CHỈ in đậm 1-2 ý quan trọng nhất

QUAN TRỌNG: Đây là lần đầu phân tích, hãy đưa ra cái nhìn toàn diện và LUÔN đề cập đến 12 TC tự chọn.";
        }
        else
        {
            // System prompt cho những lần sau - ngắn gọn, không lặp lại
            return $@"Bạn là trợ lý AI tư vấn học tập HUTECH chuyên nghiệp.

DỮ LIỆU SINH VIÊN:
{smartData}

NHIỆM VỤ:
- Trả lời TRỰC TIẾP câu hỏi của người dùng
- Dựa trên dữ liệu cụ thể đã có
- KHÔNG lặp lại thông tin đã nói

QUY TẮC TRẢ LỜI:
1. ✅ TRẢ LỜI BẰNG TIẾNG VIỆT
2. ✅ NGẮN GỌN, SÚCÍCH (tối đa 150 từ)  
3. ✅ XUỐNG DÒNG rõ ràng
4. ✅ TẬP TRUNG vào câu hỏi cụ thể
5. ✅ SỬ DỤNG EMOJI phù hợp
6. ✅ CHỈ in đậm **1 từ/cụm từ quan trọng nhất** (hoặc không in đậm gì)
7. ❌ KHÔNG lặp lại thông số đã nói (GPA, số môn...)
8. ❌ KHÔNG đưa ra thông tin dài dòng

QUY TẮC ĐẶC BIỆT VỀ TC TỰ CHỌN:
- Khi hỏi về TC tự chọn: Nhắc đến 2 lựa chọn (đồ án vs 4 môn)
- Khi chưa biết chuyên ngành: Hỏi để tư vấn 4 môn thay thế cụ thể
- Khi đã biết chuyên ngành: Gợi ý 4 môn cụ thể theo ngành đó

CÁCH TRẢ LỜI:
- TRẢ LỜI THẲNG vào vấn đề
- ĐƯA RA lời khuyên cụ thể
- KẾT THÚC bằng câu hỏi ngắn (nếu cần)

QUAN TRỌNG: Hãy trả lời ngắn gọn và CHỈ in đậm điều thực sự quan trọng.";
        }
    }

    private string ExtractSmartData(object studyData)
    {
        try
        {
            var json = JsonSerializer.Serialize(studyData);
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            
            // Nếu có wrapper với StudyData bên trong, lấy dữ liệu thực
            if (element.TryGetProperty("StudyData", out var actualStudyData))
            {
                element = actualStudyData;
            }
            
            // Trích xuất thông tin quan trọng nhất
            var summary = new StringBuilder();
            
            // Thông tin cơ bản
            summary.AppendLine($"MSSV: {TryGetProperty(element, "studentId")}");
            summary.AppendLine($"Khoa: {TryGetProperty(element, "department")}");
            summary.AppendLine($"Niên khóa: {TryGetProperty(element, "academicYear")}");
            
            // Kết quả học tập
            summary.AppendLine($"Đã học: {TryGetProperty(element, "summary.totalSubjects")} môn");
            summary.AppendLine($"GPA(4): {TryGetProperty(element, "summary.gpa4")}");
            summary.AppendLine($"GPA(10): {TryGetProperty(element, "summary.gpa10")}");
            summary.AppendLine($"TC tích lũy: {TryGetProperty(element, "summary.accumulatedCredits")}");
            summary.AppendLine($"TC tự chọn thiếu: {TryGetProperty(element, "summary.missingElectiveCredits")}");
            
            // ĐÂY LÀ PHẦN QUAN TRỌNG: Đọc điểm từ dữ liệu grades gốc
            if (element.TryGetProperty("grades", out var gradesArray) && 
                gradesArray.ValueKind == JsonValueKind.Array)
            {
                summary.AppendLine("\n=== CHI TIẾT TẤT CẢ MÔN ĐÃ HỌC VÀ ĐIỂM SỐ ===");
                
                var gradeList = new List<string>();
                foreach (var grade in gradesArray.EnumerateArray())
                {
                    var code = TryGetProperty(grade, "courseCode") ?? 
                              TryGetProperty(grade, "CourseCode") ?? "";
                    var name = TryGetProperty(grade, "courseName") ?? 
                              TryGetProperty(grade, "CourseName") ?? "";
                    var credits = TryGetProperty(grade, "credits") ?? 
                                 TryGetProperty(grade, "Credits") ?? "";
                    var score10 = TryGetProperty(grade, "score10") ?? 
                                 TryGetProperty(grade, "Score10") ?? "";
                    var score4 = TryGetProperty(grade, "gpa") ?? 
                                TryGetProperty(grade, "Gpa") ?? 
                                TryGetProperty(grade, "gpa4") ?? 
                                TryGetProperty(grade, "Gpa4") ?? "";
                    var letter = TryGetProperty(grade, "letterGrade") ?? 
                                TryGetProperty(grade, "LetterGrade") ?? "";
                    
                    if (!string.IsNullOrEmpty(code) && code != "N/A")
                    {
                        var subjectInfo = $"- {code}";
                        if (!string.IsNullOrEmpty(name) && name != "N/A")
                            subjectInfo += $": {name}";
                        
                        var scoreDetails = new List<string>();
                        if (!string.IsNullOrEmpty(credits) && credits != "N/A")
                            scoreDetails.Add($"{credits}TC");
                        if (!string.IsNullOrEmpty(score10) && score10 != "N/A")
                            scoreDetails.Add($"Điểm 10: {score10}");
                        if (!string.IsNullOrEmpty(letter) && letter != "N/A")
                            scoreDetails.Add($"Xếp loại: {letter}");
                        if (!string.IsNullOrEmpty(score4) && score4 != "N/A")
                            scoreDetails.Add($"GPA 4: {score4}");
                        
                        if (scoreDetails.Any())
                            subjectInfo += $" [{string.Join(" | ", scoreDetails)}]";
                        
                        gradeList.Add(subjectInfo);
                    }
                }
                
                // Hiển thị tất cả môn đã học với điểm
                foreach (var gradeInfo in gradeList.Take(50)) // Giới hạn 50 môn để tránh quá dài
                {
                    summary.AppendLine(gradeInfo);
                }
                
                if (gradeList.Count > 50)
                {
                    summary.AppendLine($"... và {gradeList.Count - 50} môn khác");
                }
                
                summary.AppendLine($"\nTổng cộng: {gradeList.Count} môn đã hoàn thành");
            }
            
            // Môn chưa học (giới hạn để không quá dài)
            if (element.TryGetProperty("summary", out var summaryEl) && 
                summaryEl.TryGetProperty("notLearnedSubjects", out var notLearned) &&
                notLearned.ValueKind == JsonValueKind.Array)
            {
                summary.AppendLine("\n=== MÔN CHƯA HỌC (10 môn quan trọng đầu) ===");
                var count = 0;
                foreach (var subject in notLearned.EnumerateArray())
                {
                    if (count >= 10) break;
                    var code = TryGetProperty(subject, "code") ?? "";
                    var name = TryGetProperty(subject, "name") ?? "";
                    var credits = TryGetProperty(subject, "credits") ?? "";
                    
                    if (!string.IsNullOrEmpty(name) && name != "N/A")
                    {
                        summary.AppendLine($"- {name} [{code}] ({credits}TC)");
                    }
                    else if (!string.IsNullOrEmpty(code) && code != "N/A")
                    {
                        summary.AppendLine($"- {code} ({credits}TC)");
                    }
                    count++;
                }
                if (notLearned.GetArrayLength() > 10)
                {
                    summary.AppendLine($"... và {notLearned.GetArrayLength() - 10} môn khác");
                }
            }
            
            // Thông tin chuyên ngành
            if (element.TryGetProperty("currentProgram", out var programEl))
            {
                if (programEl.TryGetProperty("electiveGroups", out var groupsEl) &&
                    groupsEl.ValueKind == JsonValueKind.Array)
                {
                    summary.AppendLine("\n=== CHUYÊN NGÀNH KHẢ DỤNG CHO 12 TC TỰ CHỌN ===");
                    foreach (var group in groupsEl.EnumerateArray())
                    {
                        if (group.TryGetProperty("group_name", out var groupName))
                        {
                            var name = groupName.GetString();
                            if (!string.IsNullOrEmpty(name) && !name.ToLower().Contains("tốt nghiệp"))
                            {
                                summary.AppendLine($"- {name}");
                            }
                        }
                    }
                }
            }
            
            return summary.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi phân tích dữ liệu study data");
            return "Không thể phân tích dữ liệu học tập";
        }
    }

    private string? TryGetProperty(JsonElement element, string path)
    {
        try
        {
            var parts = path.Split('.');
            var current = element;
            
            foreach (var part in parts)
            {
                if (!current.TryGetProperty(part, out current))
                    return "N/A";
            }
            
            // Xử lý số thập phân để hiển thị đẹp hơn
            if (current.ValueKind == JsonValueKind.Number)
            {
                var number = current.GetDouble();
                return Math.Round(number, 2).ToString("0.##");
            }
            
            var result = current.ToString();
            return string.IsNullOrWhiteSpace(result) ? "N/A" : result;
        }
        catch
        {
            return "N/A";
        }
    }
}