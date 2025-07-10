using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Enums;

namespace BookSoulsApp.Application.Models.Orders
{
    public class OrderFilterRequest
    {
        public string? CustomerId { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
    }
}
