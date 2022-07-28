using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser> userManager )
        {
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUserWithRoles()
        {
            // Get the users with their role.
            var users = await _userManager.Users
                // Get the UserRoles list of user roles, but we also need to include the role itself below.
                .Include(r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                // Project out of this and return an anonymous object(don't have a type for what we returning),
                // we're going to get an object back with the users Id and their user name and the roles that they're in,
                // and then we're sending that to a list.
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);

            // NOTE for future implementation: paginating these result.
        }

        [HttpPost("edit-roles/{username}")]
        // Pass the roles as query string
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            // selectedRoles will come up as a comma separated list query string and split to an array.
            var selectedRoles = roles.Split(",").ToArray();

            var user = await _userManager.FindByNameAsync(username);

            // Check there is a user otherwise we get exception from var userRoles.
            if (user == null) return NotFound("Could not find user");

            // Give us a list of roles for this particular user.
            var userRoles = await _userManager.GetRolesAsync(user);

            // Take a look at the list of roles and add the user to the roles unless they're already in that particular role.
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            // Check if that succeeded or not.
            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            // Return roles for the specific user .
            return Ok(await _userManager.GetRolesAsync(user));



        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}
