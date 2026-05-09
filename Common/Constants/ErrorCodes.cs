namespace UserBlog.Common.Constants;

public static class ErrorCodes
{
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string ValidationError = "VALIDATION_ERROR";

    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string InvalidAccessToken = "INVALID_ACCESS_TOKEN";
    public const string AccessTokenExpired = "ACCESS_TOKEN_EXPIRED";

    public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
    public const string RefreshTokenRevoked = "REFRESH_TOKEN_REVOKED";
    public const string RefreshTokenExpired = "REFRESH_TOKEN_EXPIRED";

    public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
    public const string UsernameAlreadyExists = "USERNAME_ALREADY_EXISTS";

    public const string PostNotFound = "POST_NOT_FOUND";
}
