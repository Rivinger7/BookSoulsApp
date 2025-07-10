namespace BookSoulsApp.Application.Models.Chats;
public class ConversationResponse
{
    public string ConversationId { get; set; }
    public string? OtherUserId { get; set; }
    public string? LastMessage { get; set; }
    public string? LastSenderId { get; set; }
    public DateTime? LastSentAt { get; set; }
}
