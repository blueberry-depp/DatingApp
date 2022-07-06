namespace API.DTOs
{
    public class MemberDto
    {
        public int Id { get; set; }
        // Change UserName to Username as lowercase to be able use inside angular application.
        public string Username { get; set; }
        // Use this property to use as the main photo that we send back for a user.
        public string PhotoUrl { get; set; }
        // Return user's age.
        // Automapper automatically call the AppUser GetAge method base on Get keyword.
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        // One to many relationship.
        public ICollection<PhotoDto> Photos { get; set; }
    }
}
