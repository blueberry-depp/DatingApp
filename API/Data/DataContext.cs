using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        // Users -> The database table name. 
        public DbSet<AppUser> Users { get; set; }

        // We only want to add photos to an individual user's photo collection,
        // We're not going to be getting photos independently of the photo collection, as in we don't need to
        // go to our API specifically and go and get an individual photo,
        // we only want our photos to be added to a user's photo collection,
        // So for all of those reasons, we don't necessarily need a DB set in here for the photo class.

        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

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

            // Configure the other side of this relationship.
            builder.Entity<UserLike>()
               // Specify the relationships, the liked user can have many liked by users.
               .HasOne(s => s.LikedUser)
               .WithMany(l => l.LikedByUsers)
               .HasForeignKey(s => s.LikedUserId)
               .OnDelete(DeleteBehavior.Cascade);

            // We won't give it a made up key like we did with the likes, we gonna let the database generate this,
            builder.Entity<Message>()
               .HasOne(u => u.Recipient)
               .WithMany(m => m.MessagesReceived)
               // We don't want to remove the messages if the other party hasn't deleted them themselves.
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
               .HasOne(u => u.Sender)
               // Sender has many messages sent.
               .WithMany(m => m.MessagesSent)
               .OnDelete(DeleteBehavior.Restrict);
        }




    }
}
