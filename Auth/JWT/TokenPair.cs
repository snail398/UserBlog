namespace UserBlog.Auth;

public sealed class TokenPair
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public Guid RefreshTokenId { get; init; }

    public DateTimeOffset RefreshTokenExpiresAt { get; init; }
}