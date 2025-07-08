using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookSoulsApp.Application.Models.Orders
{
    public class CreateOrderRequest
    {
        public string CustomerId { get; set; }
        public IList<OrderBooksRequest> OrderBooks { get; set; } = [];
    }
    public class OrderBooksRequest
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string BookId { get; set; }
        public int Quantity { get; set; }
    }
}
