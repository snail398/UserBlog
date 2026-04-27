namespace UserBlog.Auth.Dtos;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}