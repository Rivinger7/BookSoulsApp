using AutoMapper;
using BookSoulsApp.Application.Models.Books;
using BookSoulsApp.Application.Models.Notifications;
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.Models.Reviews;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Exceptions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using static System.Reflection.Metadata.BlobBuilder;

namespace BookSoulsApp.Infrastructure.Services;
public class NotificationService(IUnitOfWork unitOfWork, IMapper mapper) : INotificationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task CreateNotificationAsync(CreateNotificationRequest createNotificationRequest)
    {
        if(string.IsNullOrEmpty(createNotificationRequest.UserId))
        {
			throw new BadRequestCustomException("UserID cannot be null or empty.");
		}
		Notification notification = new Notification()
		{
			UserId = createNotificationRequest.UserId,
			Title = createNotificationRequest.Title,
			Content = createNotificationRequest.Content,
			CreatedAt = DateTime.UtcNow,
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
		IQueryable<Notification> query = _unitOfWork.GetCollection<Notification>().AsQueryable();
		// UserID
		if (!string.IsNullOrEmpty(notificationFilterRequest.UserID))
		{
			query = query.Where(n => n.UserId == notificationFilterRequest.UserID);
		}
		// TIle and Content
		if (!string.IsNullOrEmpty(notificationFilterRequest.Title))
		{
			query = query.Where(n => n.Title.Contains(notificationFilterRequest.Title, StringComparison.OrdinalIgnoreCase));
		}
		if (!string.IsNullOrEmpty(notificationFilterRequest.Content))
		{
			query = query.Where(n => n.Content.Contains(notificationFilterRequest.Content, StringComparison.OrdinalIgnoreCase));
		}
		// Date
		if (notificationFilterRequest.FromDate.HasValue && notificationFilterRequest.ToDate.HasValue)
		{
			query = query.Where(n => n.CreatedAt >= notificationFilterRequest.FromDate.Value && r.CreatedAt <= notificationFilterRequest.ToDate.Value);
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

		return new PaginatedResult<NotificationResponse>
		{
			Items = notificationResponses,
			TotalCount = notifications.Count(),
		};
	}

    public async Task<int> GetUnreadNotificationCountAsync(string userId)
    {
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
