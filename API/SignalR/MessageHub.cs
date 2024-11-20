using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.SignalR;

[Authorize]
public class MessageHub(IMessageRepository messageRepository, IUserRepository userRepository
    , IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
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
        // Get group after adding to DB
        var group = await AddToGroup(groupName);
        // Send method to Client app (all connections connected to this groupname) with the updated group
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        // get message thread between these users
        var messages = await messageRepository.GetMessagesThread(Context.User.GetUsername(), otherUser);

        // send message thread to Caller
        await Clients.Caller.SendAsync("RecieveMessageThread", messages);
    }

    //  Override the HUB method, When a user disconnects from HUB
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // Remove current connection from the message group
        var group = await RemoveFromMessageGroup();
        // Send method to client app (to all connections still connected to this groupname)
        await Clients.Group(group.Name).SendAsync("UpdatedGroup");
        await base.OnDisconnectedAsync(exception);
    }

    // To send message from Message Hub (SignalR), instead of using Controller
    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var userName = Context.User.GetUsername();

            if(userName.Equals(createMessageDto.RecipientUserName, StringComparison.OrdinalIgnoreCase))
                throw new HubException("You cannot send messages to yourself");

            // Get Sender
            var sender = await userRepository.GetUserbyNameAsync(userName);
            // Get Recipient
            var recipient = await userRepository.GetUserbyNameAsync(createMessageDto.RecipientUserName);

            if(recipient == null || sender == null || 
                sender.UserName == null || recipient.UserName == null)
                throw new HubException("Cannot send message at this time");

            var message = new Message{
                Sender = sender,
                Recipient = recipient,
                SenderUserName = userName,
                RecipientUserName = createMessageDto.RecipientUserName,
                Content = createMessageDto.Content
            };

            // Check if two users are in same group, so that messages can be marked "read" if both are chatting
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await messageRepository.GetMessageGroup(groupName);
            // If recipient is in the group
            if(group.connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else // to notify users not in Message group, but Online, if they are the recipient
            {
                // Get connections of the recipient user (a user could be connected from multiple devices)
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if(connections != null)
                {
                    // This information will be used in Client app
                    await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new {
                        username = sender.UserName,
                        knownAs = sender.KnownAs
                    });
                }
            }

            messageRepository.AddMessage(message);

            if(await messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string groupName){
        // Check if group already exists
        var group = await messageRepository.GetMessageGroup(groupName);
        // If not
        if(group == null)
        {
            group = new Group(groupName);
            messageRepository.AddGroup(group);
        }

        // Create Connection object using connection details from HUBContext
        var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
        group.connections.Add(connection);
        
        if (await messageRepository.SaveAllAsync())
            return group;

        throw new HubException("Failed to add to group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
        // Get message group, using current connection Id
        var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
        // Get connection from this group
        var connection = group.connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        // Remove connection
        messageRepository.RemoveConnection(connection);
        // Save to DB
        if(await messageRepository.SaveAllAsync())
            return group;

        throw new HubException("Failed to remove from group"); 
    }
}
