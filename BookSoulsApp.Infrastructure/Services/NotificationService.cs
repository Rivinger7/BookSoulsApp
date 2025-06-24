using AutoMapper;
using BookSoulsApp.Application.Models.Notifications;
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.ServiceInterfaces;

namespace BookSoulsApp.Infrastructure.Services;
public class NotificationService(IUnitOfWork unitOfWork, IMapper mapper) : INotificationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public Task CreateNotificationAsync(CreateNotificationRequest createNotificationRequest)
    {
        throw new NotImplementedException();
    }

    public Task DeletedNotificationByIdAsync(string notificationId)
    {
        throw new NotImplementedException();
    }

    public Task<PaginatedResult<NotificationResponse>> GetNotificationsAsync(string userId, int pageIndex = 1, int limit = 10)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetUnreadNotificationCountAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task MarkNotificationAsReadAsync(string notificationId)
    {
        throw new NotImplementedException();
    }
}
