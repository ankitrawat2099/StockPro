using System.Net;
using System.Text.Json;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        int statusCode;

        switch (ex)
        {
            case KeyNotFoundException:
                statusCode = 404;
                break;
            case ArgumentException:
                statusCode = 400;
                break;
            case UnauthorizedAccessException:
                statusCode = 401;
                break;
            case InvalidOperationException:
                statusCode = 409;
                break;
            default:
                statusCode = 500;
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            status = statusCode,
            error = ex.GetType().Name,
            message = ex.Message,
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}