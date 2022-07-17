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

        public DbSet<UserLike> Likes { get; set; }

        // We need give the entities some configuration, and the way that we do
        // that is we need to override a method inside the DB context.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // because we're overriding this method, we'll just pass into the class we're deriving from and we get access to that using base,
            // if we don't do this, we can sometimes get errors when we try and add migration.
            base.OnModelCreating(builder);

            // Work on UserLike entity.
            builder.Entity<UserLike>()
                // Form the primary key for UserLike table.
                .HasKey(k => new { k.SourceUserId, k.LikedUserId });

            // Configure the relationships.
            builder.Entity<UserLike>()
                // Specify the relationships, source user can like many other users
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                // Specify the foreign key.
                .HasForeignKey(s => s.SourceUserId)
                // Configure the on delete behavior to cascade, so if we delete a user, we delete the related entities.
                .OnDelete(DeleteBehavior.Cascade);

            // The other side of this relationship.
            builder.Entity<UserLike>()
               // Specify the relationships, the liked user can have many liked by users.
               .HasOne(s => s.LikedUser)
               .WithMany(l => l.LikedByUsers)
               .HasForeignKey(s => s.LikedUserId)
               .OnDelete(DeleteBehavior.Cascade);   

        }

    }
}
