using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;

        public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _photoService = photoService;
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

        // Implement the AdminController GetPhotosForApproval method.
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration()
        {
            var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();

            return Ok(photos);
        }


        // Add a method in the Admin Controller to Approve a photo.
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo == null) return NotFound("Could not find photo");

            photo.IsApproved = true;    

            var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);

            // Set the photo to main when approved.
            if (!user.Photos.Any(p => p.IsMain)) photo.IsMain = true;

            await _unitOfWork.Complete();

            return Ok();
        }


        // Add a method in the Admin controller to reject a photo.
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Result == "ok")
                {
                    _unitOfWork.PhotoRepository.RemovePhoto(photo);
                }
            }
            else
            {
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
            }

            await _unitOfWork.Complete();

            return Ok();
        }
    }
}
