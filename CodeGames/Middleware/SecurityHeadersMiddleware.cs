namespace CodeGames.Middleware;

// Zaglavlja koja preglednik traži od API-ja. Swagger u razvoju ostaje bez strogog CSP-a.
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        if (!context.Request.Path.StartsWithSegments("/swagger"))
        {
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        }

        await _next(context);
    }
}
