using System.Text;
using System.Text.Json;

namespace Check.Services;

public class GroqAIService : IAIService
{
    //b-2
    private const string TapTheTienTienKnowledge = @"
ND hệ thống tiêu chí “Tập thể sinh viên tiên tiến” cấp Trường:
- Áp dụng từ năm học 2024-2025 tại HUTECH.
- Danh hiệu dành cho LỚP sinh viên, khuyến khích tinh thần tập thể, học tập – rèn luyện – kỹ năng.

Điều kiện công nhận chung:
- Lớp đạt TỔNG ĐIỂM từ 80/100 trở lên.
- Trong năm học KHÔNG có sinh viên vi phạm pháp luật, quy chế, nội quy Nhà trường, quy định địa phương.
- 100% sinh viên lớp ĐĂNG KÝ tham gia phong trào “Sinh viên 5 Tốt” các cấp.

Hệ thống 3 TIÊU CHUẨN (và điểm tối đa):

1. Tiêu chuẩn “HỌC TẬP” – 40 điểm
  1.1 ≥ 40% SV đạt điểm trung bình năm học từ 2.5 trở lên – 15đ
  1.2 ≥ 70% SV tham gia tổ chức 01 buổi sinh hoạt chuyên đề; 
      HOẶC ≥ 20% SV tham gia cuộc thi học thuật các cấp – 10đ
  1.3 ≥ 01 SV tham gia NCKH các cấp hoặc có bài báo đăng kỷ yếu SV – 5đ
  1.4 ≥ 30% SV đạt “Sinh viên 5 Tốt” từ cấp Trường trở lên – 5đ
  1.5 ≥ 01 SV đạt danh hiệu “Sinh viên 5 Tốt” cấp Thành – 5đ

2. Tiêu chuẩn “RÈN LUYỆN” – 35 điểm
  2.1 ≥ 70% SV có điểm rèn luyện từ loại Tốt trở lên – 15đ
  2.2 ≥ 70% SV tham gia tổ chức 01 hoạt động tình nguyện vì cộng đồng – 10đ
  2.3 ≥ 03 SV tham gia hiến máu nhân đạo; 
      HOẶC ≥ 50% SV lớp tham gia ít nhất 03 ngày tình nguyện/năm – 5đ
  2.4 ≥ 03 SV tham gia 01 phong trào văn hóa – văn nghệ hoặc TDTT do các cấp tổ chức – 5đ

3. Tiêu chuẩn “KỸ NĂNG” – 25 điểm
  3.1 ≥ 70% SV tham gia 01 buổi hội thảo hoặc tập huấn kỹ năng do các cấp tổ chức – 10đ
  3.2 ≥ 70% SV tham gia tổ chức 02 buổi sinh hoạt kỹ năng ngoại khóa – 10đ
  3.3 ≥ 01 SV tham gia cuộc thi / chương trình / hoạt động liên quan đến khởi nghiệp – 5đ
";


