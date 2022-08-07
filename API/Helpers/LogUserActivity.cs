using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    // Implementing action filter(allow us to do something even before the request is executing or after the request is executed),
    // and make this as services.
    // Add this in a central place so we can put it in one place, kind of switch it on and then forget about it.
    public class LogUserActivity : IAsyncActionFilter
    {
        // next: what's going to happen next after the action is executed and we can use this property, all this parameter,
        // to execute the action and then do something after this is executed.
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // We have a context after the action has been executed in this variable.
            var resultContext = await next();

            // Check if the user is authenticated because we don't want to try and execute something when we don't have a user to do this on,
            // return true is the user have the token otherwise return nothing.
            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            // If user are authenticated, update last active property.
            // Get the user id.
            var userId = resultContext.HttpContext.User.GetUserId();


            // Get access to our repository and to do that inside here, we can use the service locator pattern and
            // we actually use this inside our program.cs earlier and
            // we're going to specify that we want to get access to the IUserRepository.
            var unitOfWork = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();

            // Get hold of our user object.
            var user = await unitOfWork.UserRepository.GetUserByIdAsync(userId);

            // Set the user last active.
            user.LastActive = DateTime.UtcNow;

            // Save the changes.
            await unitOfWork.Complete();


        }
    }
}
