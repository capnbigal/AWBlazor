using ElementaryApp.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ElementaryApp.Services;

public sealed class NotificationService(IHubContext<NotificationHub> hubContext)
{
    public async Task NotifyAsync(string message, string? excludeConnectionId = null)
    {
        var clients = excludeConnectionId is not null
            ? hubContext.Clients.AllExcept(excludeConnectionId)
            : hubContext.Clients.All;
        await clients.SendAsync("Notify", message);
    }
}
