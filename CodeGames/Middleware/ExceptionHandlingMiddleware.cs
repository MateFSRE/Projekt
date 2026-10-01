using System.Text.Json;
using CodeGames.Exceptions;

namespace CodeGames.Middleware;

// Neobrađene iznimke postaju JSON. Stack trace ostaje u logu, ne u odgovoru.
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
        catch (AppException exception)
        {
            await WriteMessageAsync(context, exception.StatusCode, exception.Message);
        }
        catch (BadHttpRequestException exception)
        {
            var code = exception.StatusCode;
            var message = code == StatusCodes.Status413PayloadTooLarge
                ? "Zahtjev je prevelik."
                : "Zahtjev nije ispravan.";
            await WriteMessageAsync(context, code, message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Neobrađena iznimka.");
            await WriteMessageAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Dogodila se greška na poslužitelju.");
        }
    }

    private static async Task WriteMessageAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new { message });
        await context.Response.WriteAsync(body);
    }
}
