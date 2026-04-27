namespace UserBlog.Common.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(
        string errorCode,
        string message,
        object? details = null)
        : base(errorCode, message, StatusCodes.Status403Forbidden, details)
    {
    }
}