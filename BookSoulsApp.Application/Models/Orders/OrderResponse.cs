using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Enums;

namespace BookSoulsApp.Application.Models.Orders
{
    public class OrderResponse
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string Code { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; }
        public string? CancelReason { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public IList<OrderBooks> OrderBooks { get; set; } = [];
    }
}
