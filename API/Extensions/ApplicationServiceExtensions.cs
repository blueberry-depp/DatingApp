using API.Data;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using API.Services;
using API.SignalR;

namespace API.Extensions
{
    // When we create extension method, the class itself must be static.
    public static class ApplicationServiceExtensions
    {
        // To use or extend the IServiceCollection that we're going to be returning, we need to use 'this' keyword.
        public static IServiceCollection AddAplicationServices(this IServiceCollection services, IConfiguration config) {
            // AddSingleton: when instantiated is created and then it doesn't stop until our application stops. 
            services.AddSingleton<PresenceTracker>();
            // When we strongly type the key or configuration in this way use 'Configure'.
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            // AddScoped: when instantiated is created and then it stop when our http request are finished(suit for http request).
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, PhotoService>();
            // When we inject UnitOfWork into a controller then it's going to have an instance,
            // they're going to be using the data context that's injected into the unit of work. And that's the idea we pass that same
            // instance, the single instance to each of our repositories.
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<LogUserActivity>();

            // We only have a single project, so we only have a single assembly of where this can be created, so we use typeof(),
            // this is enough for Automapper to go ahead and find those profiles.
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
      
    }
}
