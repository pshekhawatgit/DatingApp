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
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;
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
            var sender = await _userRepository.GetUserbyNameAsync(userName);
            // Get Recipient
            var recipient = await _userRepository.GetUserbyNameAsync(createMessageDto.RecipientUserName);

            if(recipient == null)
                return NotFound();

            var message = new Message{
                Sender = sender,
                Recipient = recipient,
                SenderUserName = userName,
                RecipientUserName = createMessageDto.RecipientUserName,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync())
                return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
        {
            // Get username of Loggedin user 
            messageParams.UserName = User.GetUsername();
            // Get messages (paged list) for this user, using the implementation in Message Repository
            var messages = await _messageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(new PaginationHeader(
                messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages
            ));
            return messages;
        }
    }
}
