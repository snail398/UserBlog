namespace UserBlog.Common.Exceptions;

public sealed class ValidationAppException : AppException
{
    public ValidationAppException(string message, object? details = null) : base("VALIDATION_ERROR", message, StatusCodes.Status400BadRequest, details)
    {
    }
}