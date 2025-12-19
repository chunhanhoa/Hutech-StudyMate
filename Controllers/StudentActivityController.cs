using Check.Models;
using Check.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace Check.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StudentActivityController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public StudentActivityController(MongoDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateActivityRequest request)
    {
        if (!User.Identity.IsAuthenticated) return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var fullName = User.FindFirst("FullName")?.Value;
        // Try to get StudentId from claims if available, otherwise fetch from DB or rely on what's passed (but safer to fetch)
        // In AccountController login, we didn't put StudentId in claims explicitly as "StudentId", let's check.
        // AccountController: new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim("FullName", user.FullName), ...
        // It doesn't seem to have StudentId in claims.
        // However, the user said "Tự fill vào dựa vào thông tin login".
        // I'll fetch the user to be sure.
        
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null) return Unauthorized();

        string imageUrl = "";
        if (request.Image != null && request.Image.Length > 0)
        {
            using (var ms = new MemoryStream())
            {
                await request.Image.CopyToAsync(ms);
                var fileBytes = ms.ToArray();
                imageUrl = "data:" + request.Image.ContentType + ";base64," + Convert.ToBase64String(fileBytes);
            }
        }

        // Calculate Academic Year
        // Logic: Sep (9) starts new year.
        // If Month >= 9: Year - (Year+1)
        // If Month < 9: (Year-1) - Year
        int year = request.ParticipationDate.Year;
        int month = request.ParticipationDate.Month;
        string academicYear;
        
        if (month >= 9)
        {
            academicYear = $"{year}-{year + 1}";
        }
        else
        {
            academicYear = $"{year - 1}-{year}";
        }

        var activity = new StudentActivity
        {
            UserId = userId,
            StudentId = user.StudentId,
            StudentName = user.FullName,
            ActivityName = request.ActivityName,
            ParticipationDate = request.ParticipationDate,
            ImageUrl = imageUrl,
            AcademicYear = academicYear
        };

        await _context.StudentActivities.InsertOneAsync(activity);

        return Ok(new { message = "Thêm hoạt động thành công", data = activity });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyActivities()
    {
        if (!User.Identity.IsAuthenticated) return Unauthorized();
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var activities = await _context.StudentActivities
            .Find(a => a.UserId == userId)
            .SortByDescending(a => a.ParticipationDate)
            .ToListAsync();

        return Ok(activities);
    }
}

public class CreateActivityRequest
{
    public string ActivityName { get; set; } = string.Empty;
    public DateTime ParticipationDate { get; set; }
    public IFormFile? Image { get; set; }
}
