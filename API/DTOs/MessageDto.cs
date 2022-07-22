namespace API.DTOs
{
    // Because we've got a big DTO here, we're going to use automapper to help us
    // with the mapping between this and our actual message.
    public class MessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        // So that we can display the image of the user that sent the message.
        public string SenderPhotoUrl { get; set; }
        public int RecipientId { get; set; }
        public string RecipientUsername { get; set; }
        public string RecipientPhotoUrl { get; set; }
        public string Content { get; set; }
        // Make this optional because we want this to be null if the message has not been read.
        public DateTime? DateRead { get; set; }
        // Remove the initialization of the date time.
        public DateTime MessageSent { get; set; }
        // We don't need to send back the sender deleted and recipient deleted.

    }
}