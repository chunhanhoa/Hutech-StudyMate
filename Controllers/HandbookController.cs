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
public class HandbookController : ControllerBase
{
    private readonly MongoDbContext _context;

    public HandbookController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var activities = await _context.Activities.Find(_ => true).ToListAsync();
        var userActivities = await _context.UserActivities.Find(ua => ua.UserId == userId).ToListAsync();

        var result = activities.Select(a => new
        {
            a.Id,
            a.Name,
            a.Description,
            a.Date,
            a.TrainingPoints,
            Status = userActivities.FirstOrDefault(ua => ua.ActivityId == a.Id)?.Status ?? "NotRegistered"
        });

        return Ok(result);
    }

    [HttpPost("register-activity/{activityId}")]
    public async Task<IActionResult> RegisterActivity(string activityId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var existing = await _context.UserActivities
            .Find(ua => ua.UserId == userId && ua.ActivityId == activityId)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            return BadRequest(new { message = "Đã đăng ký hoạt động này rồi" });
        }

        var userActivity = new UserActivity
        {
            UserId = userId,
            ActivityId = activityId,
            Status = "Registered"
        };

        await _context.UserActivities.InsertOneAsync(userActivity);
        return Ok(new { message = "Đăng ký thành công" });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null) return NotFound();

        // Tính điểm rèn luyện (demo)
        var userActivities = await _context.UserActivities.Find(ua => ua.UserId == userId && ua.Status == "Completed").ToListAsync();
        var totalPoints = 0; 
        // Cần join với Activities để tính điểm, nhưng làm đơn giản trước
        
        return Ok(new
        {
            user.FullName,
            user.StudentId,
            user.ClassName,
            user.Faculty,
            TrainingPoints = 85, // Mock data
            SV5T = false // Mock data
        });
    }
}
