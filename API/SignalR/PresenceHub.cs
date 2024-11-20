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
        if(Context.User == null)
            throw new HubException("Cannot get current user claim");

        var isOnline = await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);

        if(isOnline)
        // invoke methods on Clients connected to this HUB, only is user came online
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

        // Get currently online users
        var currentUsers = await _tracker.GetOnlineUsers();
        // Notify the caller of all online users
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }

    //  Override the HUB method, When a user disconnects from HUB
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if(Context.User == null)
            throw new HubException("Cannot get current user claim");
            
        var isOffline = await _tracker.UserDisonnected(Context.User.GetUsername(), Context.ConnectionId);

        if(isOffline)
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

        // // Get currently online users
        // var currentUsers = await _tracker.GetOnlineUsers();
        // // Notify all connected clients
        // await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

        await base.OnDisconnectedAsync(exception);
    }
}
