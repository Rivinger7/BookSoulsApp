using BookSoulsApp.Application.Models.Reviews;
using BookSoulsApp.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSoulsApp.API.Controllers;
[Route("api/reviews")]
[ApiController]
public class ReviewController(IReviewService reviewService) : ControllerBase
{
    private readonly IReviewService _reviewService = reviewService;

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> GetReviews([FromQuery] ReviewFilterRequest reviewFilterRequest, [FromQuery] int pageIndex = 1, [FromQuery] int limit = 10)
    {
        var result = await _reviewService.GetReviewsAsync(reviewFilterRequest, pageIndex, limit);
        return Ok(new { Message = "Reviews retrieved successfully", result });
    }

    [AllowAnonymous, HttpGet("{id}")]
    public async Task<IActionResult> GetReviewById(string id)
    {
        var result = await _reviewService.GetReviewByIdAsync(id);
        return Ok(new { message = $"Review with ID {id} retrieved successfully", result });
    }

    //[AllowAnonymous, HttpGet("confirmation")]
    //public async Task<IActionResult> CheckBoughtBook([FromQuery] string userId, [FromQuery] string productId)
    //{
    //    bool hasBought = await _reviewService.CheckBoughtBook(userId, productId);
    //    return Ok(new { message = hasBought });
    //}

    [Authorize(Roles = "Customer"), HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest createReviewRequest)
    {
        await _reviewService.CreateReviewAsync(createReviewRequest);
        return Ok(new { Message = "Created review successfully" });
    }

    [Authorize(Roles = "Admin"), HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(string id)
    {
        await _reviewService.DeleteReviewByIdAsync(id);
        return Ok(new { Message = "Deleted review successfully" });
    }
}
