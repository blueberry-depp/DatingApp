﻿using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    // This is entities class.
    public class AppUser : IdentityUser<int>
    {
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime LastActive { get; set; } = DateTime.Now;
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        // One to many relationship.
        public ICollection<Photo> Photos { get; set; }

        // LikedByUser: who has liked the currently logged in user.
        public ICollection<UserLike> LikedByUsers { get; set; }

        // This is the list of users, that are currently logged in user has liked.
        public ICollection<UserLike> LikedUsers { get; set; }

        public ICollection<Message> MessagesSent { get; set; }
        public ICollection<Message> MessagesReceived { get; set; }

        // AppUserRole is acting as joint table.
        public ICollection<AppUserRole> UserRoles { get; set; }






    }
}
