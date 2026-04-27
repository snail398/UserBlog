namespace UserBlog.Common;

public sealed class ApiResponse<T>
{
    public T Data { get; init; } = default!;

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>
        {
            Data = data
        };
    }
}