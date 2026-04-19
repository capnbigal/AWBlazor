using AWBlazorApp.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace AWBlazorApp.Features.Insights.Services;

public sealed class NotificationService(IHubContext<NotificationHub> hubContext)
{
    public async Task NotifyAsync(string message, string? excludeConnectionId = null)
    {
        var clients = excludeConnectionId is not null
            ? hubContext.Clients.AllExcept(excludeConnectionId)
            : hubContext.Clients.All;
        await clients.SendAsync("Notify", message);
    }

    /// <summary>
    /// Push a message to every active connection owned by a specific user. The SignalR hub's
    /// UserIdentifier is mapped from ClaimTypes.NameIdentifier, which matches AspNetUsers.Id.
    /// </summary>
    public Task NotifyUserAsync(string userId, string message) =>
        hubContext.Clients.User(userId).SendAsync("Notify", message);

    /// <summary>
    /// Tells every connected client that something changed in the named source module so any
    /// open dashboard / list page can choose to invalidate its cache and reload. Sent as the
    /// "DashboardChanged" SignalR event with the source module as the payload.
    /// Fire-and-forget — failures are swallowed because a missed dashboard refresh is not
    /// worth bubbling up to the caller's transactional path.
    /// </summary>
    public Task NotifyDashboardChangedAsync(string sourceModule)
    {
        try { return hubContext.Clients.All.SendAsync("DashboardChanged", sourceModule); }
        catch { return Task.CompletedTask; }
    }
}