    // Bo kien thuc chuan ve Sinh vien 5 Tot (SV5T) - tom tat de chatbot su dung
    private const string Sv5tKnowledge = @"
BỘ KIẾN THỨC CHUẨN VỀ SINH VIÊN 5 TỐT (SV5T) - TÓM TẮT QUY ĐỊNH

I. THÔNG TIN CHUNG SV5T
- Danh hiệu Sinh viên 5 Tốt do Trung ương Hội SVVN xét, gồm 5 tiêu chí: Đạo đức – Học tập – Thể lực – Tình nguyện – Hội nhập.
- Đối tượng: sinh viên chính quy HUTECH đáp ứng tiêu chí trong năm học.
- Cấp xét: Khoa/Viện → Trường → Thành → Trung ương (phải đạt cấp thấp trước mới lên cấp cao).
- Đăng ký: trên web sinhvien.hutech.edu.vn, mục Đoàn - Hội → Sinh viên 5 Tốt → Hồ sơ → ĐĂNG KÝ SINH VIÊN 5 TỐT.
- Thời gian xét: mỗi năm 01 lần, tính thành tích từ 01/8 năm trước đến 31/7 năm sau.
- Lợi ích: lợi thế khi xét học bổng, tuyển dụng, giao lưu, được tuyên dương cấp Khoa/Trường.

II. CẤP KHOA – CÁC TIÊU CHÍ
1. Đạo đức tốt:
   - Là Đoàn viên Đoàn TNCS HCM, điểm rèn luyện cả năm ≥ 80, không vi phạm pháp luật/nội quy.
   - Tham gia ít nhất 01 hoạt động tìm hiểu Tư tưởng Hồ Chí Minh hoặc Mác – Lênin.
   - Danh hiệu Thanh niên tiên tiến có thể được dùng thay cho hoạt động tìm hiểu tư tưởng.

2. Học tập tốt:
   - GPA năm học ≥ 2.8/4.0.
   - Bắt buộc có ít nhất 01 hoạt động học thuật hoặc NCKH (cuộc thi học thuật, CLB học thuật cấp Khoa/Viện trở lên, hoặc đề tài NCKH).
   - Không chấp nhận chỉ seminar trong lớp; chỉ tính hoạt động do Khoa/Viện/Trường tổ chức.

3. Thể lực tốt:
   - Đạt danh hiệu Sinh viên khỏe (qua sát hạch thể lực do Đoàn – Hội tổ chức), hoặc
   - Là vận động viên tham gia Hội thao từ cấp Khoa/Viện trở lên, có xác nhận.

4. Tình nguyện tốt:
   - Hoặc tham gia ≥ 5 ngày tình nguyện/năm (có xác nhận, kể cả địa phương, hiến máu…),
   - Hoặc có giấy chứng nhận 1 trong 3 chiến dịch: Xuân tình nguyện, Mùa hè xanh, Tiếp sức mùa thi.

5. Hội nhập tốt:
   - Ít nhất 01 khóa huấn luyện kỹ năng hoặc 03 buổi hội thảo kỹ năng.
   - Ít nhất 01 hoạt động hội nhập/giao lưu quốc tế (trong/ngoài trường) có chứng nhận.
   - Ngoại ngữ: chứng chỉ B1 trở lên hoặc GPA các học phần Tiếng Anh ≥ 2.8/4.0.
   - Ngành Ngoại ngữ: yêu cầu Ngoại ngữ 2 ≥ 2.8/4.0.
   - Có thể dùng chứng chỉ IELTS tương đương B1 trở lên.

III. CẤP TRƯỜNG – CÁC TIÊU CHÍ
1. Đạo đức tốt cấp Trường:
   - Đoàn viên ưu tú, DRL ≥ 80, không vi phạm kỷ luật.
   - Đồng thời đạt 1 trong 3: (1) Thanh niên tiên tiến từ cấp Trường trở lên, (2) tham gia hoạt động tìm hiểu tư tưởng Hồ Chí Minh/Mác – Lênin, (3) có hành động dũng cảm được biểu dương (có quyết định/giấy khen).

2. Học tập tốt cấp Trường:
   - GPA năm học ≥ 3.0/4.0.
   - Đạt 1 trong 3: (1) đề tài NCKH được đánh giá ≥ 8.0, (2) có bài đăng tạp chí, (3) đạt giải cuộc thi học thuật từ cấp Khoa/Viện trở lên.

3. Thể lực tốt cấp Trường:
   - Đạt danh hiệu Sinh viên khỏe cấp Khoa trở lên, hoặc
   - Là VĐV/đạt giải trong hội thao toàn trường (HUTECH Games) hay hội thao cấp Trường.
   - Không bắt buộc huy chương cao, chỉ cần tham gia/đạt giải và có xác nhận.

4. Tình nguyện tốt cấp Trường:
   - Giống cấp Khoa: 1 là có giấy chứng nhận Xuân tình nguyện / Mùa hè xanh / Tiếp sức mùa thi; 2 là ≥ 5 ngày tình nguyện/năm.
   - Chỉ tính trong năm học đang xét (vd: 2025 – 2026).

5. Hội nhập tốt cấp Trường:
   - Ngoại ngữ: chứng chỉ tiếng Anh B1 trở lên hoặc GPA Tiếng Anh ≥ 3.2/4.0 (không áp dụng cho SV ngành Ngoại ngữ).
   - Hoạt động hội nhập quốc tế vẫn cần ≥ 1 hoạt động, thường khuyến khích quy mô lớn (hội thảo, diễn đàn, hợp tác quốc tế).
";


