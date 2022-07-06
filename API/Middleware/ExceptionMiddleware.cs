using API.Errors;
using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    // Create our own exception middleware in API.
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        // RequestDelegate is what's next, what coming up in the middleware pipeline.
        // ILogger for log out exception into terminal.
        // IHostEnvironment: Check what environment we're running in, are we in production, are we in development.
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        // HttpContext: because this is happening in the context of an HTTP request when
        // we're add middleware we have access to the actual HTTP request that's coming in.
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // First thing we're going to do is get a context and to simply pass this on to the next piece of middleware.

                await _next(context);
            }
            // This piece of middleware is going to live at the very top. Of our middleware and anything below this,
            // let's if we have 17 of middleware that all gonna invoked next at some point and if any of them get an exception,
            // they're going to throw the exception up and up and up until they reach something that can handle the exception,
            // and then because our exception middleware is going to be at the top of that tree, then we're going to catch the exception inside here. 
            catch (Exception ex)
            {
                // If we don't do this, then our exception is going to be silence in our terminal.
                _logger.LogError(ex, ex.Message);
                // Write out this exception to our response. 
                context.Response.ContentType = "application/json";
                // (int): cast to integer
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Create a response.
                // Check to see what environment we're running in, are we running in development mode?
                // ex.StackTrace?: just in case this is null, we need to add the question mark,
                // we don't want to cause an exception in our exception handling middleware,
                // just in case this is null, then we're going to prevent any exceptions from this by adding that question mark.
                var response = _env.IsDevelopment() ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()) 
                    // In production mode
                    : new ApiException(context.Response.StatusCode, "Internal Server Error");

                // Create some options
                // what we can do is send back this in Json, by default, we want our Json responses to go back in Camel case, 
                // so we're going to create some options to enable this because we need to serialize this response into JSON response
                // and this is just going to ensure that our response just goes back as a normal Json formatted response in camel case
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var json = JsonSerializer.Serialize(response, options); 

                await context.Response.WriteAsync(json);    

            }
        }
    }
}
