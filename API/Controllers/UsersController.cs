using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using System.Security.Claims;
using AutoMapper;
using API.Extensions;
using API.Entities;
using API.Helpers;

namespace API.Controllers
{
    // User goes to get a user or get all of the users, then we want them to authenticate,
    // all of the methods inside this controller now are going to be protected with authorization.
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        // Automapper is going to be smart enough to recognize any properties that are named the same to make DTO works.
        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) // Inject the automapper and photo service too.
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        // IEnumerable is just use simple iteration over a colection a specified type, not have the method,
        // otherwise in List, we can use many method associated with List.
        // UserParams userParams is query string. We have to specified it. 
        // The error is shown because we didn't supply anything in the query string in url and we've got the UserParams objects there,
        // then that's the reason they got confused. And it's OK with that being empty, but it just didn't know what to do in that case,
        // and add FromQuery guarantees that our request works.
        // We also populate the current user name property into the userParams and set a default property that is just the opposite to their
        // current gender if they don't specify anything inside userParams.
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            // We need to get the current user in order to get access to the gender or this is simply to get an the gender.
            var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";  
            }

            // This is PagedList of type MemberDto. And this means we've got our pagination information inside here as well.
            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

            // We've always got access to the HTTP request response stuff inside controller.
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            // We just have to wrap the result in an OK response to make it work.
            return users;
        }

        //[Authorize(Roles = "Member")]
        // Give the root the name 'Name = "GetUser"'.
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var currentUsername = User.GetUsername();
            // Return MemberDto directly from our repository.
            return await _unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser: currentUsername == username);
        }


        // Receive data from user.
        // We don't need to send anything, any objects back from this, because the theory is that the client
        // has all the data related to the entity we're about to update, so we don't need to return the objects
        // back from our API because the client is telling the API what it's updating, so it has everything
        // it needs and we don't need to return the user object from this.
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            // The first thing we need to do is actually get hold of the user and
            // we also need to get hold of the user's username, and we don't want to
            // trust the user to give us their user name,
            // we actually want to get it from what we're authenticating again, which is the token,
            // inside the controller, we have access to a ClaimsPrincipal of the user,
            // this contains information about their identity.
            // And what we want to do inside here is find the claim that matches the name identifier, which is the
            // claim that we give the user in that token.
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            // This saves us manually mapping between our update dto and our user objects, ex: user.City = memberUpdateDto.City.
            _mapper.Map(memberUpdateDto, user);

            // Now our user object is flagged as being updated by entity framework,
            // whatever happens, even if our user has not been updated by simply adding this flag, we guarantee that 
            // we are not going to get an exception or an error when we come back from updating the user in our database.
            _unitOfWork.UserRepository.Update(user);

            // We don't need to send any content back for put request.
            if (await _unitOfWork.Complete()) return NoContent();

            // If this fails, then what we can do is just return a bad request.
            return BadRequest("Failed to update user");
        }


        [HttpPost("add-photo")]
        // Return PhotoDto.
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            // We get the user and have user object,
            // don't forget, when we do this GetUserByUsernameAsync method, this includes our photos we're eagerly loading them in
            // this method and we need to for this.
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            var result = await _photoService.AddPhotoAsync(file);

            // Check the error
            if (result.Error != null) return BadRequest(result.Error.Message);

            // Creating new photo.
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId

            };

            /*// Check to see if the users got any photos at the moment.
            if (user.Photos.Count == 0)
            {
                // If it is, then we know that this is the first image that the users uploading, and
                // if it is the first photo uploaded, then we're going to set this one to main.
                photo.IsMain = true;
            }*/

            // Add the photo.
            user.Photos?.Add(photo);

            // Save the changes to database and return the photo.
            // The response status code must 201 not 200.
            if (await _unitOfWork.Complete())
            {
                // Map photo to photoDto.
                // Return 201.
                // We're going to return the roots of how to get the user, which contains the photos, and we can still return our photo object.
                // 'username' is name of route parameter and set it to equal user.UserName.
                // Check the headers in Location ex. https://localhost:5001/api/Users/cara
                // that tell the client where to go to get the image that we've uploaded, now, the image is going to be in a collection of photos for cara.
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }

            // If fail.
            return BadRequest("Problem adding photo");
        }


        [HttpPut("set-main-photo/{photoId}")]
        // When we're updating resource, remember, we don't need to pass the object back.
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            // Get the username from the token, this means we're validating that this is the user that they say they are. We can
            // trust the information inside the token. There's no trickery going on there because our servers signed the token
            // so they're authenticating to this method.
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            // Also remember that GetUserByUsernameAsync method that we get from our repository has an eager loading property for the user's photos collection, 
            // so we do have access to the photos inside here.
            // And this is not asynchronous because we've already got the user in memory at this point, so we're not going to the database now,
            // we've already done that inside the _unitOfWork.UserRepository.
            // Finally look for a photo with the ID that matches the photo ID we're getting from photoId parameter.
            var photo = user.Photos?.FirstOrDefault(x => x.Id == photoId);

            // Check to make sure the user is not trying to set a photo that is main to main. So if somehow they managed to do that,
            // which we will, of course, prevent them from doing on the client too.
            if (photo.IsMain) return BadRequest("This is already your main photo");

            // Get the current main photo.
            var currentMain = user.Photos?.FirstOrDefault(x => x.IsMain);
            // Check if the current main photo is null and set it to false.
            if (currentMain != null) currentMain.IsMain = false;

            // Set the photo that we're passing in, the one that we're trying to set to main.
            photo.IsMain = true;

            // Save our changes back to the repository and for update we return no content, don't need to send anything back in this request.
            if (await _unitOfWork.Complete()) return NoContent();

            // If fail.
            return BadRequest("Failed to set main photo");
        }


        [HttpDelete("delete-photo/{photoId}")]
        // When we delete a resource, then we don't need to send anything back to the client.
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            // Get the user object.
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            // Get the photo that we're interested in deleting.
            var photo = user.Photos?.FirstOrDefault(x => x.Id == photoId);

            // Check the photo is not null.
            if (photo == null) return NotFound();

            // Check the photo is main photo.
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            // Check to see if we have a public id for this particular image, because some of our photos have this property, some of them don't,
            // and the only ones that we need to delete from cloudinary are the ones that do have a public id.
            if (photo.PublicId != null)
            {
                // Delete it from cloudinary and we do actually get deletion results from this method.
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                // We're just going to return out of our method now, so this is going to stop execution when we return.
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            // Remove photo from the database.
            // And all this does is add the tracking flag and we're updating our user at this point, remember, because the photos is a related entity on our user.
            user.Photos?.Remove(photo);

            if (await _unitOfWork.Complete()) return Ok();

            // If we don't have success with saving.
            return BadRequest("Failed to delete the photo");





        }

    }

}
