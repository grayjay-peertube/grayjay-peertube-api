using GrayjayPeerTube.Domain.Exceptions;

namespace GrayjayPeerTube.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (PeerTubeValidationException ex)
        {
            _logger.LogWarning(ex, "PeerTube validation failed: {Message}", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (UpstreamConfigException ex)
        {
            _logger.LogError(ex, "Upstream config error: {Message}", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new { error = message };
        await context.Response.WriteAsJsonAsync(response);
    }
}
