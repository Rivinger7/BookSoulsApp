using BookSoulsApp.Application.Models.Auth;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Exceptions;
using BookSoulsApp.Domain.Utils;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Security.Claims;

namespace BookSoulsApp.Infrastructure.Services;
public class AuthenticationService(IUnitOfWork unitOfWork, IJsonWebToken jsonWebToken, IHttpContextAccessor httpContextAccessor) : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IJsonWebToken _jsonWebToken = jsonWebToken;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    private bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public async Task RegisterAsync(RegisterRequest registerRequest)
    {
        string email = registerRequest.Email;
        string password = registerRequest.Password;
        string confirmPassword = registerRequest.ConfirmPassword;
        string fullName = registerRequest.FullName;
        string phoneNumber = registerRequest.PhoneNumber;
        Address address = registerRequest.Address;

        // Kiểm tra mật khẩu xác nhận
        bool isConfirmedPassword = password == confirmPassword;
        if (!isConfirmedPassword)
        {
            throw new BadRequestCustomException("Password and Confirm Password do not match");
        }

        // Kiểm tra account đã tồn tại hay chưa
        if (await IsEmailExisted(email))
        {
            throw new ConflictCustomException("Account already exists");
        }

        if (await IsPhoneNumberExisted(phoneNumber))
        {
            throw new ConflictCustomException("Phone number already exists");
        }

        User user = new()
        {
            Email = email,
            Password = HashPassword(password),
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Address = address,
            Avatar = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730097883/60d5dc467b950c5ccc8ced95_spotify-for-artists_on4me9.jpg",
            Role = "Customer", // Mặc định là Customer

            CreatedAt = TimeControl.GetUtcPlus7Time(),
        };

        await _unitOfWork.GetCollection<User>().InsertOneAsync(user);
    }

    private async Task<bool> IsEmailExisted(string email)
    {
        return await _unitOfWork.GetCollection<User>()
            .Find(user => user.Email == email)
            .Project(user => user.Email)
            .AnyAsync();
    }

    private async Task<bool> IsPhoneNumberExisted(string phoneNumber)
    {
        return await _unitOfWork.GetCollection<User>()
            .Find(user => user.PhoneNumber == phoneNumber)
            .Project(user => user.PhoneNumber)
            .AnyAsync();
    }

    public async Task<AuthTokenResponse> LoginAsync(LoginRequest loginRequest)
    {
        string email = loginRequest.Email;
        string password = loginRequest.Password;

        User user = await _unitOfWork.GetCollection<User>()
            .Find(u => u.Email == email && !u.IsDeleted)
            .FirstOrDefaultAsync() ?? throw new NotFoundCustomException("User not found");

        if (!VerifyPassword(password, user.Password))
        {
            throw new BadRequestCustomException("Invalid password");
        }

        // Tạo JWT token
        IEnumerable<Claim> claims =
        [
            new Claim("Id", user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim("Avatar", user.Avatar),
            new Claim(ClaimTypes.Role, user.Role)
        ];

        string accessToken = _jsonWebToken.GenerateAccessToken(claims);

        return new AuthTokenResponse
        {
            AccessToken = accessToken,
            Id = user.Id,
            FullName = user.FullName,
            Avatar = user.Avatar,
            Role = user.Role
        };
    }
}
