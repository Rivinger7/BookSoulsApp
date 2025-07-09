using BookSoulsApp.Application.Models.Chats;

namespace BookSoulsApp.Application.ServiceInterfaces;
public interface IChatService
{
    Task<IEnumerable<ConversationResponse>> GetConversationsAsync();
    Task<IEnumerable<MessageResponse>> GetMessagesAsync(string conversationId);
}
