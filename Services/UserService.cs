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
}
