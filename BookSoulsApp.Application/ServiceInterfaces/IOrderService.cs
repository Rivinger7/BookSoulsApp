using BookSoulsApp.Application.Models.Orders;
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Domain.Enums;

namespace BookSoulsApp.Application.ServiceInterfaces
{
    public interface IOrderService
    {
        Task CancelOrder(string orderId, string cancelReason);
        Task ChangeOrderStatus(string orderId, OrderStatus status);
        Task<string> CreateOrder(CreateOrderRequest req);
        Task<OrderResponse> GetOrderByIdAsync(string orderId);
        Task<PaginatedResult<OrderResponse>> GetOrdersAsync(OrderFilterRequest req, int pageIndex = 1, int limit = 10);
    }
}
