namespace BookSoulsApp.Application.Models.Pagination;
public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
