using UserBlog.Common.Constants;

namespace UserBlog.Common.Exceptions;

public sealed class ValidationAppException : AppException
{
    public ValidationAppException(string message, object? details = null) : base(ErrorCodes.ValidationError, message, StatusCodes.Status400BadRequest, details)
    {
    }
}