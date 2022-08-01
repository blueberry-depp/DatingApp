using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    // <AppUser, AppRole, int>: Because we want to access to user roles, we need to provide type parameters for this.
    // Because we want to get a list of the user roles, then we need to go a bit further and we need to identify every single type,
    // that we need to add to identity. If we specify AppUserRole then we need to identify all of the different types and
    // give us an opportunity to ensure they're using integers
    public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole, 
        IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        // Users -> We don't need Users DbSet because IdentityDbContext provide it. 

        // We only want to add photos to an individual user's photo collection,
        // We're not going to be getting photos independently of the photo collection, as in we don't need to
        // go to our API specifically and go and get an individual photo,
        // we only want our photos to be added to a user's photo collection,
        // So for all of those reasons, we don't necessarily need a DB set in here for the photo class.

        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        // In order to access to different entities, we're going to need to update data context.
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }

        // We need give the entities some configuration, and the way that we do
        // that is we need to override a method inside the DB context.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // because we're overriding this method, we'll just pass into the class we're deriving from and we get access to that using base,
            // if we don't do this, we can sometimes get errors when we try and add migration.
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

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
