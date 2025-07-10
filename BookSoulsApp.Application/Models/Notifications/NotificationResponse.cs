using BookSoulsApp.Application.Mappers;
using BookSoulsApp.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookSoulsApp.Application.Models.Notifications;
public class NotificationResponse : IMapFrom<Notification>
{
	public string Id { get; set; }
	public string UserId { get; set; }
	public string Title { get; set; }
	public string Content { get; set; }
	public bool IsRead { get; set; } 
	public DateTime CreatedAt { get; set; }
}
