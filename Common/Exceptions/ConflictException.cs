namespace UserBlog.Common.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(
        string errorCode,
        string message,
        object? details = null)
        : base(errorCode, message, StatusCodes.Status409Conflict, details)
    {
    }
}