using API.DTOs;
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
                .AsQueryable();

            // Check the container. And depending on which container it is, will depend on which messages we return.
            query = messageParams.Container switch
            {   // if we're the recipient of a message and we've read it, then this is what is gonna to go back from this one.
                // u.RecipientDeleted == false: add extra check to see if a user who's sent a message deletes that message
                // sent, then we don't want to send that back. So their see it in their outbox. So we're only
                // returning messages that the recipient has not deleted or returning messages that have not been deleted.
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username && u.SenderDeleted == false),
                // u.DateRead == null: which will mean that they have not read the message yet.
                _ => query.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
            };

            // Project in here, so must bring the IMapper here.
            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
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
                // Eagerly loading.
                // .ThenInclude(p => p.Photos): this is going to give us access to our user photos.
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                // We want to display the user's photo in the message design as well.
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
                    && m.Sender.UserName == recipientUsername
                    // Go the other way.
                    || m.Recipient.UserName == recipientUsername && m.SenderDeleted == false
                    && m.Sender.UserName == currentUsername
                )
                .OrderBy(m => m.MessageSent)
                // We use ToListAsync, we're not going to project out of this, because
                // we're going to take the opportunity to mark the messages as read when a user gets the message thread
                // and then any messages that have been sent will mark them as read during this process as well,
                // so because we're not projecting, what we do need to do is eagerly load the photos for the user because
                // we're going to include those as well.
                .ToListAsync();

            // We then find out if there's any unread messages for the current user that they received.
            // Check unread messages and get a list because we're going to need to loop over.
            // Check any unread messages while the recipient is the current username, we'll gonna mark them as read.
            var unreadMessages = messages.Where(m => m.DateRead == null
                && m.Recipient.UserName == currentUsername).ToList();

            // Then we mark them as read.
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }

                // Save these changes to the database.
                await _context.SaveChangesAsync();
            }

            // Return message dto as a list.
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
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
