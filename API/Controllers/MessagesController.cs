using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> AddMessage(CreateMessageDto createMessageDto)
        {
            // Get User name
            var userName = User.GetUsername();

            if(userName.Equals(createMessageDto.RecipientUserName, StringComparison.OrdinalIgnoreCase))
                BadRequest("You cannot send messages to yourself");

            // Get Sender
            var sender = await _uow.UserRepository.GetUserbyNameAsync(userName);
            // Get Recipient
            var recipient = await _uow.UserRepository.GetUserbyNameAsync(createMessageDto.RecipientUserName);

            if(recipient == null)
                return NotFound();

            var message = new Message{
                Sender = sender,
                Recipient = recipient,
                SenderUserName = userName,
                RecipientUserName = createMessageDto.RecipientUserName,
                Content = createMessageDto.Content
            };

            _uow.MessageRepository.AddMessage(message);

            if(await _uow.Complete())
                return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
        {
            // Get username of Loggedin user 
            messageParams.UserName = User.GetUsername();
            // Get messages (paged list) for this user, using the implementation in Message Repository
            var messages = await _uow.MessageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(new PaginationHeader(
                messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages
            ));
            return messages;
        }

        //// Commented because this is taken care by SignalR now
        // [HttpGet("thread/{username}")]
        // public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        // {
        //     // Get current logged in user
        //     var currentUsername = User.GetUsername();
        //     // Return message thread
        //     return Ok(await _uow.MessageRepository.GetMessagesThread(currentUsername, username));
        // }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            // Get username
            var username = User.GetUsername();
            // Get message from repository using id
            var message = await _uow.MessageRepository.GetMessage(id);
            // Check if User is neither the sender nor receiver of this message
            if(message.SenderUserName != username && message.RecipientUserName != username)
                return Unauthorized();

            // check if message is getting deleted by Sender or receiver 
            if(message.SenderUserName == username)
                message.SenderDeleted = true;
            if(message.RecipientUserName == username)
                message.RecipientDeleted = true;

            // Check if both sender and receiver have deleted this message, if so Delete message
            if(message.SenderDeleted && message.RecipientDeleted)
                _uow.MessageRepository.DeleteMessage(message);

            // save changes to DB
            if(await _uow.Complete())
                return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}
