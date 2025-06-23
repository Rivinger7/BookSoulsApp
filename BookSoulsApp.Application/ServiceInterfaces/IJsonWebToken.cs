using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookSoulsApp.Application.ServiceInterfaces;
public interface IJsonWebToken
{
    string GenerateAccessToken(IEnumerable<Claim> claims);

    ClaimsPrincipal ValidateToken(string token);

    JwtSecurityToken DecodeToken(string token);
}
