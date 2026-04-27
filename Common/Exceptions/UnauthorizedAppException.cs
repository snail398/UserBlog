namespace UserBlog.Common.Exceptions;

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(
        string errorCode,
        string message,
        object? details = null)
        : base(errorCode, message, StatusCodes.Status401Unauthorized, details)
    {
    }
}