namespace UserBlog.Auth.JWT;

public static class JwtClaimNames
{
    public const string Subject = "sub";
    public const string Username = "username";
    public const string TokenId = "tokenId";
    public const string TokenType = "type";

    public const string AccessTokenType = "access";
    public const string RefreshTokenType = "refresh";
}
