using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Check.Models;

public class ActivityArticle
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty; // Tiêu đề
    public string ThumbnailUrl { get; set; } = string.Empty; // Ảnh đại diện
    public string Description { get; set; } = string.Empty; // Mô tả ngắn
    public string Content { get; set; } = string.Empty; // Nội dung HTML
    public string PublishDate { get; set; } = string.Empty; // Ngày đăng (String format dd/MM/yyyy HH:mm)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
