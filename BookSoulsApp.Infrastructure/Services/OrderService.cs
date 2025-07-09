using AutoMapper;
using BookSoulsApp.Application.Models.Orders;
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Application.ThirdPartyServiceInterfaces.Payment;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Enums;
using BookSoulsApp.Domain.Exceptions;
using Microsoft.OpenApi.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BookSoulsApp.Infrastructure.Services
{
    public class OrderService(IUnitOfWork unitOfWork, IMapper mapper, IPayosService payosService) : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IPayosService _payosService = payosService;

        public async Task<PaginatedResult<OrderResponse>> GetOrdersAsync(OrderFilterRequest req, int pageIndex = 1, int limit = 10)
        {
            IQueryable<Order> query = _unitOfWork.GetCollection<Order>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(req.CustomerId))
            {
                query = query.Where(o => o.CustomerId == req.CustomerId);
            }
            if (req.OrderStatus.HasValue)
            {
                query = query.Where(o => o.OrderStatus == req.OrderStatus);
            }
            if (req.PaymentStatus.HasValue)
            {
                query = query.Where(o => o.PaymentStatus == req.PaymentStatus);
            }
            // Phân trang và thực hiện truy vấn
            IEnumerable<Order> orders = await query
                .Skip((pageIndex - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // Chuyển đổi sang Response
            IEnumerable<OrderResponse> orderResponses = orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                Code = o.Code,
                TotalPrice = o.TotalPrice,
                OrderStatus = o.OrderStatus.GetDisplayName(),
                CancelReason = o.CancelReason,
                PaymentStatus = o.PaymentStatus.GetDisplayName(),
                CreatedAt = o.CreatedAt,
                OrderBooks = o.OrderBooks.Select(ob => new OrderBooks
                {
                    BookId = ob.BookId,
                    BookTitle = ob.BookTitle,
                    BookPrice = ob.BookPrice,
                    Quantity = ob.Quantity
                }).ToList()
            });


            return new PaginatedResult<OrderResponse>
            {
                Items = orderResponses,
                TotalCount = orders.Count(),
            };
        }

        public async Task<OrderResponse> GetOrderByIdAsync(string orderId)
        {
            Order o = await _unitOfWork.GetCollection<Order>().Find(o => o.Id == orderId).FirstOrDefaultAsync()
                ?? throw new NotFoundCustomException("Order Not Found");

            return new OrderResponse
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                Code = o.Code,
                TotalPrice = o.TotalPrice,
                OrderStatus = o.OrderStatus.GetDisplayName(),
                CancelReason = o.CancelReason,
                PaymentStatus = o.PaymentStatus.GetDisplayName(),
                CreatedAt = o.CreatedAt,
                OrderBooks = o.OrderBooks.Select(ob => new OrderBooks
                {
                    BookId = ob.BookId,
                    BookTitle = ob.BookTitle,
                    BookPrice = ob.BookPrice,
                    Quantity = ob.Quantity
                }).ToList()
            };
        }

        public async Task<string> CreateOrder(CreateOrderRequest req)
        {
            Order order = new();
            order.CustomerId = req.CustomerId;

            IList<string> existCode = await _unitOfWork.GetCollection<Order>().AsQueryable().Select(o => o.Code).ToListAsync();
            string code = GenerateInvoiceCode().ToString();
            while (existCode.Contains(code))
            {
                code = GenerateInvoiceCode().ToString();
            }
            order.Code = code;

            IList<Book> books = await _unitOfWork.GetCollection<Book>().AsQueryable()
                .Where(b => req.OrderBooks.Select(ob => ob.BookId).Contains(b.Id)).ToListAsync();
            if (books.Count != req.OrderBooks.Count)
            {
                throw new BadRequestCustomException("Book Not Found");
            }
            decimal totalPrice = req.OrderBooks
                .Join(books,
                      selected => selected.BookId,
                      book => book.Id,
                      (selected, book) => book.Price * selected.Quantity)
                .Sum();
            order.TotalPrice = totalPrice;
            
            foreach (var book in books)
            {
                order.OrderBooks.Add(new OrderBooks()
                {
                    BookId = book.Id,
                    BookTitle = book.Title,
                    BookPrice = book.Price,
                    Quantity = req.OrderBooks.First(o => o.BookId == book.Id).Quantity
                });
            }

            await _unitOfWork.GetCollection<Order>().InsertOneAsync(order);
            // làm payment

            return await _payosService.CreatePaymentLinkRequest(order.Id);
        }

        public async Task CancelOrder(string orderId, string cancelReason)
        {
            Order order = await _unitOfWork.GetCollection<Order>().Find(o => o.Id == orderId).FirstOrDefaultAsync()
                ?? throw new NotFoundCustomException("Order Not Found");

            UpdateDefinitionBuilder<Order> updateBuilder = Builders<Order>.Update;
            List<UpdateDefinition<Order>> updates = [];
            updates.Add(updateBuilder.Set(o => o.OrderStatus, OrderStatus.Cancel));
            updates.Add(updateBuilder.Set(o => o.CancelReason, cancelReason));
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                updates.Add(updateBuilder.Set(o => o.PaymentStatus, PaymentStatus.Refund));
            }
            UpdateDefinition<Order> updateDefinition = updateBuilder.Combine(updates);

            UpdateResult updateResult = await _unitOfWork.GetCollection<Order>()
                .UpdateOneAsync(b => b.Id == orderId, updateDefinition);

            if (updateResult.ModifiedCount == 0)
            {
                throw new NotFoundCustomException("Cancel Order failed");
            }
        }
        public async Task ChangeOrderStatus(string orderId, OrderStatus status)
        {
            if (status == OrderStatus.Cancel)
            {
                await CancelOrder(orderId, "Store don't accept order");
                return;
            }
            // Mark the book as deleted
            UpdateResult updateResult = await _unitOfWork.GetCollection<Order>()
                .UpdateOneAsync(c => c.Id == orderId, Builders<Order>.Update.Set(c => c.OrderStatus, status));

            if (updateResult.ModifiedCount == 0)
            {
                throw new NotFoundCustomException("Change Order Status failed");
            }
        }
        private static long GenerateInvoiceCode()
        {
            // 8 chữ số: YYMMDDHH
            string timePart = DateTime.UtcNow.ToString("yyMMddHH");

            // 3 chữ số ngẫu nhiên:
            string randomPart = Random.Shared.Next(0, 1_000).ToString("D3");

            string combined = timePart + randomPart; // 11 chữ số

            return long.Parse(combined);
        }
    }
}
