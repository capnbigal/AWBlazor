namespace AWBlazorApp.App.Middleware;

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
        // Note: 'unsafe-inline' for style-src is required by MudBlazor (it injects inline styles
        // for component layout). 'unsafe-eval' was previously here but was not required by Blazor;
        // removing it tightens XSS protection.
        //
        // unpkg.com is whitelisted for script-src + style-src so the /analytics/geo page can pull
        // Leaflet + MarkerCluster from their CDN. tile.openstreetmap.org is whitelisted for img-src
        // so the basemap tiles load. If we ever self-host those assets under wwwroot/, these host
        // allowances can be removed.
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' https://unpkg.com; " +
            "style-src 'self' 'unsafe-inline' https://unpkg.com; " +
            "img-src 'self' data: https://*.tile.openstreetmap.org https://unpkg.com; " +
            "connect-src 'self' ws: wss:; font-src 'self' data:; " +
            "frame-ancestors 'none'; base-uri 'self'; form-action 'self';";
        headers["Cross-Origin-Resource-Policy"] = "same-site";
        await next(context);
    }
}
