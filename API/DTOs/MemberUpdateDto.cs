namespace API.DTOs
{
    // Because we're going to be receiving data from the user, we need to add MemberUpdateDto that we can use as a parameter for our update method.
    public class MemberUpdateDto
    {
        // We'll let user update in the following option
        // Because it's a dto and we're going to want to map this into our user entity.
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }


    }
}
