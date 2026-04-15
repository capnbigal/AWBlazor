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
}
