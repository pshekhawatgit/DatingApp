using System;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub(IMessageRepository messageRepository) : Hub
{
    // Override the HUB method, When a user connects to HUB
    public override async Task OnConnectedAsync()
    {
        // get http context
        var httpcontext = Context.GetHttpContext();
        // get other user from query strings
        var otherUser = httpcontext.Request.Query["user"];
        // check if both user are not null
        if(Context.User == null || string.IsNullOrEmpty(otherUser))
            throw new Exception("Cannot join group. User details missing.");
        // get groupname and add to groups
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        // get message thread between these users
        var messages = await messageRepository.GetMessagesThread(Context.User.GetUsername(), otherUser);

        // send group to clients
        await Clients.Group(groupName).SendAsync("RecieveMessageThread", messages);
    }

    //  Override the HUB method, When a user disconnects from HUB
    public override Task OnDisconnectedAsync(Exception exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    private string GetGroupName(string caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

}
