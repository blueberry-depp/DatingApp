using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string GetUserName(this ClaimsPrincipal user)
        {
            // This give us the user's username from the token that the API uses
            // to authenticate this user, so that's the user we're going to be updating in this case.
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
        }
    }
}
