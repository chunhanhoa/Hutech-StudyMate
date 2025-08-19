using Microsoft.AspNetCore.Mvc;
using Check.Services;
using Check.Models;

namespace Check.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IProgramService _programService;
    private readonly IExcelGradeParser _parser;

    public UploadController(IProgramService programService, IExcelGradeParser parser)
    {
        _programService = programService;
        _parser = parser;
    }

    [HttpPost]
    [DisableRequestSizeLimit]
    [RequestSizeLimit(20_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadResult>> Post([FromForm] string mssv, [FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File rỗng.");
        if (string.IsNullOrWhiteSpace(mssv))
            return BadRequest("Thiếu MSSV.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);

        List<ParsedGrade> gradesParsed;
        try
        {
            var grades = await _parser.ParseAsync(ms, ct);
            gradesParsed = grades.ToList();
        }
        catch (Exception ex)
        {
            return BadRequest($"Lỗi đọc Excel: {ex.Message}");
        }

        var (programCode, doc) = _programService.GetCurriculumByStudentId(mssv);

        // Bổ sung tên môn nếu thiếu
        var enriched = gradesParsed
            .Select(g => string.IsNullOrWhiteSpace(g.CourseName)
                ? g with { CourseName = _programService is ProgramService ps ? ps.GetCourseName(g.CourseCode) : g.CourseName }
                : g)
            .ToList();

        string? dept = null;
        string? year = null;
        int? totalCredits = null;
        if (doc != null)
        {
            var root = doc.RootElement;
            if (root.TryGetProperty("department", out var d)) dept = d.GetString();
            if (root.TryGetProperty("academic_year", out var y)) year = y.GetString();
            if (root.TryGetProperty("total_credits", out var tc)) totalCredits = tc.GetInt32();
        }

        return Ok(new UploadResult(
            mssv,
            programCode,
            doc != null,
            dept,
            year,
            totalCredits,
            enriched
    ));
    }
}