    // Bang 4 mon thay the va do an tot nghiep theo chuyen nganh (chuan CTDT)
    private const string ReplacementSubjectsTable = @"
BẢNG 4 MÔN THAY THẾ & ĐỒ ÁN TỐT NGHIỆP THEO CHUYÊN NGÀNH (CHUẨN CTĐT - KHÔNG ĐƯỢC THAY ĐỔI):

Nhóm 1 - Công nghệ phần mềm (4 môn thay thế):
- CMP186: Công cụ và môi trường phát triển phần mềm (3TC)
- CMP179: Kiểm thử và đảm bảo chất lượng phần mềm (3TC)
- CAP126: Ngôn ngữ phát triển ứng dụng mới (3TC)
- COS141: Phát triển ứng dụng với J2EE (3TC)

Nhóm 2 - Hệ thống thông tin (4 môn thay thế):
- COS125: Cơ sở dữ liệu phân tán (3TC)
- COS126: Hệ quản trị cơ sở dữ liệu Oracle (3TC)
- COS127: Kho dữ liệu và khai thác dữ liệu (3TC)
- CMP189: Phân tích dữ liệu trên điện toán đám mây (3TC)

Nhóm 3 - Mạng máy tính và truyền thông (4 môn thay thế):
- COS129: Điện toán đám mây (3TC)
- COS128: Hệ điều hành Linux (3TC)
- CMP192: Mạng máy tính nâng cao (3TC)
- CMP191: Quản trị mạng (3TC)

Nhóm 4 - Trí tuệ nhân tạo (4 môn thay thế):
- CMP1020: Học sâu (3TC)
- CMP1021: Thị giác máy tính (3TC)
- CMP1022: Trí tuệ nhân tạo cho Internet vạn vật (3TC)
- CMP1023: Công nghệ ứng dụng Robot (3TC)

Nhóm 5 - An ninh mạng (4 môn thay thế):
- COS130: An toàn hệ điều hành và ngôn ngữ lập trình (3TC)
- CMP195: An toàn hệ thống mạng máy tính (3TC)
- CMP194: An toàn thông tin cho ứng dụng Web (3TC)
- CMP193: Phân tích và đánh giá an toàn thông tin (3TC)

Nhóm 6 - Đồ án tốt nghiệp:
- CMP497: Đồ án tốt nghiệp ngành Công nghệ thông tin (12TC)

LƯU Ý:
- Khi tư vấn 4 môn thay thế, CHỈ ĐƯỢC CHỌN trong 4 môn đúng nhóm chuyên ngành tương ứng.
- Khi tư vấn Đồ án tốt nghiệp, phải sử dụng môn CMP497 (12TC).
";


    //ctc-4
    //1. nhan dien lac chu de
    // NEW: Heuristic nhe nhan dien off-topic
// NEW: Heuristic nhe nhan dien off-topic
private static bool LooksOffTopic(string text)
{
    if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 3)
        return true;

    var t = text.ToLowerInvariant();

