
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.Models.Reviews;

namespace BookSoulsApp.Application.ServiceInterfaces;
public interface IReviewService
{
    Task CreateReviewAsync(CreateReviewRequest createReviewRequest);
    Task DeleteReviewByIdAsync(string id);
    Task<ReviewResponse> GetReviewByIdAsync(string id);
    Task<PaginatedResult<ReviewResponse>> GetReviewsAsync(ReviewFilterRequest reviewFilterRequest, int pageIndex = 1, int limit = 10);
}
