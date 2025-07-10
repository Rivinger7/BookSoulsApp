using AutoMapper;
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.Models.Reviews;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Exceptions;
using BookSoulsApp.Domain.Utils;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BookSoulsApp.Infrastructure.Services;
public class ReviewService(IUnitOfWork unitOfWork, IMapper mapper) : IReviewService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<PaginatedResult<ReviewResponse>> GetReviewsAsync(ReviewFilterRequest reviewFilterRequest, int pageIndex = 1, int limit = 10)
    {
        IQueryable<Review> query = _unitOfWork.GetCollection<Review>().AsQueryable();

        if (!string.IsNullOrEmpty(reviewFilterRequest.Comment))
        {
            query = query.Where(r => r.Comment.ToLower().Contains(reviewFilterRequest.Comment.ToLower()));
        }

        if (!string.IsNullOrEmpty(reviewFilterRequest.BookId))
        {
            query = query.Where(r => r.BookId == reviewFilterRequest.BookId);
        }
        if (!string.IsNullOrEmpty(reviewFilterRequest.UserId))
        {
            query = query.Where(r => r.UserId == reviewFilterRequest.UserId);
        }
        // Filter by creation date range
        if (reviewFilterRequest.FromDate.HasValue && reviewFilterRequest.ToDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= reviewFilterRequest.FromDate.Value && r.CreatedAt <= reviewFilterRequest.ToDate.Value);
        }
        else if (reviewFilterRequest.FromDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= reviewFilterRequest.FromDate.Value);
        }
        else if (reviewFilterRequest.ToDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= reviewFilterRequest.ToDate.Value);
        }
        // Filter by rating by MinRating and MaxRating
        if (reviewFilterRequest.MinRating.HasValue && reviewFilterRequest.MaxRating.HasValue)
        {
            query = query.Where(r => r.Rating >= reviewFilterRequest.MinRating.Value && r.Rating <= reviewFilterRequest.MaxRating.Value);
        }
        else if (reviewFilterRequest.MinRating.HasValue)
        {
            query = query.Where(r => r.Rating >= reviewFilterRequest.MinRating.Value);
        }
        else if (reviewFilterRequest.MaxRating.HasValue)
        {
            query = query.Where(r => r.Rating <= reviewFilterRequest.MaxRating.Value);
        }

        // Phân trang
        query = query
            .Skip((pageIndex - 1) * limit)
            .Take(limit);

        // Thực hiện truy vấn
        IEnumerable<Review> reviews = await query.ToListAsync();

        // Get all user IDs from the reviews
        IEnumerable<string> userIds = reviews.Select(r => r.UserId).Distinct().ToList();

        // Fetch all users with these IDs
        IEnumerable<User> users = await _unitOfWork.GetCollection<User>()
            .Find(u => userIds.Contains(u.Id))
            .ToListAsync();

        // Create a dictionary for quick user lookup
        Dictionary<string, User> usersDictionary = users.ToDictionary(u => u.Id);

        IEnumerable<ReviewResponse> reviewResponses = reviews.Select(review =>
        {
            ReviewResponse reviewResponse = _mapper.Map<ReviewResponse>(review);

            // Set the user information in the reviewResponse
            if (usersDictionary.TryGetValue(review.UserId, out User? user))
            {
                reviewResponse.User = new UserProfileResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Avatar = user.Avatar
                };
            }

            return reviewResponse;
        });

        return new PaginatedResult<ReviewResponse>
        {
            Items = reviewResponses,
            TotalCount = reviewResponses.Count(),
        };
    }

    public async Task<ReviewResponse> GetReviewByIdAsync(string id)
    {
        Review? review = await _unitOfWork.GetCollection<Review>().Find(r => r.Id == id).FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Review not found");

        User? user = await _unitOfWork.GetCollection<User>().Find(u => u.Id == review.UserId).FirstOrDefaultAsync() ?? throw new NotFoundCustomException("User not found");

        ReviewResponse reviewResponse = _mapper.Map<ReviewResponse>(review);

        reviewResponse.User = new UserProfileResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Avatar = user.Avatar
        };
        return reviewResponse;
    }

    // Check if a user has bought a book by checking if they have a paid order containing that book
    //public async Task<bool> CheckBoughtBook(string userId, string bookId)
    //{
    //    // Fix: Replace 'Book' with 'Items' as per the Orders class definition
    //    long count = await _unitOfWork.GetCollection<Order>()
    //        .CountDocumentsAsync(o => o.UserId == userId && o.Items.Any(p => p.BookId == bookId) && o.Status == "PAID");
    //    return count > 0;
    //}

    public async Task CreateReviewAsync(CreateReviewRequest createReviewRequest)
    {
        if (string.IsNullOrEmpty(createReviewRequest.BookId) || string.IsNullOrEmpty(createReviewRequest.UserId))
        {
            throw new BadRequestCustomException("BookId and UserId cannot be null or empty.");
        }

        double rating = await _unitOfWork.GetCollection<Book>()
            .Find(b => b.Id == createReviewRequest.BookId)
            .Project(b => b.Rating)
            .FirstOrDefaultAsync();

        Review review = new()
        {
            BookId = createReviewRequest.BookId,
            UserId = createReviewRequest.UserId,
            Rating = createReviewRequest.Rating,
            Comment = createReviewRequest.Comment,
            CreatedAt = TimeControl.GetUtcPlus7Time(), // Assuming you want to set the current UTC time
        };

        long reviewCount = await _unitOfWork.GetCollection<Book>()
            .Find(b => b.Id == createReviewRequest.BookId)
            .Project(b => b.RatingCount)
            .FirstOrDefaultAsync();

        UpdateDefinition<Book> updateDefinition = Builders<Book>.Update
            .Set(b => b.Rating, (rating * reviewCount + createReviewRequest.Rating) / (reviewCount + 1))
            .Inc(b => b.RatingCount, 1)
            .Set(b => b.UpdatedAt, TimeControl.GetUtcPlus7Time());

        UpdateResult updateResult = await _unitOfWork.GetCollection<Book>()
            .UpdateOneAsync(b => b.Id == createReviewRequest.BookId, updateDefinition);

        await _unitOfWork.GetCollection<Review>().InsertOneAsync(review);
    }

    public async Task DeleteReviewByIdAsync(string id)
    {
        // Xóa mềm
        UpdateResult updateResult = await _unitOfWork.GetCollection<Review>()
            .UpdateOneAsync(r => r.Id == id, Builders<Review>.Update.Set(r => r.IsDeleted, true));

        if (updateResult.ModifiedCount == 0)
        {
            throw new NotFoundCustomException("Delete failed");
        }
    }
}
