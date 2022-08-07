using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRole : IdentityRole<int>
    {
        // Create a collection.
        public ICollection<AppUserRole>? UserRoles { get; set; }
    }
}
