using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Hangfire.Dashboard;

namespace AWBlazorApp.Infrastructure.Hangfire;

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
