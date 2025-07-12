using BookSoulsApp.Application.Models.Dashboard;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BookSoulsApp.Infrastructure.Services
{
    public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async Task<IList<RevenueByDateResponse>> GetRevenueByDay(DateTime fromDate, DateTime toDate)
        {
            var builder = Builders<Order>.Filter;
            var filter = builder.Gte(o => o.CreatedAt, fromDate.Date) &
                         builder.Lte(o => o.CreatedAt, toDate.Date.AddDays(1).AddTicks(-1)) & // end of day
                         builder.Eq(o => o.OrderStatus, OrderStatus.Shipping) & // Chỉ lấy đơn hoàn tất
                         builder.Eq(o => o.PaymentStatus, PaymentStatus.Paid); // Chỉ lấy đơn hoàn tất

            var orders = await _unitOfWork.GetCollection<Order>().Find(filter).ToListAsync();

            var grouped = orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new RevenueByDateResponse
                {
                    Date = g.Key,
                    Amount = g.Sum(x => x.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return grouped;
        }
    }
}
