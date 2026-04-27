using UserBlog.Users.Dtos;

namespace UserBlog.Users;

public interface IUserService
{
    Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}