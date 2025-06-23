using BookSoulsApp.Application.Mappers;
using BookSoulsApp.Domain.Entities;

namespace BookSoulsApp.Application.Models.Books;
public class BookResponse : IMapFrom<Book>
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Isbn { get; set; }
    public string PublisherId { get; set; }
    public List<string> CategoryIds { get; set; } = [];
    public int ReleaseYear { get; set; }
    public bool IsStricted { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Image { get; set; }
    public double Rating { get; set; }
    public long RatingCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
