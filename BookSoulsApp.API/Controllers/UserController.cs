using BookSoulsApp.Application.Models.Users;
using BookSoulsApp.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSoulsApp.API.Controllers;
[Route("api/users")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [Authorize(Roles = "Admin"), HttpGet]
    public async Task<IActionResult> GetUsers(int pageIndex = 1, int limit = 10)
    {
        var users = await _userService.GetUsersAsync(pageIndex, limit);
        return Ok(users);
    }

    [Authorize(Roles = "Admin"), HttpGet("{id}")]
    public async Task<IActionResult> GetUserProfileById(string id)
    {
        var userProfile = await _userService.GetUserProfileByIdAsync(id);
        return Ok(userProfile);
    }

    [Authorize(Roles = "Customer,Admin,Staff"), HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        var userProfile = await _userService.GetUserProfileAsync();
        return Ok(userProfile);
    }

    [Authorize(Roles = "Admin"), HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest createUserRequest)
    {
        await _userService.CreateUserAsync(createUserRequest);
        return Ok(new { Message = "Created user successfully" });
    }

    [Authorize(Roles = "Customer,Staff"), HttpPut("edit-profile")]
    public async Task<IActionResult> EditProfile(UpdateUserRequest updateUserRequest)
    {
        await _userService.EditProfileAsync(updateUserRequest);
        return NoContent();
    }

    [Authorize(Roles = "Customer"), HttpPatch("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        await _userService.ChangePasswordAsync(changePasswordRequest.NewPassword, changePasswordRequest.OldPassword);
        return Ok(new { Message = "Changed password successfully" });
    }

    [Authorize(Roles = "Admin"), HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserById(string id)
    {
        await _userService.DeleteUserByIdAsync(id);
        return NoContent();
    }
}
