namespace UserBlog.Common.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(
        string errorCode,
        string message,
        object? details = null)
        : base(errorCode, message, StatusCodes.Status404NotFound, details)
    {
    }
}