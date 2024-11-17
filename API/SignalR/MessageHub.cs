using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

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

            messageRepository.AddMessage(message);

            if(await messageRepository.SaveAllAsync())
            {
                var groupName = GetGroupName(sender.UserName , recipient.UserName);
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
    }

    private string GetGroupName(string caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

}
