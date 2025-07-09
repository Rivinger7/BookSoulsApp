using BookSoulsApp.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSoulsApp.API.Controllers;
[Route("api/chat")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
public class ChatController(IChatService chatService) : ControllerBase
{
    private readonly IChatService _chatService = chatService;

    [Authorize(Roles = "Staff,Customer"), HttpGet("conversation")]
    public IActionResult CreateConversation()
    {
        var result = _chatService.GetConversationsAsync();
        return Ok(new { message = "Get conversations successfully.", result });
    }

    [Authorize(Roles = "Staff,Customer"), HttpGet("messages/{conversationId}")]
    public IActionResult GetMessages(string conversationId)
    {
        var result = _chatService.GetMessagesAsync(conversationId);
        return Ok(new { message = "Get messages successfully.", result });
    }
}
