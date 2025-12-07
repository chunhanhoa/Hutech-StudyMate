using Check.Models;
using Check.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace Check.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SV5TController : ControllerBase
{
    private readonly MongoDbContext _context;

    public SV5TController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] SV5TRegistrationRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        // Check if already registered for this academic year
        var existing = await _context.SV5TRegistrations
            .Find(r => r.UserId == userId && r.AcademicYear == request.AcademicYear)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            return BadRequest(new { message = "Bạn đã đăng ký xét SV5T cho năm học này rồi." });
        }

        var registration = new SV5TRegistration
        {
            UserId = userId,
            StudentName = request.StudentName,
            StudentId = request.StudentId,
            Major = request.Major,
            AcademicYear = request.AcademicYear,
            Reason = request.Reason,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _context.SV5TRegistrations.InsertOneAsync(registration);

        return Ok(new { message = "Đăng ký thành công!" });
    }
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var history = await _context.SV5TRegistrations
            .Find(r => r.UserId == userId)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(history);
    }
}

public class SV5TRegistrationRequest
{
    public string StudentName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
