using BookSoulsApp.Application.Mappers;
using BookSoulsApp.Domain.Entities;

namespace BookSoulsApp.Application.Models.Reviews;
public class ReviewResponse : IMapFrom<Review>
{
    public string Id { get; set; }

    public UserProfileResponse User { get; set; }

    public string BookId { get; set; }
    public double Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UserProfileResponse
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Avatar { get; set; }
}
