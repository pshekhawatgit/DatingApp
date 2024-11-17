using System;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub : Hub
{
    private readonly PresenceTracker _tracker;

    public PresenceHub(PresenceTracker tracker)
    {
        _tracker = tracker;
    }
    // Override the HUB method, When a user connects to HUB
    public override async Task OnConnectedAsync()
    {
        await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);

        // invoke methods on Clients connected to this HUB
        await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
        // Get currently online users
        var currentUsers = await _tracker.GetOnlineUsers();
        // Notify all connected clients
        await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
    }

    //  Override the HUB method, When a user disconnects from HUB
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await _tracker.UserDisonnected(Context.User.GetUsername(), Context.ConnectionId);

        await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

        // Get currently online users
        var currentUsers = await _tracker.GetOnlineUsers();
        // Notify all connected clients
        await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        
        await base.OnDisconnectedAsync(exception);
    }
}
