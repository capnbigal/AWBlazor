namespace ElementaryApp.Startup;

/// <summary>
/// Adds standard security headers to every HTTP response.
/// Replaces the inline lambda that was previously in Program.cs.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Content-Security-Policy"] =
            "default-src 'self'; script-src 'self' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; connect-src 'self' ws: wss:; font-src 'self' data:;";
        await next(context);
    }
}
