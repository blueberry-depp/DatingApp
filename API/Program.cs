using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class Program
    {
        // This is the fine place to seed data,
        // and what goes on inside here happens before our application is actually started.
        public static async Task Main(string[] args)
        {   
            // Remove the run() method here.
                var host = CreateHostBuilder(args).Build();

            // Get data context service so that we can pass it to seed method.
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            // So even though we spent a bunch of time setting up a global exception handler, we don't have access 
            // to it in this Main method.
            try
            {
                var context = services.GetRequiredService<DataContext>();
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
                // Get the database and migrate our database here,
                // if we drop our database, then all we need to do is restart our application and our database will be recreated.
                await context.Database.MigrateAsync();
                await Seed.SeedUsers(userManager, roleManager);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during migration");
            }

            // Make sure we call the run method after we finished doing what we're doing here.
            await host.RunAsync();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
