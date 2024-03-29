﻿using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
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
            // If we want to get access to the related entities, then we even need to project or we need to eagerly load the
            // related entities and we're not going to project inside here.
            // NOTE: we can't use Include with a FindAsync.
            return await _context.Messages
                .Include(u => u.Sender)
                .Include(u => u.Recipient)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                // Order them by most recent.
                .OrderByDescending(m => m.MessageSent)
                // We projected this query to dto, then it would not need to select so much from the database when it makes these web queries.
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .AsQueryable();

            // Check the container. And depending on which container it is, will depend on which messages we return.
            query = messageParams.Container switch
            {   // if we're the recipient of a message and we've read it, then this is what is gonna to go back from this one.
                // u.RecipientDeleted == false: add extra check to see if a user who's sent a message deletes that message
                // sent, then we don't want to send that back. So their see it in their outbox. So we're only
                // returning messages that the recipient has not deleted or returning messages that have not been deleted.
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && u.SenderDeleted == false),
                // u.DateRead == null: which will mean that they have not read the message yet.
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
            };

            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        // We're going to get the messages for both sides of the conversation, and
        // we'd also do inside here is we'll take the opportunity to mark any of the messages that have not currently been
        // read and begin to mark them as read. What it does mean is that we're going to need
        // to get the messages in memory, then do something with the messages. Then we're going to have to map to a DTO,
        // so we can't work with DTO to update the database. We're going to need to execute the request and
        // get it out to a list and then work with the messages inside memory here.
        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            // Get the message conversation between two users.
            var messages = await _context.Messages
                .Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
                    && m.Sender.UserName == recipientUsername
                    // Go the other way.
                    || m.Recipient.UserName == recipientUsername && m.SenderDeleted == false
                    && m.Sender.UserName == currentUsername
                )
                .OrderBy(m => m.MessageSent)
                // Projected into MessageDto and now we're working with MessageDto.
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                // We make it as a list,  
                // we're going to take the opportunity to mark the messages as read when a user gets the message thread
                // and then any messages that have been sent will mark them as read during this process as well,
                .ToListAsync();

            // We then find out if there's any unread messages for the current user that they received.
            // Check unread messages and get a list because we're going to need to loop over.
            // Check any unread messages while the recipient is the current username, we'll gonna mark them as read.
            var unreadMessages = messages.Where(m => m.DateRead == null
                && m.RecipientUsername == currentUsername).ToList();

            // Then we mark them as read.
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            // We don't need to map anymore. Just return the messages because we're already project to MessageDto.
            return messages;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);

        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);

        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
               .Include(x => x.Connections)
               .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);

        }

        // This is return a group for that specific connection id.
        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                // This is all related entity.
                .Include(c => c.Connections)
                .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }
    }
}
