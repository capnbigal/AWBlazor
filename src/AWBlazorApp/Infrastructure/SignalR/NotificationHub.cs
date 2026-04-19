using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AWBlazorApp.Infrastructure.SignalR;

[Authorize]
public sealed class NotificationHub : Hub
{
    public const string HubUrl = "/hubs/notifications";
}
