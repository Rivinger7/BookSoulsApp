using Microsoft.AspNetCore.Http;

namespace BookSoulsApp.Application.Models.Books;
public class UpdateBookRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? PublisherId { get; set; }
    public List<string> CategoryIds { get; set; } = [];
    public int? ReleaseYear { get; set; }
    public bool? IsStricted { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }

    public IFormFile? Image { get; set; }
}
