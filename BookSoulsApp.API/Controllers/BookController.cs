using BookSoulsApp.Application.Models.Books;
using BookSoulsApp.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSoulsApp.API.Controllers;
[Route("api/books")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
public class BookController(IBookService bookService) : ControllerBase
{
    private readonly IBookService _bookService = bookService;

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> GetBooks([FromQuery] BookFilterRequest bookFilterRequest, [FromQuery] int pageIndex = 1, [FromQuery] int limit = 10)
    {
        var result = await _bookService.GetBooksAsync(bookFilterRequest, pageIndex, limit);
        return Ok(new { message = "Books retrieved successfully", result });
    }

    [AllowAnonymous, HttpGet("{id}")]
    public async Task<IActionResult> GetBookById(string id)
    {
        var result = await _bookService.GetBookByIdAsync(id);
        return Ok(new { message = $"Book with ID {id} retrieved successfully", result });
    }

    [Authorize(Roles = "Staff"), HttpPost]
    public async Task<IActionResult> CreateBook(CreateBookRequest createBookRequest)
    {
        await _bookService.CreateBookAsync(createBookRequest);
        return Ok(new { Message = "Created book Successfully" });
    }

    [Authorize(Roles = "Staff"), HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(string id, UpdateBookRequest updateBookRequest)
    {
        await _bookService.UpdateBookByIdAsync(id, updateBookRequest);
        return Ok(new { Message = "Updated book Successfully" });
    }

    [Authorize(Roles = "Staff"), HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(string id)
    {
        await _bookService.DeleteBookByIdAsync(id);
        return Ok(new { Message = "Deleted book Successfully" });
    }
}
