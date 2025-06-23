using BookSoulsApp.Application.Models.Auth;
using BookSoulsApp.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSoulsApp.API.Controllers;
[Route("api/authentication")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
public class AuthenticationController(IAuthenticationService authenticationService) : ControllerBase
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        var token = await _authenticationService.LoginAsync(loginRequest);
        return Ok(new { Token = token });
    }

    [AllowAnonymous, HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        await _authenticationService.RegisterAsync(registerRequest);
        return Ok(new { Message = "User registered successfully" });
    }
}
