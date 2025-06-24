namespace BookSoulsApp.Application.Models.Reviews;
public class ReviewFilterRequest
{
    public string? BookId { get; set; }
    public string? UserId { get; set; }
    public double? MinRating { get; set; }
    public double? MaxRating { get; set; }
    public string? Comment { get; set; } // For searching by content or user name
    public DateTime? FromDate { get; set; } // Filter reviews created after this date
    public DateTime? ToDate { get; set; } // Filter reviews created before this date
}
