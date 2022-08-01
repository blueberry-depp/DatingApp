using API.Data;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Extensions
{
    // When we create extension method, the class itself must be static
    public static class IdentityServiceExtensions
    {
        // To use or extend the IServiceCollection that we're going to be returning, we need to use 'this' keyword
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            // AddIdentityCore is for SPA application.
            // opt is option.
            services.AddIdentityCore<AppUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddSignInManager<SignInManager<AppUser>>()
                .AddRoleValidator<RoleValidator<AppRole>>()
                // DataContext for it sets up our database with all of the tables we need, to create the .net identity tables.
                .AddEntityFrameworkStores<DataContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                        ValidateIssuer = false, // obviously our API server
                        ValidateAudience = false, // our angular application
                    };

                    // Vecause signalR or WebSocket cannot send an authentication header,
                    // for signalR we can always use a query string, whereas for our API controllers, it's just
                    // going to use the authentication header as we've been using so far.
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // access_token: this needs to be specific because signalR by default will send up token as a query string.
                            // This allows our client to send up the token as a query string,
                            var accessToken = context.Request.Query["access_token"];

                            // Check the path of this request, where's it coming to, because we only want to do this for signalR,
                            var path = context.HttpContext.Request.Path;
                            // See if we actually have an access token and see if we've got our JWT token and check that the path to match what we
                            // used inside startup class.
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }

                    };

                });

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            return services;
        }
    }
}
