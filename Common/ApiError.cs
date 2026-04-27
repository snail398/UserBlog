namespace UserBlog.Common;

public sealed class ApiError
{
    public string Error { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public object? Details { get; init; }

    public static ApiError Create(
        string error,
        string message,
        object? details = null)
    {
        return new ApiError
        {
            Error = error,
            Message = message,
            Details = details
        };
    }
}