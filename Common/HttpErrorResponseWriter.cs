namespace UserBlog.Common;

public static class HttpErrorResponseWriter
{
    public static async Task WriteAsync(HttpContext context, int statusCode, string errorCode, string message, object? details = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var error = ApiError.Create(errorCode, message, details);

        await context.Response.WriteAsJsonAsync(error);
    }
}