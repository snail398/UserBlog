using UserBlog.Auth.Dtos;
using UserBlog.Users.Dtos;

namespace UserBlog.Auth;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);
}