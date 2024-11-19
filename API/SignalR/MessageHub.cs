using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.SignalR;

[Authorize]
public class MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper) : Hub
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
        await AddToGroup(groupName);

        // get message thread between these users
        var messages = await messageRepository.GetMessagesThread(Context.User.GetUsername(), otherUser);

        // send group to clients
        await Clients.Group(groupName).SendAsync("RecieveMessageThread", messages);
    }

    //  Override the HUB method, When a user disconnects from HUB
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await RemoveFromMessageGroup();
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

    private async Task<bool> AddToGroup(string groupName){
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
        
        return await messageRepository.SaveAllAsync();
    }

    private async Task RemoveFromMessageGroup()
    {
        // Get connection
        var connection = await messageRepository.GetConnection(Context.ConnectionId);
        // Remove connection
        messageRepository.RemoveConnection(connection);
        // Save to DB
        await messageRepository.SaveAllAsync();
    }
}
