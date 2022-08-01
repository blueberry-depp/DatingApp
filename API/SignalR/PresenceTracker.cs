namespace API.SignalR
{
    // This PresenceTracker, is going to be a service that we create and we want this to be shared amongst every
    // single connection that comes into our server,
    // in presence tracker, we're maintaining a dictionary of all of the users that are online and including their connections.
    public class PresenceTracker
    {   // This dictionary store is memory not the database.
        // Dictionary store key and value pairs.
        // Every time that a user connects to the hub, they're going to be given a connection ID. Now,
        // there's nothing to stop a user from connecting to the same application from a different device,
        // and they would get a different connection ID for each different connection that they're having or making to our application.
        // List<>: list of the connection IDs and a connection are strings.
        private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();

        // Create a couple of methods to add a user to the dictionary when they connect along with their connection ID, 
        // and also handle the occasion when they disconnect.
        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            // We need to be careful here because this dictionary is going to be shared amongst everyone who
            // connects to our server and the dictionary is not a threat safe resource, 
            // so if we had concurrent users trying to update this at the same time, then we're probably going to run into problems,
            // so what we need to do to get around that is we need to lock the dictionary,
            // so we're effectively locking the dictionary until we've finished doing what we're doing inside lock dictionary.
            lock (OnlineUsers)
            {
                // Check to see if that we already have a dictionary element with a key of the currently log in username, and if it is, then we're going
                // to add the connection, otherwise we're going to create a new dictionary entry for this particular username with that connection.
                if (OnlineUsers.ContainsKey(username))
                {
                    // And if we do, then access the dictionary element with the key of the username and add the connection ID to the list.
                    OnlineUsers[username].Add(connectionId);
                }
                // If they just connected and it's a new connection.
                else
                {
                    OnlineUsers.Add(username, new List<string>{connectionId});
                    isOnline = true;
                }
            }

            // To say we finished.
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false;
            lock (OnlineUsers)
            {
                // Check to see if user have a dictionary element with the key of the currently login username. If not
                // then we simply return tasks to complete a task as we have no more work to do here.
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

                // If we move past this.
                OnlineUsers[username].Remove(connectionId);
                // If the users offline, they don't have any connections for that particular username.
                if (OnlineUsers[username].Count == 0)
                {
                    // Remove the element with the key of username. 
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
            }

            return Task.FromResult(isOffline);

        }

        // Get all of the users that are currently connected and return an array of the usernames. 
        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;

            lock (OnlineUsers)
            {
                // OrderBy: order by the key and each key is a user's username and select the key, we're not interested in the values or connection ID, 
                // ToArray to execute this particular request.
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }


        // Get a list of connections for a particular user that stored inside dictionary.
        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock (OnlineUsers)
            {
                // Get connection ids.
                // GetValueOrDefault: if the user doesn't exist in our connections then it's simply going to be null. That's the
                // default that we would be returning if we don't have the value in dictionary,
                // and if we have a dictionary element with key of username, then this is going to return the list of connection id for that particular user.
                connectionIds = OnlineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connectionIds);

        }


    }
}
