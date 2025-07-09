using BookSoulsApp.Application.Models.Chats;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Security.Claims;

namespace BookSoulsApp.Infrastructure.Services;
public class ChatService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : IChatService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<IEnumerable<ConversationResponse>> GetConversationsAsync()
    {
        string userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value ?? throw new UnauthorizedCustomException("Your session is limit, you must login again!");

        FilterDefinition<Conversation> filter = Builders<Conversation>.Filter.AnyEq(c => c.UserIds, userId);

        IEnumerable<Conversation> conversations = await _unitOfWork.GetCollection<Conversation>().Find(filter)
            .SortBy(c => c.UpdatedAt)
            .ToListAsync();

        IEnumerable<ConversationResponse> responses = conversations.Select(c => new ConversationResponse
        {
            ConversationId = c.Id,
            OtherUserId = c.UserIds.FirstOrDefault(uid => uid != userId),
            LastMessage = c.LastMessage?.Text,
            LastSenderId = c.LastMessage?.SenderId,
            LastSentAt = c.LastMessage?.SentAt
        });

        return responses;
    }

    public async Task<IEnumerable<MessageResponse>> GetMessagesAsync(string conversationId)
    {
        string userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value ?? throw new UnauthorizedCustomException("Your session is limit, you must login again!");

        FilterDefinition<Message> filter = Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId);

        IEnumerable<Message> messages = await _unitOfWork.GetCollection<Message>().Find(filter)
            .SortBy(m => m.SentAt)
            .ToListAsync();

        IEnumerable<MessageResponse> responses = messages.Select(m => new MessageResponse
        {
            Id = m.Id.ToString(),
            ConversationId = m.ConversationId,
            ReceiverId = m.ReceiverId,
            SenderId = m.SenderId,
            Text = m.DeletedFor.Contains(userId) ? "Tin nhắn đã thu hồi" : m.Text,
            SentAt = m.SentAt,
            IsRead = m.IsRead,
            IsDeleted = m.DeletedFor.Contains(userId)
        });

        return responses;
    }
}