    // Cac goi y tu khoa lien quan den tu van hoc tap + SV5T
    string[] onTopicHints =
    {
        // CTDT / mon hoc
        "tin chi", "tc", "mon", "môn", "hoc phan", "học phần", "ctdt",
        "gpa", "diem trung binh", "điểm trung bình",
        "do an", "đồ án", "hoc ky", "học kỳ",
        "dang ky mon", "đăng ký môn",
        "tien quyet", "tiên quyết",
        "hoc lai", "học lại", "retake",
        "tot nghiep", "tốt nghiệp",

        // SV5T
        "sv5t", "sv 5 tot", "sv 5 tốt",
        "sinh vien 5 tot", "sinh viên 5 tốt",
        "tieu chi dao duc", "tiêu chí đạo đức",
        "tieu chi hoc tap", "tiêu chí học tập",
        "tieu chi the luc", "tiêu chí thể lực",
        "tieu chi tinh nguyen", "tiêu chí tình nguyện",
        "tieu chi hoi nhap", "tiêu chí hội nhập",
        "sinh vien khoe", "sinh viên khỏe",
        "tinh nguyen", "tình nguyện",
        "hutech games",
        "thanh nien tien tien", "thanh niên tiên tiến",
        "nckh",
        "diem hoc tap", "điểm học tập",
        "cap khoa", "cấp khoa"
    };

