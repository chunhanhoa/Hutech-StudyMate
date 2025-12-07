using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Check.Models;

public class Activity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Type { get; set; } = "Movement"; // Movement, Academic, Volunteer
    public int TrainingPoints { get; set; } = 0;
}
