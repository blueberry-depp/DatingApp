using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string GetUserName(this ClaimsPrincipal user)
        {
            // This give us the user's unique name from the token that the API uses
            // to authenticate this user, so that's the user we're going to be updating in this case.
            return user.FindFirst(ClaimTypes.Name)?.Value; 
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            // int.Parse: convert to int
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
