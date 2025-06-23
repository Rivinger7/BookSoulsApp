using BookSoulsApp.Application.Mappers;
using BookSoulsApp.Domain.Entities;

namespace BookSoulsApp.Application.Models.Publishers;
public class PublisherResponse : IMapFrom<Publisher>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
