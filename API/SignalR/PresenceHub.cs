using System;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub : Hub
{
    // Override the HUB method, When a user connects to HUB
    public override async Task OnConnectedAsync()
    {
        // invoke methods on Clients connected to this HUB
        await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
    }

    //  Override the HUB method, When a user disconnects from HUB
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
        await base.OnDisconnectedAsync(exception);
    }
}
