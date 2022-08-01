using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;

        // When we send a message, we're going to have to map it into a dto.
        // IHubContext<PresenceHub> presenceHub and PresenceTracker tracker: we got access to presence hub and tracker from within message hub.
        public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository, 
            IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _presenceHub = presenceHub;
            _tracker = tracker;
        }

        // Both of these methods(OnConnectedAsync and OnDisconnectedAsync) are now going to return an updated group to anyone else
        // that's still connected in that group. Obviously, if the group is empty, signal doesn't send anything because there's nothing
        // to listen to what it's sending. So we don't need to worry about that SignalR is going to if the group is empty, it simply doesn't
        // send anything to anyone because the group is empty.
        // When a client connects. 
        public override async Task OnConnectedAsync()
        {
            // The concept is that we gonna create a group for each user. We need to define the group name, 
            // the group name in our case is going to be the combination of the username and username. But
            // we want this in alphabetical order as well so that whatever a user connects to this particular hub,
            // we're going to put them into a group and we want to make sure it's the same group every time if they're
            // still chatting to the same user. So if we've got a group name of Lisa Todd
            // and Todd joins the group, we want it still to be Lisa Todd.

            // Get a hold of the HttpContext because we need to get hold of the other user's username.
            var httpContext = Context.GetHttpContext();
            // user: is the other user.
            // So what this means is that when we make a connection to this hub, we're going to pass in the other username with the key of user,
            // And get this into this particular property, we need to know which user profile the currently logged in user has clicked on and we
            // can get that via a query string that we can use when we create this particular hub connection.
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            //  It doesn't matter if there's any one user in that group connected or both users are connected. A user is always going
            // to go into this group.
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            // We've added user to the group.
            var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            // When a user joins the group. 
            var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            // Sending the message thread to caller whoever's connecting needs to receive message.
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        // SignalR when a user connects and disconnects and they are a member of the group,
        // then when they disconnect is automatically going to remove them from that particular group. And that's one of the advantages of using
        // a new hub so that when we do make a connection to this particular hub, then we have access to these two methods that we can override.


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

            await base.OnDisconnectedAsync(exception);
        }
         

        // Send message alongside with hub.
        // In signalR, we don't have access to API responses. We didn't have access to signalR that doesn't use HTTP.
        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            // Check to see if the recipient is trying to send a message to themselves.
            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot send messages to yourself");

            // Get hold of both of our users in the sender and the recipient as we need to populate the message.
            // Get the sender user. 
            var sender = await _userRepository.GetUserByUsernameAsync(username);
            // Get the recipient user.
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            // If it's not found, then we're going to return not found because we could not find the user.
            if (recipient == null) throw new HubException("Not found user");

            // Create the message.
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            // Get the group name.
            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _messageRepository.GetMessageGroup(groupName);
            
            // Check if they're in the same group.
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            // If the user is not connected to this hub or this group, then we want to send them a notification.
            else
            {
                var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    // We know the users online, but we also know they're not part of the same message group or not connected to the same group.
                    // Clients(connections): this can be used to invoke methods on the specified client connections, so for all of the connection id
                    // that a user might have where they're connected to the presence hub, then we're going to use that.
                    // new {}: create new anonymous object.
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new
                    {
                        username = sender.UserName,
                        knownAs = sender.KnownAs
                    });
                }
            }

            // What we want to return from this is a MessageDto. So we will also need to bring in IMapper into what we're doing here.
            _messageRepository.AddMessage(message);

            // Map from message.
            if (await _messageRepository.SaveAllAsync())
            {
                // Send the message.
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }


        // This method return the group, the message group that a user belongs to and we're going to send the group back to the
        // members of the group so they're always going to know who is connected inside the group that they're in and therefore we'll
        // be able to check to see if the recipient has joined the group and mark the messages as read.
        private async Task<Group> AddToGroup(string groupName)
        {   
            // Get the group.
            var group = await _messageRepository.GetMessageGroup(groupName);
            // Get a new connection for user. When a user connects to this hub, they're always given a new connection id unless they're reconnecting.
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _messageRepository.SaveAllAsync()) return group;

            // Because we are returning from this now, we need to throw an exception if that doesn't work.
            throw new HubException("Failed to join group");
        }


        private async Task<Group> RemoveFromMessageGroup()
        {
            // The goal here is we need to get the group for this particular connection because we were, again,
            // going to want to return the group from this particular method. But what we need is a method so
            // that we can get the group for this specific connection instead of just a connection, we need the
            // group and then we can get the connection from inside the group.
            var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
            // We got a group that includes their specific connection.
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _messageRepository.RemoveConnection(connection);
            if (await _messageRepository.SaveAllAsync()) return group;

            // If fail.
            throw new HubException("Failed to remove from group");

        }



        // Create helper method.
        //string other: other username.
        private string GetGroupName(string caller, string other)
        {
            // This is going to ensure that group name is always going to be alphabetical order for both the caller and the other.
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            // $: string formater.
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}
