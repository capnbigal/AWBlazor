using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AWBlazorApp.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    public const string HubUrl = "/hubs/notifications";
}
