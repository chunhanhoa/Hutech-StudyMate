using Microsoft.AspNetCore.Mvc;
using Check.Services;
using System.Text.Json;

namespace Check.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IAIService aiService, ILogger<ChatController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<object>> Ask([FromBody] ChatRequest request, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Tin nhắn không được để trống" });
            }

            if (request.StudyData == null)
            {
                return BadRequest(new { error = "Chưa có dữ liệu học tập để phân tích" });
            }

            // Log dữ liệu để debug (tạm thời)
            _logger.LogDebug("StudyData received: {StudyDataType}", request.StudyData.GetType().Name);
            _logger.LogDebug("IsFirstInteraction: {IsFirst}", request.IsFirstInteraction);

            // Tạo enhanced study data với thông tin isFirstInteraction
            var enhancedStudyData = new 
            {
                IsFirstInteraction = request.IsFirstInteraction,
                // Sao chép tất cả dữ liệu gốc
                StudyData = request.StudyData
            };

            // Truyền message và enhanced data cho AI service
            var response = await _aiService.GetStudyAdviceAsync(
                request.Message, 
                enhancedStudyData, 
                ct
            );

            return Ok(new { response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý chat request: {Message}", request.Message);
            return StatusCode(500, new { error = "Lỗi hệ thống, vui lòng thử lại sau" });
        }
    }

    public record ChatRequest(
        string Message, 
        object? StudyData = null, 
        object[]? ChatHistory = null,
        bool IsFirstInteraction = true
    );
}