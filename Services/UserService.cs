using Check.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace Check.Services;

public class UserService
{
    private readonly MongoDbContext _context;

    public UserService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (user == null) return null;

        if (VerifyPassword(password, user.PasswordHash))
        {
            return user;
        }
        return null;
    }

    public async Task<bool> RegisterAsync(User user, string password)
    {
        var existing = await _context.Users.Find(u => u.Username == user.Username).FirstOrDefaultAsync();
        if (existing != null) return false;

        user.PasswordHash = HashPassword(password);
        await _context.Users.InsertOneAsync(user);
        return true;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User> CreateExternalUserAsync(string email, string fullName, string googleId)
    {
        var user = new User
        {
            Username = email, // Use email as username for external users
            Email = email,
            FullName = fullName,
            GoogleId = googleId,
            PasswordHash = "", // No password for external users
            Role = "Student" // Default role
        };

        await _context.Users.InsertOneAsync(user);
        return user;
    }

    public async Task<bool> CheckStudentIdExistsAsync(string studentId)
    {
        var user = await _context.Users.Find(u => u.StudentId == studentId || u.Username == studentId).FirstOrDefaultAsync();
        return user != null;
    }

    public async Task<(User? user, string? password)> AutoRegisterAsync(string studentId)
    {
        // Check if student ID already exists
        var exists = await CheckStudentIdExistsAsync(studentId);
        if (exists) return (null, null);

        // Generate random password
        var password = GenerateRandomPassword();

        // Create new user with Username = StudentId
        var user = new User
        {
            Username = studentId,
            StudentId = studentId,
            FullName = "", // Empty, can be updated later
            PasswordHash = HashPassword(password),
            Role = "Student",
            MustChangePassword = true // Require password change on first login
        };

        await _context.Users.InsertOneAsync(user);
        return (user, password);
    }

    private string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var random = new Random();
        var password = new char[10];
        
        for (int i = 0; i < password.Length; i++)
        {
            password[i] = chars[random.Next(chars.Length)];
        }
        
        return new string(password);
    }

    public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null) return false;

        // Verify old password
        if (!VerifyPassword(oldPassword, user.PasswordHash))
        {
            return false;
        }

        // Update password and clear MustChangePassword flag
        var update = Builders<User>.Update
            .Set(u => u.PasswordHash, HashPassword(newPassword))
            .Set(u => u.MustChangePassword, false);

        var result = await _context.Users.UpdateOneAsync(u => u.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateProfileAsync(string userId, string fullName, string? className, string? major, string? email, string? avatarUrl)
    {
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null) return false;

        var update = Builders<User>.Update
            .Set(u => u.FullName, fullName);

        if (className != null)
            update = update.Set(u => u.ClassName, className);

        if (major != null)
            update = update.Set(u => u.Major, major);

        if (email != null)
            update = update.Set(u => u.Email, email);

        if (!string.IsNullOrEmpty(avatarUrl))
            update = update.Set(u => u.AvatarUrl, avatarUrl);

        var result = await _context.Users.UpdateOneAsync(u => u.Id == userId, update);
        return result.ModifiedCount > 0;
    }
}
