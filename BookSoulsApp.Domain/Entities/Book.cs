using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookSoulsApp.Domain.Entities;
public class Book
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Isbn { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string PublisherId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> CategoryIds { get; set; } = [];

    public int ReleaseYear { get; set; }
    public bool IsStricted { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }

    public double Rating { get; set; }
    public int RatingCount { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

