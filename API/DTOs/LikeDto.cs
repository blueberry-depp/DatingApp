namespace API.DTOs
{
    public class LikeDto
    {
        // The properties that we want to return, this is the information we need to create a member card
        // just like displaying on our member list and the users we like, we're going to display them just as cards,
        // we'll get the information from this dto.
        public int Id { get; set; }
        public string Username { get; set; }
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public string PhotoUrl { get; set; }
        public string City { get; set; }

    }
}
