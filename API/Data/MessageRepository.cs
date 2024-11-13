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
    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.Messages.Remove(message);
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        // Order by recent send date  
        var query = _context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();
        
        // Filter based on the selected Container (Inbox / Outbox / Unread (default))
        query = messageParams.Container switch 
        {
            "Inbox" => query.Where(u => u.RecipientUserName == messageParams.UserName),
            "Outbox" => query.Where(u => u.SenderUserName == messageParams.UserName),
            _ => query.Where(u => u.RecipientUserName == messageParams.UserName 
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
        var messages = await _context.Messages
            .Include(u => u.Sender).ThenInclude(s => s.Photos)
            .Include(u => u.Recipient).ThenInclude(r => r.Photos)
            .Where(m => 
                (m.SenderUserName == currentUserName && m.RecipientUserName == recipientUserName) 
                || (m.RecipientUserName == currentUserName && m.SenderUserName == recipientUserName))
            .OrderBy(m => m.MessageSent)
            .ToListAsync();
        // Get Unread messages
        var unreadMessages = messages.Where(m => m.DateRead == null
            && m.RecipientUserName == currentUserName).ToList();

        // Set MessageRead Date for the unread messages
        if(unreadMessages.Any())
        {
            foreach(var umessage in unreadMessages)
            {
                umessage.DateRead = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        return _mapper.Map<IEnumerable<MessageDto>>(messages); 
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
