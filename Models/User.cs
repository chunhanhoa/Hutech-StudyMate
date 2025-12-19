using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Check.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty; // MSSV
    public string Role { get; set; } = "Student"; // Student, Admin
    public string? ClassName { get; set; } // Lớp
    public string? Faculty { get; set; } // Khoa
    public string? Major { get; set; } // Chuyên Ngành
    public string? Email { get; set; } // Email for OAuth2 mapping
    public string? GoogleId { get; set; } // Google Unique ID
    public bool MustChangePassword { get; set; } = false; // True for auto-registered users
    public string? AvatarUrl { get; set; } // Profile picture URL or Base64
}
