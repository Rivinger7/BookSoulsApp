using BookSoulsApp.Application.Models.Books;
using BookSoulsApp.Application.Models.Pagination;

namespace BookSoulsApp.Application.ServiceInterfaces;
public interface IBookService
{
    Task CreateBookAsync(CreateBookRequest createBookRequest);
    Task DeleteBookByIdAsync(string id);
    Task<BookResponse> GetBookByIdAsync(string bookId);
    Task<PaginatedResult<BookResponse>> GetBooksAsync(BookFilterRequest bookFilterRequest, int pageIndex = 1, int limit = 10);
    Task UpdateBookByIdAsync(string id, UpdateBookRequest updateBookRequest);
}
