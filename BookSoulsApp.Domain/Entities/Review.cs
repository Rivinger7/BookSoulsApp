using MongoDB.Bson.Serialization.Attributes;

namespace BookSoulsApp.Domain.Entities;
public class Review
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string BookId { get; set; }

    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string UserId { get; set; }

    public string Comment { get; set; }
    public double Rating { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
