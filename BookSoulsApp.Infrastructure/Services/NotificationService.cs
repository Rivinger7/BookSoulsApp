using AutoMapper;
using BookSoulsApp.Application.Models.Notifications;
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Exceptions;
using BookSoulsApp.Domain.Utils;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BookSoulsApp.Infrastructure.Services;
public class NotificationService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : INotificationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task CreateNotificationAsync(CreateNotificationRequest createNotificationRequest)
    {
        // UserID lấy từ phiên người dùng có thể là FE hoặc BE
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;

        // Kiểm tra UserId
        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("Your session is limit, you must login again!");
        }

        Notification notification = new()
		{
			UserId = userId,
			Title = createNotificationRequest.Title,
			Content = createNotificationRequest.Content,
			CreatedAt = TimeControl.GetUtcPlus7Time(),
		};
		await _unitOfWork.GetCollection<Notification>().InsertOneAsync(notification);
    }

    public async Task DeletedNotificationByIdAsync(string notificationId)
    {
		UpdateResult updateResult = await _unitOfWork.GetCollection<Notification>()
		   .UpdateOneAsync(n => n.Id == notificationId, Builders<Notification>.Update.Set(c => c.IsDeleted, true));

		if (updateResult.ModifiedCount == 0)
		{
			throw new NotFoundCustomException("Delete failed");
		}
	}

    public async Task<PaginatedResult<NotificationResponse>> GetNotificationsAsync(NotificationFilterRequest notificationFilterRequest, int pageIndex = 1, int limit = 10)
    {
        // UserID lấy từ phiên người dùng có thể là FE hoặc BE
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;

        IQueryable<Notification> query = _unitOfWork.GetCollection<Notification>().AsQueryable();

        // Kiểm tra UserId
        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("Your session is limit, you must login again!");
        }

		// UserID
		if (!string.IsNullOrEmpty(userId))
		{
			query = query.Where(n => n.UserId == userId);
		}
		// TIle and Content
		if (!string.IsNullOrEmpty(notificationFilterRequest.Title))
		{
			query = query.Where(n => n.Title.ToLower().Contains(notificationFilterRequest.Title.ToLower()));
		}
		if (!string.IsNullOrEmpty(notificationFilterRequest.Content))
		{
			query = query.Where(n => n.Content.ToLower().Contains(notificationFilterRequest.Content.ToLower()));
		}
		// Date
		if (notificationFilterRequest.FromDate.HasValue && notificationFilterRequest.ToDate.HasValue)
		{
			query = query.Where(n => n.CreatedAt >= notificationFilterRequest.FromDate.Value && n.CreatedAt <= notificationFilterRequest.ToDate.Value);
		}
		else if (notificationFilterRequest.FromDate.HasValue)
		{
			query = query.Where(n => n.CreatedAt >= notificationFilterRequest.FromDate.Value);
		}
		else if (notificationFilterRequest.ToDate.HasValue)
		{
			query = query.Where(n => n.CreatedAt <= notificationFilterRequest.ToDate.Value);
		}

		query = query.Where(n => n.IsDeleted == false);

		// Phân trang và thực hiện truy vấn
		IEnumerable<Notification> notifications = await query
			.Skip((pageIndex - 1) * limit)
			.Take(limit)
			.ToListAsync();

		// Chuyển đổi sang Response
		IEnumerable<NotificationResponse> notificationResponses = _mapper.Map<IEnumerable<NotificationResponse>>(notifications);

		//long totalCount = await _unitOfWork.GetCollection<Notification>()
		//	.CountDocumentsAsync(n => n.UserId == userId && n.IsDeleted == false);
		long totalCount = await query.CountAsync();

        return new PaginatedResult<NotificationResponse>
		{
			Items = notificationResponses,
			TotalCount = totalCount,
		};
	}

    public async Task<int> GetUnreadNotificationCountAsync()
    {
        // UserID lấy từ phiên người dùng có thể là FE hoặc BE
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;

        // Kiểm tra UserId
        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("Your session is limit, you must login again!");
        }

        IQueryable<Notification> query = _unitOfWork.GetCollection<Notification>().AsQueryable();
		// UserID
		if (!string.IsNullOrEmpty(userId))
		{
			query = query.Where(n => n.UserId == userId);
		}

		//Unread and IsDeleted Notification
		query = query.Where(n => n.IsRead == false);
		query = query.Where(n => n.IsDeleted == false);

		return query.Count();

	}

    public async Task MarkNotificationAsReadAsync(string notificationId)
    {
		UpdateResult updateResult = await _unitOfWork.GetCollection<Notification>()
		   .UpdateOneAsync(n => n.Id == notificationId, Builders<Notification>.Update.Set(n => n.IsRead, true));

		if (updateResult.ModifiedCount == 0)
		{
			throw new NotFoundCustomException("Mark read failed");
		}
	}
}
