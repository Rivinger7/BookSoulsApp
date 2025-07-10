using AutoMapper;
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.Models.Users;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Application.ThirdPartyServiceInterfaces.Cloudinary;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Enums;
using BookSoulsApp.Domain.Exceptions;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace BookSoulsApp.Infrastructure.Services;
public class UserService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService, IHttpContextAccessor httpContextAccessor) : IUserService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ICloudinaryService _cloudinaryService = cloudinaryService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<PaginatedResult<UserReponse>> GetUsersAsync(int pageIndex, int limit)
    {
        // Lấy danh sách người dùng từ DB với phân trang
        IEnumerable<User> users = await _unitOfWork.GetCollection<User>()
            .Find(_ => true)
            .Skip((pageIndex - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        // Chuyển đổi sang danh sách DTO
        IEnumerable<UserReponse> userAccountsDto = _mapper.Map<IEnumerable<UserReponse>>(users);

        return new PaginatedResult<UserReponse>
        {
            Items = userAccountsDto,
            TotalCount = userAccountsDto.Count()
        };
    }

    public async Task<UserReponse> GetUserProfileAsync()
    {
        // UserID lấy từ phiên người dùng có thể là FE hoặc BE
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;

        // Kiểm tra UserId
        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("Your session is limit, you must login again to edit profile!");
        }

        // Lấy thông tin người dùng từ DB
        User user = await _unitOfWork.GetCollection<User>().Find(user => user.Id == userId).FirstOrDefaultAsync() ?? throw new Exception("User not found");

        // Chuyển đổi sang Response
        UserReponse userProfileResponse = _mapper.Map<UserReponse>(user);

        return userProfileResponse;
    }

    public async Task<UserReponse> GetUserProfileByIdAsync(string id)
    {
        // Kiểm tra UserId
        if (string.IsNullOrEmpty(id))
        {
            throw new ValidationCustomException("Invalid UserId parameters");
        }

        // Lấy thông tin người dùng từ DB
        User user = await _unitOfWork.GetCollection<User>().Find(user => user.Id == id).FirstOrDefaultAsync() ?? throw new Exception("User not found");

        // Chuyển đổi sang Response
        UserReponse userProfileResponse = _mapper.Map<UserReponse>(user);

        return userProfileResponse;
    }

    public async Task CreateUserAsync(CreateUserRequest createUserRequest)
    {
        // Kiểm tra mật khẩu xác nhận
        bool isConfirmedPassword = createUserRequest.Password == createUserRequest.ConfirmPassword;
        if (!isConfirmedPassword)
        {
            throw new BadRequestCustomException("Password and Confirm Password do not match");
        }

        // Mã hóa mật khẩu
        string hashedPassword = HashPassword(createUserRequest.Password);


        if (await IsEmailExisted(createUserRequest.Email))
        {
            throw new ConflictCustomException("Account already exists");
        }

        if (await IsPhoneNumberExisted(createUserRequest.PhoneNumber))
        {
            throw new ConflictCustomException("Phone number already exists");
        }


        // Tạo người dùng mới
        User newUser = new()
        {
            Email = createUserRequest.Email,
            Password = hashedPassword,
            FullName = createUserRequest.FullName,
            PhoneNumber = createUserRequest.PhoneNumber,
            Avatar = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730097883/60d5dc467b950c5ccc8ced95_spotify-for-artists_on4me9.jpg",
            Role = createUserRequest.Role
        };

        await _unitOfWork.GetCollection<User>().InsertOneAsync(newUser);
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

    public async Task EditProfileAsync(UpdateUserRequest updateUserRequest)
    {
        // UserID lấy từ phiên người dùng có thể là FE hoặc BE
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;

        // Kiểm tra UserId
        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("Your session is limit, you must login again to edit profile!");
        }

        User user = await _unitOfWork.GetCollection<User>().Find(user => user.Id == userId).FirstOrDefaultAsync() ?? throw new Exception("User not found");

        // Build update definition
        UpdateDefinitionBuilder<User> updateBuilder = Builders<User>.Update;
        List<UpdateDefinition<User>> updates = [];

        if (!string.IsNullOrEmpty(updateUserRequest.FullName))
        {
            updates.Add(updateBuilder.Set(u => u.FullName, updateUserRequest.FullName));
        }
        if (!string.IsNullOrEmpty(updateUserRequest.PhoneNumber))
        {
            updates.Add(updateBuilder.Set(u => u.PhoneNumber, updateUserRequest.PhoneNumber));
        }
        if (!string.IsNullOrEmpty(updateUserRequest.Gender))
        {
            updates.Add(updateBuilder.Set(u => u.Gender, updateUserRequest.Gender));
        }

        if (!string.IsNullOrEmpty(updateUserRequest.Address?.Ward))
        {
            updates.Add(updateBuilder.Set(u => u.Address.Ward, updateUserRequest.Address.Ward));
        }

        if (!string.IsNullOrEmpty(updateUserRequest.Address?.Street))
        {
            updates.Add(updateBuilder.Set(u => u.Address.Street, updateUserRequest.Address.Street));
        }

        if (!string.IsNullOrEmpty(updateUserRequest.Address?.District))
        {
            updates.Add(updateBuilder.Set(u => u.Address.District, updateUserRequest.Address.District));
        }

        if (!string.IsNullOrEmpty(updateUserRequest.Address?.City))
        {
            updates.Add(updateBuilder.Set(u => u.Address.City, updateUserRequest.Address.City));
        }

        if (!string.IsNullOrEmpty(updateUserRequest.Address?.Country))
        {
            updates.Add(updateBuilder.Set(u => u.Address.Country, updateUserRequest.Address.Country));
        }

        if (updateUserRequest.Avatar is not null)
        {
            ImageUploadResult upload = _cloudinaryService.UploadImage(updateUserRequest.Avatar, ImageTag.Users_Profile);

            updates.Add(updateBuilder.Set(u => u.Avatar, upload.SecureUrl.AbsoluteUri ?? user.Avatar));
        }

        UpdateDefinition<User> updateDefinition = updateBuilder.Combine(updates);

        // Cập nhật thông tin người dùng
        UpdateResult updateResult = await _unitOfWork.GetCollection<User>().UpdateOneAsync(u => u.Id == userId, updateDefinition);

        if (updateResult.ModifiedCount == 0)
        {
            throw new BadRequestCustomException("Update failed");
        }
    }

    public async Task DeleteUserByIdAsync(string id)
    {
        // Mark the book as deleted
        UpdateResult updateResult = await _unitOfWork.GetCollection<User>()
            .UpdateOneAsync(c => c.Id == id, Builders<User>.Update.Set(c => c.IsDeleted, true));

        if (updateResult.ModifiedCount == 0)
        {
            throw new NotFoundCustomException("Delete failed");
        }
    }

    public async Task ChangePasswordAsync(string newPassword, string oldPassword)
    {
        // UserID lấy từ phiên người dùng có thể là FE hoặc BE
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;

        // Kiểm tra UserId
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedCustomException("Your session is limit, you must login again to edit profile!");
        }

        // Kiểm tra tài khoản có tồn tại hay không
        User user = await _unitOfWork.GetCollection<User>()
            .Find(user => user.Id == userId)
            .FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Account does not exist");

        // Kiểm tra mật khẩu cũ có đúng hay không
        bool isOldPasswordValid = VerifyPassword(oldPassword, user.Password);
        if (!isOldPasswordValid)
        {
            throw new BadRequestCustomException("Invalid old password");
        }

        // Mã hóa mật khẩu mới
        string hashedNewPassword = HashPassword(newPassword);

        // Cập nhật mật khẩu mới cho người dùng
        var updateDefinition = Builders<User>.Update.Set(user => user.Password, hashedNewPassword);
        await _unitOfWork.GetCollection<User>().UpdateOneAsync(user => user.Id == userId, updateDefinition);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    private bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
