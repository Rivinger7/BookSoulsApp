using BookSoulsApp.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace BookSoulsApp.Application.Models.Users;
public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public IFormFile? Avatar { get; set; }
    public Address? Address { get; set; }
}
