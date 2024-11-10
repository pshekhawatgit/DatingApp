using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;

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

    public Task<IEnumerable<MessageDto>> GetMessagesThread(int currentUserId, int recipientId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
