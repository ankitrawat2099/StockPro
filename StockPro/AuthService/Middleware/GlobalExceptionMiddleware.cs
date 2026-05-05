using System.Net;
using System.Text.Json;

namespace AuthService.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static Task HandleException(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        switch (ex)
{
    case UnauthorizedAccessException:
        context.Response.StatusCode = 401;
        break;

    case KeyNotFoundException:
        context.Response.StatusCode = 404;
        break;

    default:
        context.Response.StatusCode = 500;
        break;
}

        var result = JsonSerializer.Serialize(new
        {
            success = false,
            message = ex.Message
        });

        return context.Response.WriteAsync(result);
    }
}