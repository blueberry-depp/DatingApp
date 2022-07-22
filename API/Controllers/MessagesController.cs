﻿using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
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
        // Return MessageDto to the user.
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUserName();

            // Check to see if our user name is equal to the recipient name in the CreateMessageDto.
            if (username == createMessageDto.RecipientUsername.ToLower())
            {
                return BadRequest("You cannot send messages to yourself");
            }

            // Get hold of both of our users in the sender and the recipient as we need to populate the message.
            // Get the sender user. 
            var sender = await _userRepository.GetUserByUsernameAsync(username);
            // Get the recipient user.
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            // If it's not found, then we're going to return not found because we could not find the user.
            if (recipient == null) return NotFound();

            // Create the message.
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            // What we want to return from this is a MessageDto. So we will also need to bring in IMapper into what we're doing here.
            _messageRepository.AddMessage(message);

            // Map from message.
            if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

            // If we don't manage to save it, return a bad request.
            return BadRequest("Failed to send message");
        }


        // No parameters else because everything else is going to come up by a query string, 
        [HttpGet]
        // we're returning is an IEnumerable of type MessageDto because we're adding the paging onto the header.
        // Create unread messages inbox and outbox messaging system.
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();
            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            // Add pagination header.
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return Ok(messages);
        }

        // Give this root parameter.
        // username: username of the other user.
        // Remember, we've always got access to the current user name inside controllers. So we only need to know who the other
        // participant in this conversation is.
        // string username: is the other user.
        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            // Get current username.
            var currentUsername = User.GetUserName();

            return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
        }


        // We don't return anything from deletions.
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();

            // The message must include the sender and recipient.
            var message = await _messageRepository.GetMessage(id);

            // Check if either the sender username or the recipient username is not equal to the username,
            // then this message has nothing to do with that user.
            if (message.Sender.UserName != username && message.Recipient.UserName != username) return Unauthorized();
            
            if (message.Sender.UserName == username) message.SenderDeleted = true;
            if (message.Recipient.UserName == username) message.RecipientDeleted = true;

            // Check if the sender and recipient deleted the message, then we deleted from the server.
            if (message.SenderDeleted && message.RecipientDeleted) _messageRepository.DeleteMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok();

            // If doesn't work then return bad request.
            return BadRequest("Problem deleting the message");



        }






    }
}
