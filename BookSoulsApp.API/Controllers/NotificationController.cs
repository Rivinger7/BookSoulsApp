using BookSoulsApp.Application.Models.Notifications;
using BookSoulsApp.Application.Models.Reviews;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSoulsApp.API.Controllers;
[Route("api/notifications")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    private readonly INotificationService _notificationService = notificationService;

	// Nhớ phân quyền cho các phương thức này nếu cần thiết

	[Authorize(Roles = "Customer"), HttpGet]
	public async Task<IActionResult> GetNotifications([FromQuery] NotificationFilterRequest notificationFilterRequest, [FromQuery] int pageIndex = 1, [FromQuery] int limit = 10)
	{
		var result = await _notificationService.GetNotificationsAsync(notificationFilterRequest, pageIndex, limit);
		return Ok(new { Message = "Notification retrieved successfully", result });
	}

	[Authorize(Roles = "Customer"), HttpPut("{id}")]
	public async Task<IActionResult> MarkNotificationAsRead(string id)
	{
		await _notificationService.MarkNotificationAsReadAsync(id);
		return Ok(new {Message = "Mark notification as read successfully"});
	}

	[Authorize(Roles = "Admin,Customer,Staff"), HttpPost]
	public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest createNotificationRequest)
	{
		await _notificationService.CreateNotificationAsync(createNotificationRequest);
		return Ok(new { Message = "Created review successfully" });
	}

	[Authorize(Roles = "Admin,Customer,Staff"), HttpDelete("{id}")]
	public async Task<IActionResult> DeleteNotification(string id)
	{
		await _notificationService.DeletedNotificationByIdAsync(id);
		return Ok(new { Message = "Deleted notification successfully" });
	}
} 
