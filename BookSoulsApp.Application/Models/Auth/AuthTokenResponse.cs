namespace BookSoulsApp.Application.Models.Auth;
public class AuthTokenResponse
{
    public string AccessToken { get; set; }
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
    public string Avatar { get; set; }
}
