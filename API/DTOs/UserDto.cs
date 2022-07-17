namespace API.DTOs
{
    // This is the object we gonna return when user login or register.
    public class UserDto
    {
        public string Username { get; set; }
        public string Token { get; set; }
        // Return the users photo with their user object and it's going to be their main photo.
        public string PhotoUrl { get; set; }
        public string KnownAs { get; set; }
        public string Gender { get; set; }
    }
}
