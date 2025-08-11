using Microsoft.AspNetCore.Mvc;
using Check.Services;
using Check.Models;

namespace Check.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdviceController : ControllerBase
{
    private readonly IAdviceService _advice;

    public AdviceController(IAdviceService advice)
    {
        _advice = advice;
    }

    public record GradeDto(string courseCode, string? courseName, int? credits, double? score10, string? letterGrade, double? gpa4);
    public record AdviceRequest(string mssv, List<GradeDto> grades);
    public record ChatRequest(string mssv, List<GradeDto> grades, List<ChatMessage> messages);
    public record ChatMessage(string role, string content);

    private static IEnumerable<ParsedGrade> Map(IEnumerable<GradeDto> list) =>
        list.Select(g => new ParsedGrade(
            g.courseCode,
            g.courseName,
            g.credits,
            g.score10,
            g.letterGrade,
            g.gpa4
        ));

    [HttpPost]
    public async Task<ActionResult<object>> Post([FromBody] AdviceRequest req, CancellationToken ct)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.mssv))
            return BadRequest("Thiếu mssv.");
        var grades = Map(req.grades ?? new()).ToList();
        var text = await _advice.GenerateAdviceAsync(req.mssv, grades, ct);
        return Ok(new { advice = text });
    }

    [HttpPost("chat")]
    public async Task<ActionResult<object>> Chat([FromBody] ChatRequest req, CancellationToken ct)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.mssv))
            return BadRequest("Thiếu mssv.");
        var grades = Map(req.grades ?? new()).ToList();
        var msgs = (req.messages ?? new()).Select(m => (m.role, m.content));
        var reply = await _advice.ChatAsync(req.mssv, grades, msgs, ct);
        return Ok(new { reply });
    }
}
