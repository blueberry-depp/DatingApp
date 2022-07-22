namespace API.Helpers
{
    // We want pagination.
    public class MessageParams : PaginationParams
    {
        // This is going to be our currently logged in user,
        // because we're obviously going to get the messages for the user that's logged in.

        public string Username { get; set; }
        // Set the default the unread messages to the user.
        public string Container { get; set; } = "Unread";
    }
}
