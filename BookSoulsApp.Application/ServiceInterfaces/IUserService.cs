using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.Models.Users;

namespace BookSoulsApp.Application.ServiceInterfaces;
public interface IUserService
{
    Task ChangePasswordAsync(string newPassword, string oldPassword);
    Task CreateUserAsync(CreateUserRequest createUserRequest);
    Task DeleteUserByIdAsync(string id);
    Task EditProfileAsync(UpdateUserRequest updateUserRequest);
    Task<UserReponse> GetUserProfileAsync();
    Task<UserReponse> GetUserProfileByIdAsync(string id);
    Task<PaginatedResult<UserReponse>> GetUsersAsync(int pageIndex, int limit);
}
