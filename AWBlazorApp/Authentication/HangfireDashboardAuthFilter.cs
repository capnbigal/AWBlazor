using AWBlazorApp.Data;
using Hangfire.Dashboard;

namespace AWBlazorApp.Authentication;

/// <summary>
/// Hangfire dashboard filter that only allows users in the Admin role.
/// </summary>
public sealed class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        return http.User.Identity?.IsAuthenticated == true && http.User.IsInRole(AppRoles.Admin);
    }
}