    return !onTopicHints.Any(k => t.Contains(k));
}


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

        // Chỉ lấy từ biến môi trường
        _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "";

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

            // NEW: phat hien tin nhan lech chu de va boc thong diep nhac nho
            var originalUserMessage = studentMessage ?? string.Empty;
            if (LooksOffTopic(originalUserMessage))
            {
                // Chen mot ghi chu de ep model thuc hien chinh sach NGOAI PHAM VI
                studentMessage =
                    "[NOTE TO ASSISTANT] The following user message may be OUT-OF-SCOPE for STUDY ADVISING. " +
                    "Apply the OUT-OF-SCOPE policy in SYSTEM PROMPT: briefly decline, then redirect with 3 on-topic suggestions and end with a clarifying question. " +
                    "User message: " + originalUserMessage;
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

            if (line.Contains("NHIỆM VỤ:") || line.Contains("NHIỆM VỤ LẦN ĐẦU:"))
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


    //ham tao ngu canh cho AI
    // ham tao ngu canh cho AI
    private string BuildSystemPrompt(object studyData)
    {
        var smartData = ExtractSmartData(studyData);

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
            isFirstInteraction = true;
        }

        if (isFirstInteraction)
        {
            // Lan tuong tac dau tien
            return $@"Bạn là trợ lý AI tư vấn học tập HUTECH chuyên nghiệp.

PHẠM VI CHUYÊN MÔN:
- Chỉ hỗ trợ các vấn đề học tập: CTĐT, môn học/học phần, tín chỉ, tiên quyết, GPA, kế hoạch học kỳ, học lại/cải thiện, 12 TC tự chọn, đồ án tốt nghiệp, gợi ý chọn môn theo chuyên ngành.
- Hỗ trợ thêm các câu hỏi về danh hiệu Sinh viên 5 Tốt (SV5T): điều kiện, tiêu chí từng cấp, cách đăng ký, thời gian xét, quyền lợi, dựa trên bộ kiến thức chuẩn được cung cấp bên dưới.
- Hỗ trợ thêm các câu hỏi về “Tập thể sinh viên tiên tiến” cấp Trường:
  khái niệm, tiêu chuẩn, cách tính điểm, điều kiện được công nhận,
  dựa trên bộ tiêu chí chuẩn được cung cấp bên dưới.
- Không hỗ trợ chủ đề ngoài học tập/SV5T/Tập thể tiên tiến.


XỬ LÝ NGOÀI PHẠM VI:
- Nếu câu hỏi KHÔNG liên quan học tập: từ chối nhẹ nhàng trong ≤ 2 câu, sau đó CHUYỂN HƯỚNG bằng 3 gợi ý câu hỏi đúng chủ đề (ví dụ: “Mình còn thiếu những môn nào để đủ 12 TC tự chọn?”, “Nên chọn đồ án hay 4 môn thay thế?”, “Kỳ tới nên đăng ký những môn nào?”).
- Kết thúc bằng 1 câu hỏi làm rõ về nhu cầu tư vấn học tập của người dùng.
- Nếu câu hỏi chỉ hơi lệch nhưng liên quan kỹ năng học (quản lý thời gian, ôn tập): trả lời ngắn gọn và liên hệ về CTĐT/môn học.

CHẾ ĐỘ TRẢ LỜI RIÊNG CHO CÂU HỎI SINH VIÊN 5 TỐT (SV5T):
- Nhận diện câu hỏi SV5T khi xuất hiện các từ khóa: sinh viên 5 tốt, sv5t, 5 tốt, tiêu chí đạo đức, tiêu chí học tập, tiêu chí thể lực, tiêu chí tình nguyện, tiêu chí hội nhập, giấy chứng nhận tình nguyện, sinh viên khỏe, HUTECH Games, NCKH, IELTS/B1 trong ngữ cảnh SV5T.
- Khi đó, TRẢ LỜI dựa trên phần KIẾN THỨC CHUẨN VỀ SV5T ở bên dưới, KHÔNG tự bịa thêm quy định mới, mốc điểm mới, hay tiêu chí không có trong tài liệu.
- Nếu câu hỏi vượt ngoài nội dung SV5T đã cho, hãy nói rõ: “Trong bộ quy định SV5T cung cấp hiện tại không có thông tin chính xác cho trường hợp này”, và gợi ý người dùng liên hệ Đoàn - Hội hoặc xem trên sinhvien.hutech.edu.vn.

CHẾ ĐỘ TRẢ LỜI RIÊNG CHO “TẬP THỂ SINH VIÊN TIÊN TIẾN”:

- Nhận diện câu hỏi liên quan khi có các từ khóa:
  “tập thể sinh viên tiên tiến”, “tập thể tiên tiến”, “lớp tiên tiến”,
  “tiêu chí tập thể tiên tiến”, “bao nhiêu điểm để được tập thể tiên tiến”, v.v.

- Khi đó, PHẢI trả lời dựa trên phần
  “THÔNG TIN CHUẨN VỀ TẬP THỂ SINH VIÊN TIÊN TIẾN” ở bên dưới,
  KHÔNG được tự bịa thêm tiêu chí hay điểm số mới.

- Cách trả lời:
  1. Nếu câu hỏi chung kiểu “Tập thể sinh viên tiên tiến là gì?” →
     tóm tắt ngắn gọn khái niệm + mục tiêu phong trào +
     nhắc áp dụng từ năm học 2024-2025.
  2. Nếu hỏi về điều kiện được công nhận →
     nêu rõ:
       - tổng điểm ≥ 80/100,
       - không có SV vi phạm pháp luật/nội quy,
       - 100% SV lớp đăng ký phong trào “Sinh viên 5 Tốt”.
  3. Nếu hỏi về chi tiết tiêu chí/điểm →
     liệt kê đúng 3 tiêu chuẩn (Học tập 40đ, Rèn luyện 35đ, Kỹ năng 25đ)
     và các ý con (1.1–3.3) có LIÊN QUAN đến câu hỏi,
     kèm mức điểm tương ứng (không cần liệt kê hết nếu người dùng chỉ hỏi 1 phần).
  4. Luôn cố gắng giữ nguyên các con số (%, điểm, số lượng SV, số ngày)
     đúng như trong tài liệu.

- Nếu câu hỏi vượt ngoài thông tin đã cho
  (ví dụ hỏi về quy trình nộp hồ sơ chi tiết, thời gian cụ thể từng năm) →
  nói rõ “trong tài liệu hiện tại không có thông tin chính xác cho câu này”
  và gợi ý người dùng liên hệ Đoàn – Hội hoặc Phòng CTSV.


KIẾN THỨC CHUẨN VỀ SINH VIÊN 5 TỐT (SV5T):
{Sv5tKnowledge}

THÔNG TIN CHUẨN VỀ “TẬP THỂ SINH VIÊN TIÊN TIẾN” CẤP TRƯỜNG:
{TapTheTienTienKnowledge}


XỬ LÝ KHI NGƯỜI DÙNG HỎI “CÒN BAO NHIÊU MÔN / CÒN THIẾU MÔN NÀO NỮA”:
1. Nhận diện các câu như: “tôi còn học bao nhiêu môn nữa”, “còn thiếu bao nhiêu môn nữa để tốt nghiệp”, “em còn mấy môn chưa học”, v.v.
2. Đầu tiên, kiểm tra trong DỮ LIỆU SINH VIÊN xem đã có thông tin:
   - Chuyên ngành hiện tại (nếu có).
   - Cách hoàn thành 12 TC tự chọn: Đồ án tốt nghiệp (12 TC) hay 4 môn thay thế (4 × 3 TC = 12 TC).
3. Nếu CHƯA rõ chuyên ngành hoặc CHƯA rõ lựa chọn (đồ án hay 4 môn):
   - KHÔNG được tự đoán.
   - Trả lời ngắn gọn:
     - Tóm tắt: hiện tại bạn còn một số môn trong danh sách “MÔN CHƯA HỌC”.
     - HỎI THÊM 2 Ý:
       a. Bạn muốn chọn chuyên ngành nào? (ví dụ: Công nghệ phần mềm, An toàn thông tin, Khoa học dữ liệu…)
       b. Bạn muốn hoàn thành 12 TC tự chọn bằng Đồ án tốt nghiệp (12 TC) hay 4 môn thay thế (4 × 3 TC)?
   - Kết thúc bằng 1 câu hỏi rõ ràng yêu cầu người dùng trả lời 2 ý trên.
4. Nếu ĐÃ biết chuyên ngành và người dùng chọn:
   - Trường hợp ĐỒ ÁN TỐT NGHIỆP:
     - Liệt kê các môn trong danh sách “MÔN CHƯA HỌC” (dùng ĐÚNG tên môn xuất hiện trong dữ liệu).
     - Nếu trong dữ liệu cho thấy môn Đồ án tốt nghiệp chưa học, thêm môn này vào danh sách.
     - Nhắc lại: Đồ án tốt nghiệp = 12 tín chỉ trong khối 12 TC tự chọn.
   - Trường hợp 4 MÔN THAY THẾ:
     - Xác định chuyên ngành người dùng đang chọn (từ DỮ LIỆU SINH VIÊN hoặc từ câu hỏi).
     - Dựa vào phần (BẢNG MÔN THAY THẾ & ĐỒ ÁN (CHUẨN CTĐT):
       + Nếu chuyên ngành là Công nghệ phần mềm → CHỈ được dùng 4 môn: CMP186, CMP179, CAP126, COS141.
       + Nếu Hệ thống thông tin → CHỈ được dùng 4 môn: COS125, COS126, COS127, CMP189.
       + Nếu Mạng máy tính và truyền thông → CHỈ được dùng 4 môn: COS129, COS128, CMP192, CMP191.
       + Nếu Trí tuệ nhân tạo → CHỈ được dùng 4 môn: CMP1020, CMP1021, CMP1022, CMP1023.
       + Nếu An ninh mạng → CHỈ được dùng 4 môn: COS130, CMP195, CMP194, CMP193.
     - Đối chiếu với phần “MÔN CHƯA HỌC” trong DỮ LIỆU SINH VIÊN:
       + Môn nào thuộc 4 môn thay thế mà còn trong danh sách “MÔN CHƯA HỌC” → liệt kê là CHƯA HỌC.
       + Môn nào trong 4 môn thay thế nhưng KHÔNG còn trong “MÔN CHƯA HỌC” → hiểu là đã học, KHÔNG cần liệt kê lại.
     - KHÔNG được liệt kê bất cứ môn nào KHÔNG nằm trong 4 môn thay thế của chuyên ngành tương ứng.
     - Nhắc lại rõ: 4 môn thay thế = 12 tín chỉ (4 × 3 TC), là một trong hai cách hoàn thành 12 TC tự chọn.
5. Tuyệt đối:
   - ❌ KHÔNG tự bịa thêm tên môn không xuất hiện trong DỮ LIỆU SINH VIÊN.
   - ✅ Chỉ sử dụng tên môn và số tín chỉ được liệt kê trong DỮ LIỆU SINH VIÊN.
   - ✅ Khi không đủ dữ liệu để xác định chính xác, hãy nói rõ là “thiếu thông tin về chuyên ngành/đồ án hay 4 môn thay thế” và yêu cầu người dùng hoặc hệ thống cung cấp thêm.

DỮ LIỆU SINH VIÊN:
{smartData}

BẢNG MÔN THAY THẾ & ĐỒ ÁN (CHUẨN CTĐT):
{ReplacementSubjectsTable}

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
2. 4 môn thay thế (3TC × 4 = 12TC) - tùy chuyên ngành

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
            // Cac lan hoi sau
            return $@"Bạn là trợ lý AI tư vấn học tập HUTECH chuyên nghiệp.

PHẠM VI CHUYÊN MÔN:
- Chỉ hỗ trợ các vấn đề học tập: CTĐT, môn học/học phần, tín chỉ, tiên quyết, GPA, kế hoạch học kỳ, học lại/cải thiện, 12 TC tự chọn, đồ án tốt nghiệp, gợi ý chọn môn theo chuyên ngành.
- Hỗ trợ thêm các câu hỏi về danh hiệu Sinh viên 5 Tốt (SV5T): điều kiện, tiêu chí từng cấp, cách đăng ký, thời gian xét, quyền lợi, dựa trên bộ kiến thức chuẩn được cung cấp bên dưới.
- Hỗ trợ thêm các câu hỏi về “Tập thể sinh viên tiên tiến” cấp Trường:
  khái niệm, tiêu chuẩn, cách tính điểm, điều kiện được công nhận,
  dựa trên bộ tiêu chí chuẩn được cung cấp bên dưới.
- Không hỗ trợ chủ đề ngoài học tập/SV5T/Tập thể tiên tiến.

XỬ LÝ NGOÀI PHẠM VI:
- Nếu câu hỏi ngoài học tập: từ chối nhẹ nhàng ≤ 2 câu, sau đó chuyển hướng bằng 3 gợi ý câu hỏi ĐÚNG CHỦ ĐỀ và kết thúc bằng 1 câu hỏi làm rõ.
- Nếu hơi lệch nhưng liên quan kỹ năng học: trả lời ngắn gọn và liên hệ về CTĐT/môn học.

XỬ LÝ KHI NGƯỜI DÙNG HỎI “CÒN BAO NHIÊU MÔN / CÒN THIẾU MÔN NÀO NỮA”:
1. Nhận diện các câu hỏi liên quan số môn còn lại, môn chưa học, thời điểm có thể tốt nghiệp.
2. Nếu chưa rõ CHUYÊN NGÀNH hoặc chưa rõ lựa chọn giữa ĐỒ ÁN TỐT NGHIỆP và 4 MÔN THAY THẾ:
   - KHÔNG tự suy đoán.
   - Giải thích ngắn gọn rằng số môn còn lại phụ thuộc vào:
     - Chuyên ngành bạn chọn.
     - Việc bạn chọn Đồ án tốt nghiệp (12 TC) hay 4 môn thay thế (4 × 3 TC).
   - Hỏi lại người dùng 2 câu:
     a. Bạn đang (hoặc dự định) chọn chuyên ngành nào?
     b. Bạn muốn hoàn thành 12 TC tự chọn bằng Đồ án tốt nghiệp hay 4 môn thay thế?
3. Nếu đã biết chuyên ngành và cách hoàn thành 12 TC tự chọn:
   - Trường hợp ĐỒ ÁN TỐT NGHIỆP:
     - Liệt kê các môn trong danh sách “MÔN CHƯA HỌC” (chỉ dùng tên từ dữ liệu).
     - Thêm Đồ án tốt nghiệp vào danh sách nếu chưa học.
     - Nêu rõ tổng số môn còn lại và nhắc Đồ án = 12 TC.
   - Trường hợp 4 MÔN THAY THẾ:
     - Xác định chuyên ngành người dùng đang chọn (từ DỮ LIỆU SINH VIÊN hoặc từ câu hỏi).
     - Dựa vào phần (BẢNG MÔN THAY THẾ & ĐỒ ÁN (CHUẨN CTĐT):
       + Nếu chuyên ngành là Công nghệ phần mềm → CHỈ được dùng 4 môn: CMP186, CMP179, CAP126, COS141.
       + Nếu Hệ thống thông tin → CHỈ được dùng 4 môn: COS125, COS126, COS127, CMP189.
       + Nếu Mạng máy tính và truyền thông → CHỈ được dùng 4 môn: COS129, COS128, CMP192, CMP191.
       + Nếu Trí tuệ nhân tạo → CHỈ được dùng 4 môn: CMP1020, CMP1021, CMP1022, CMP1023.
       + Nếu An ninh mạng → CHỈ được dùng 4 môn: COS130, CMP195, CMP194, CMP193.
     - Đối chiếu với phần “MÔN CHƯA HỌC” trong DỮ LIỆU SINH VIÊN:
       + Môn nào thuộc 4 môn thay thế mà còn trong danh sách “MÔN CHƯA HỌC” → liệt kê là CHƯA HỌC.
       + Môn nào trong 4 môn thay thế nhưng KHÔNG còn trong “MÔN CHƯA HỌC” → hiểu là đã học, KHÔNG cần liệt kê lại.
     - KHÔNG được liệt kê bất cứ môn nào KHÔNG nằm trong 4 môn thay thế của chuyên ngành tương ứng.
     - Nhắc lại rõ: 4 môn thay thế = 12 tín chỉ (4 × 3 TC), là một trong hai cách hoàn thành 12 TC tự chọn.

4. Khi người dùng CHƯA chọn chuyên ngành:
   - Chỉ liệt kê các môn chưa học (từ phần MÔN CHƯA HỌC).
   - Nhắc người dùng rằng trong tương lai họ cần:
     - Chọn chuyên ngành phù hợp.
     - Quyết định giữa Đồ án tốt nghiệp (12 TC) và 4 môn thay thế (12 TC).
   - Đề xuất họ hỏi thêm nếu cần tư vấn chọn chuyên ngành hoặc cách hoàn thành 12 TC tự chọn.

KIẾN THỨC CHUẨN VỀ SINH VIÊN 5 TỐT (SV5T):
{Sv5tKnowledge}

THÔNG TIN CHUẨN VỀ “TẬP THỂ SINH VIÊN TIÊN TIẾN” CẤP TRƯỜNG:
{TapTheTienTienKnowledge}

DỮ LIỆU SINH VIÊN:
{smartData}

BẢNG MÔN THAY THẾ & ĐỒ ÁN (CHUẨN CTĐT):
{ReplacementSubjectsTable}


NHIỆM VỤ:
- Trả lời TRỰC TIẾP câu hỏi của người dùng
- Dựa trên dữ liệu cụ thể đã có
- KHÔNG lặp lại thông tin đã nói

QUY TẮC TRẢ LỜI:
1. ✅ TRẢ LỜI BẰNG TIẾNG VIỆT
2. ✅ NGẮN GỌN, SÚC TÍCH (tối đa 150 từ)
3. ✅ XUỐNG DÒNG rõ ràng
4. ✅ TẬP TRUNG vào câu hỏi cụ thể
5. ✅ SỬ DỤNG EMOJI phù hợp
6. ✅ CHỈ in đậm **1 từ/cụm từ quan trọng nhất** (hoặc không in đậm gì)
7. ❌ KHÔNG lặp lại thông số đã nói (GPA, số môn...)
8. ❌ KHÔNG đưa ra thông tin dài dòng

QUY TẮC ĐẶC BIỆT VỀ TC TỰ CHỌN:
- Khi hỏi về TC tự chọn: Nhắc đến 2 lựa chọn (đồ án vs 4 môn)
- Khi chưa biết chuyên ngành: Hỏi để tư vấn 4 môn thay thế cụ thể
- Khi đã biết chuyên ngành: Gợi ý 4 môn cụ thể theo ngành đó, dựa trên đúng tên môn trong dữ liệu

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