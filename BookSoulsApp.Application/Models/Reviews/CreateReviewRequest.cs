namespace BookSoulsApp.Application.Models.Reviews;
public class CreateReviewRequest
{
    public string BookId { get; set; }
    public string UserId { get; set; }
    public double Rating { get; set; }
    public string Comment { get; set; }
}
