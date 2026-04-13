using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ElementaryApp.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    public const string HubUrl = "/hubs/notifications";
}
