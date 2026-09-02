using System.Net;
using System.Text.Json;

namespace AuthService.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unhandled exception occurred.");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            InvalidOperationException => (int)HttpStatusCode.Conflict,

            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,

            _ => (int)HttpStatusCode.InternalServerError
        };

        var response = new
        {
            statusCode,
            message = exception switch
            {
                InvalidOperationException => exception.Message,

                UnauthorizedAccessException =>
                    exception.Message,

                _ => "An unexpected error occurred."
            }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}