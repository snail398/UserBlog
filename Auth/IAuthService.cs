using UserBlog.Auth.Dtos;
using UserBlog.Users.Dtos;

namespace UserBlog.Auth;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<LogoutResponse> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}