using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Services;

namespace AWBlazorApp.Startup;

/// <summary>
/// Middleware that enforces area-based permissions on all <c>/api/</c> routes.
/// Runs after <c>UseAuthorization()</c>. Resolves the area from the route,
/// maps the HTTP method to a required permission level, and returns 403 if denied.
/// Routes that don't map to any area pass through (backward compatible).
/// </summary>
public sealed class AreaPermissionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Only enforce on API routes
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Skip unauthenticated requests (let auth middleware handle 401)
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            await next(context);
            return;
        }

        // Resolve area from route
        var area = PermissionAreaMapping.ResolveFromApiRoute(path);
        if (area is null)
        {
            // No area maps — pass through (e.g., /api/hello, /api/chart-export, /api/toggle-dark-mode)
            await next(context);
            return;
        }

        // Map HTTP method to required level
        var requiredLevel = PermissionAreaMapping.RequiredLevelForMethod(context.Request.Method);

        // Check permission
        var permissionService = context.RequestServices.GetRequiredService<IPermissionService>();
        var hasPermission = await permissionService.HasPermissionAsync(userId, area.Value, requiredLevel);

        if (!hasPermission)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\":\"Insufficient permissions for {area.Value} ({requiredLevel})\"}}");
            return;
        }

        await next(context);
    }
}
