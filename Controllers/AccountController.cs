using Check.Models;
using Check.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Check.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserService _userService;

    public AccountController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.LoginAsync(request.Username, request.Password);
        if (user == null)
        {
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng" });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("FullName", user.FullName),
            new Claim("Major", user.Major ?? ""),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);

        return Ok(new { message = "Đăng nhập thành công", fullName = user.FullName, mustChangePassword = user.MustChangePassword });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new User
        {
            Username = request.StudentId, // Username = StudentId
            FullName = request.FullName,
            StudentId = request.StudentId,
            ClassName = request.ClassName,
            Faculty = request.Faculty,
            Major = request.Major
        };

        var result = await _userService.RegisterAsync(user, request.Password);
        if (!result)
        {
            return BadRequest(new { message = "MSSV đã được đăng ký" });
        }

        return Ok(new { message = "Đăng ký thành công" });
    }

    [HttpPost("auto-register")]
    public async Task<IActionResult> AutoRegister([FromBody] AutoRegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StudentId))
        {
            return BadRequest(new { message = "MSSV không được để trống" });
        }

        var (user, password) = await _userService.AutoRegisterAsync(request.StudentId);
        
        if (user == null || password == null)
        {
            return BadRequest(new { message = "MSSV đã được đăng ký" });
        }

        return Ok(new { 
            message = "Đăng ký thành công", 
            username = user.Username,
            password = password 
        });
    }

    [HttpGet("check-mssv/{mssv}")]
    public async Task<IActionResult> CheckMSSV(string mssv)
    {
        var exists = await _userService.CheckStudentIdExistsAsync(mssv);
        return Ok(new { exists });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return Ok(new { message = "Đăng xuất thành công" });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return Unauthorized();

        return Ok(new 
        { 
            username = user.Username, 
            fullName = user.FullName, 
            major = user.Major, 
            role = user.Role,
            studentId = user.StudentId,
            className = user.ClassName,
            faculty = user.Faculty,
            mustChangePassword = user.MustChangePassword,
            avatarUrl = user.AvatarUrl,
            email = user.Email
        });
    }

    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var success = await _userService.UpdateProfileAsync(
            userId,
            request.FullName,
            request.ClassName,
            request.Major,
            request.Email,
            request.AvatarUrl
        );

        if (!success)
        {
            return BadRequest(new { message = "Cập nhật thất bại" });
        }

        return Ok(new { message = "Cập nhật thông tin thành công" });
    }
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var success = await _userService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
        
        if (!success)
        {
            return BadRequest(new { message = "Mật khẩu cũ không đúng" });
        }

        return Ok(new { message = "Đổi mật khẩu thành công" });
    }

    [HttpGet("login-google")]
    public IActionResult LoginGoogle()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
        return Challenge(properties, "Google");
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync("Cookies"); 
        // Note: Usually ExternalLogin uses a separate cookie, but we can try to see if the default scheme works
        // However, standard flow is: Challenge Google -> Google Redirects to /signin-google (handled by middleware) -> Middleware calls callback -> We handle final signin.
        
        // Let's rely on the middleware to handle the callback at /signin-google, 
        // but wait, we need to capture the user info AFTER the middleware validates it.
        // Actually, the default callback path for Google is /signin-google.
        // We need to set the RedirectUri to an action where WE process the user logic.
        
        var authenticateResult = await HttpContext.AuthenticateAsync("Google");
        if (!authenticateResult.Succeeded)
            return BadRequest(new { message = "Google authentication failed" });

        var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "Email not received from Google" });

        // Check if user exists
        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            // Register new external user
             user = await _userService.CreateExternalUserAsync(email, name ?? "Unknown", googleId ?? "");
        }

        // Sign in to our Cookie Scheme
        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("FullName", user.FullName),
            new Claim("Major", user.Major ?? ""),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(userClaims, "Cookies");
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);

        // Redirect to homepage
        return Redirect("/");
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string? Faculty { get; set; }
    public string? Major { get; set; }
}

public class AutoRegisterRequest
{
    public string StudentId { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string? Major { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
}
