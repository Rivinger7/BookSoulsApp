using BookSoulsApp.Application.Models.Notifications;
using BookSoulsApp.Application.Models.Pagination;

namespace BookSoulsApp.Application.ServiceInterfaces;
public interface INotificationService
{
    Task CreateNotificationAsync(CreateNotificationRequest createNotificationRequest);
    Task MarkNotificationAsReadAsync(string notificationId);
    Task DeletedNotificationByIdAsync(string notificationId);
    Task<PaginatedResult<NotificationResponse>> GetNotificationsAsync(NotificationFilterRequest notificationFilterRequest, int pageIndex = 1, int limit = 10);
    Task<int> GetUnreadNotificationCountAsync(string userId);
}
