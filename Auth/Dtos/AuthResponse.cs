using UserBlog.Users.Dtos;

namespace UserBlog.Auth.Dtos;

public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public UserResponse User { get; init; } = null!;
}