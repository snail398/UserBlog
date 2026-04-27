namespace UserBlog.Common.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(
        string errorCode,
        string message,
        int statusCode,
        object? details = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Details = details;
    }

    public string ErrorCode { get; }

    public int StatusCode { get; }

    public object? Details { get; }
}