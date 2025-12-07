using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Check.Models;

public class StudentActivity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty; // Link to User Account
    public string StudentId { get; set; } = string.Empty; // MSSV
    public string StudentName { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public DateTime ParticipationDate { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
