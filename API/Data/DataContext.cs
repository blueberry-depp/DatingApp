using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; } // Users -> It's the database table 

        // We only want to add photos to an individual user's photo collection,
        // We're not going to be getting photos independently of the photo collection, as in we don't need to
        // go to our API specifically and go and get an individual photo,
        // we only want our photos to be added to a user's photo collection,
        // So for all of those reasons, we don't necessarily need a DB set in here for the photo class.
    }
}
