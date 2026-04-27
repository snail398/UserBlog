namespace UserBlog.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string AccessTokenSecret { get; init; } = string.Empty;

    public string RefreshTokenSecret { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; }

    public int RefreshTokenLifetimeDays { get; init; }
}