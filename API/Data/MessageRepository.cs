using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddGroup(Group group)
    {
        _context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.Messages.Remove(message);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _context.Groups
            .Include(g => g.connections)
            .Where(g => g.connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await _context.Groups
            .Include(x => x.connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        // Order by recent send date  
        var query = _context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();
        
        // Filter based on the selected Container (Inbox / Outbox / Unread (default))
        query = messageParams.Container switch 
        {
            "Inbox" => query.Where(u => 
                u.RecipientUserName == messageParams.UserName && u.RecipientDeleted == false),
            "Outbox" => query.Where(u => 
                u.SenderUserName == messageParams.UserName && u.SenderDeleted == false),
            _ => query.Where(u => 
                u.RecipientUserName == messageParams.UserName && u.RecipientDeleted == false 
                && u.DateRead == null)
        };

        // Project these Messages (from Query) into List of MessageDTOs using the implemented AutoMapper configuration 
        var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
        // Use the CreateAsync method implemented for Generic PagedList class to return Paginated result
        return await PagedList<MessageDto>.CreateAsync(messages, messageParams.pageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesThread(string currentUserName, string recipientUserName)
    {
        // Get all messages b/w the current loggedin user and the selected user for thread
        var query = _context.Messages
            .Where(m => 
                (m.SenderUserName == currentUserName && m.RecipientUserName == recipientUserName && m.SenderDeleted == false) 
                || (m.RecipientUserName == currentUserName && m.SenderUserName == recipientUserName && m.RecipientDeleted == false))
            .OrderBy(m => m.MessageSent)
            .AsQueryable();
        // Get Unread messages
        var unreadMessages = query.Where(m => m.DateRead == null
            && m.RecipientUserName == currentUserName).ToList();

        // Set MessageRead Date for the unread messages
        if(unreadMessages.Any())
        {
            foreach(var umessage in unreadMessages)
            {
                umessage.DateRead = DateTime.UtcNow;
            }
        }

        // return _mapper.Map<IEnumerable<MessageDto>>(messages); 
        return await query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveConnection(Connection connection)
    {
        _context.Connections.Remove(connection);
    }
}
