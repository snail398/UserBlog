namespace UserBlog.Auth.Dtos;

public sealed class LogoutRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}