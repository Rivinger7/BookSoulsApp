using BookSoulsApp.Application.Models.Dashboard;

namespace BookSoulsApp.Application.ServiceInterfaces
{
    public interface IDashboardService
    {
        Task<IList<RevenueByDateResponse>> GetRevenueByDay(DateTime fromDate, DateTime toDate);
    }
}
