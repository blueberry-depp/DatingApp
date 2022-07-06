using API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Data
{
    // Write the logic so that we can get the data out of that Json file and into database.
    public class Seed
    {
        // Not returning anything, hence Task not have any type parameter.
        // We call this method from Program.cs
        public static async Task SeedUsers(DataContext context)
        {
            // Check to see if user table contains any users and will return from this if we do have any users.
            if (await context.Users.AnyAsync()) return;

            // If we continue, that means we do not have any users in our database and we want to go and interrogate
            // that file to see what we have inside there and store it in a variable.
            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");

            // We want to get the Json out of this and deserialize into an object
            // Now, at this stage, our users should be a normal list of users of type AppUser.
            // We should have it out of the Jason file by this point.
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            foreach (var user in users)
            {
                // Add them to our database,
                // we still need passwords for these, which will hard code and we'll create them,
                // we also need to use the password, salt and password hash.
                using var hmac = new HMACSHA512();
                user.UserName = user.UserName.ToLower();
                // We don't want to generate random passwords in our seed data because then it's going to be a nightmare
                // to use and log in with these different users.
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
                user.PasswordSalt = hmac.Key;

                // Adding, tracking to the user through entity framework, we're not doing anything with the database at this point.
                context.Users.Add(user);
            }

            // Save to the database.
            await context.SaveChangesAsync();
        }
    }
}
