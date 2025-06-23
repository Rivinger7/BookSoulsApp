using BookSoulsApp.Application.Models.Auth;

namespace BookSoulsApp.Application.ServiceInterfaces;
public interface IAuthenticationService
{
    Task<AuthTokenResponse> LoginAsync(LoginRequest loginRequest);
    Task RegisterAsync(RegisterRequest registerRequest);
}
