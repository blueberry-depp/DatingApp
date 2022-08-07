namespace API.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string? SenderUsername { get; set; }
        // Define relationships between AppUser and Message.
        public AppUser? Sender { get; set; }
        public int RecipientId { get; set; }
        public string? RecipientUsername { get; set; }
        public AppUser? Recipient { get; set; }
        public string? Content { get; set; }
        // Make this optional because we want this to be null if the message has not been read.
        public DateTime? DateRead { get; set; }
        // As soon as we create a new instance of this, then we set the utc time to the current server timestamp.
        public DateTime MessageSent { get; set; } = DateTime.UtcNow;
        // If a user decides to delete a message that they have sent, then we're not
        // going to delete it from the recipient's view of the messages. The only time we delete a message from the server,
        // if both the sender and the recipient have both deleted the message.
        public bool SenderDeleted { get; set; }
        // Recipient has deleted the message.
        public bool RecipientDeleted { get; set; }

    }
}
