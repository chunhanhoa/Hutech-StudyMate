using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Check.Models;

public class UserActivity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string Status { get; set; } = "Registered"; // Registered, Completed, Rejected
    public string? ProofImage { get; set; }
    public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
}
