using MongoDB.Bson.Serialization.Attributes;
using System.Net;

namespace BookSoulsApp.Domain.Entities;
public class User
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; } = "Customer"; // "Admin", "Staff"
    public string? Gender { get; set; }
    public string? Avatar { get; set; }
    public Address? Address { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

