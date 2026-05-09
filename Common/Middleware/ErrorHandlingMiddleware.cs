using System.Text.Json;
using UserBlog.Common.Constants;
using UserBlog.Common.Exceptions;

namespace UserBlog.Common.Middleware;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException exception)
        {
            await HandleAppExceptionAsync(context, exception);
        }
        catch (Exception exception)
        {
            await HandleUnexpectedExceptionAsync(context, exception);
        }
    }

    private static async Task HandleAppExceptionAsync(HttpContext context, AppException exception)
    {
        await HttpErrorResponseWriter.WriteAsync(context, exception.StatusCode, exception.ErrorCode, exception.Message, exception.Details);
    }

    private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred.");

         await HttpErrorResponseWriter.WriteAsync(context, StatusCodes.Status500InternalServerError, ErrorCodes.InternalServerError, "An unexpected error occurred");
    }
}