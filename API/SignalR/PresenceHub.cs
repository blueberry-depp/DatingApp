using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        // We can override virtual method.
        // When a client connects, we're going to update our presence tracker and we're going to send the updated
        // list of current users back to everyone that's connected.
        public override async Task OnConnectedAsync()
        {
            // When user connect or online and get the result back from user connected presence tracker.
            var isOnline = await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);

            // Send the user is online to all the connected clients apart from the caller, so we're going to send this one to the others
            if (isOnline)
            {
                // These are clients that are connected to this hub.
                // UserIsOnline: the name of the method that we use inside the client.
                // Context.User.GetUsername(): context with token inside and we can get the user.
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
            }

            // Get a list of the currently online users.
            var currentUsers = await _tracker.GetOnlineUsers();
            // Send list to just user that's is connecting, whereas if they're already connected,
            // then really we just want to update the list with who has connected.
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);

        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

            if (isOffline)
            {
                // Notify the other users that someone is genuinely gone offline, but if they're still maintaining one connection
                // on a different device, then they're still going to be showing us online.
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            }

            // If we do have an exception, we'll just pass that up to the base so the parent class.
            await base.OnDisconnectedAsync(exception);

        }

    }
}
    