using Microsoft.AspNetCore.Identity;


namespace API.Entities
{
    public class AppUserRole : IdentityUserRole<int>
    {   
        // Joint the entities
        public AppUser? User { get; set; }
        public AppRole? Role { get; set; }
    }
}
