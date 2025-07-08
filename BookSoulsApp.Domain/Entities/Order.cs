using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using BookSoulsApp.Domain.Enums;

namespace BookSoulsApp.Domain.Entities
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CustomerId { get; set; }

        public string Code { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public string? CancelReason { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.None;
        public long? PaymentCode { get; set; } // lưu code của Paymentlink
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public IList<OrderBooks> OrderBooks { get; set; } = [];
    }
    public class OrderBooks
    {
        public string BookId { get; set; }
        public string BookTitle { get; set; }
        public decimal BookPrice { get; set; }
        public int Quantity { get; set; }
    }
}
