using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace API.Data
{
    // Write the logic so that we can get the data out of that Json file and into database.
    // We seed this data for just developing only.
    public class Seed
    {
        // Not returning anything, hence Task not have any type parameter.
        // We call this method from Program.cs
        // What we get from identity now is a UserManager so that we can manage the users.
        // And we gonna seed rolls into database.
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            // Check to see if user table contains any users and will return from this if we do have any users.
            if (await userManager.Users.AnyAsync()) return;

            // If we continue, that means we do not have any users in our database and we want to go and interrogate
            // that file to see what we have inside there and store it in a variable.
            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");

            // We want to get the Json out of this and deserialize into an object
            // Now, at this stage, our users should be a normal list of users of type AppUser.
            // We should have it out of the Jason file by this point.
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            // Check to make sure that there are users or no users in database.
            if (users == null) return;

            var roles = new List<AppRole>
            {
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"},

            };

            // Loop through these different roles.
            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            foreach (var user in users)
            {
                user.UserName = user.UserName.ToLower();

                // We're not checking the results, because this is just seed method. We're only run once
                // and only the database is clean.
                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Member");
            };

            // Create new user for the admin.
            var admin = new AppUser
            {
                UserName = "admin",
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});


            // The UserManager takes care of saving the changes into the database. So we don't need to do that here.
        }
    }
}
