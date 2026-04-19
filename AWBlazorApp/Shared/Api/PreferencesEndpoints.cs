namespace AWBlazorApp.Shared.Api;

public static class PreferencesEndpoints
{
    public static void MapPreferencesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Dark mode toggle — sets a cookie and redirects back. No JS needed.
        endpoints.MapGet("/api/toggle-dark-mode", (HttpContext ctx) =>
        {
            var current = ctx.Request.Cookies["darkMode"] == "true";
            var newValue = !current;
            ctx.Response.Cookies.Append("darkMode", newValue.ToString().ToLowerInvariant(), new CookieOptions
            {
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.FromDays(365),
                IsEssential = true,
            });
            var returnUrl = ctx.Request.Query["returnUrl"].FirstOrDefault() ?? "/";
            return Results.LocalRedirect($"~{returnUrl}");
        });
    }
}
