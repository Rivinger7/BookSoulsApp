using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookSoulsApp.Domain.Entities;
public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }

    public string Title { get; set; }
    public string Content { get; set; }
    public bool IsRead { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
