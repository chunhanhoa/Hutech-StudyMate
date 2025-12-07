using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Check.Models;

public class SV5TRegistration
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty; // MSSV
    public string Major { get; set; } = string.Empty; // Chuyên Ngành
    public string AcademicYear { get; set; } = string.Empty; // Năm học đăng ký
    public string Reason { get; set; } = string.Empty; // Lý do
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
