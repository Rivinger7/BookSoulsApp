using BookSoulsApp.Application.Mappers;
using BookSoulsApp.Domain.Entities;

namespace BookSoulsApp.Application.Models.Categories;
public class CategoryResponse : IMapFrom<Category>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
