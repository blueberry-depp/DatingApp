var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAplicationServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSignalR();

// Configure the HTTP request pipeline
var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Regardless of which we're running in, we're just going to use our middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors(x => x.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials() // For signalR due to the way that we now send up our access token or token.
.WithOrigins("https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();

// Tell API server to use static files.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("Index", "Fallback");

// Get data context service so that we can pass it to seed method.
using var scope = app.Services.CreateScope();
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
await app.RunAsync();


