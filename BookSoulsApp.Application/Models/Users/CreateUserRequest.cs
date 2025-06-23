using BookSoulsApp.Domain.Entities;

namespace BookSoulsApp.Application.Models.Users;
public class CreateUserRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public Address? Address { get; set; }

    public string Role { get; set; } // "Customer", "Admin", "Staff"
}
