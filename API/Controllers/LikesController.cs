using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // Make sure we're authenticated to this controller.
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;

        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
        }

        // Give the users a method to like another user.
        // {username}: this is the user that they're going to be liking.
        [HttpPost("{username}")]
        // Even though we're technically creating a resource on here, we don't need to return anything back to the client,
        // We already know what the primary key of the created entity is going to be, and we don't need to return that to the client in this case.
        public async Task<ActionResult> AddLike(string username)
        {
            // This is functionality that we want to user to be logged in for.
            var sourceUserId = User.GetUserId();

            // Get hold of the user that we're liking.
            var likedUser = await _userRepository.GetUserByUsernameAsync(username);
            // Get hold of the source user.
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

            // NotFound(): we didn't find the user that they wanted to like.
            if (likedUser == null) return NotFound();

            // Check as well to prevent user from liking themselves.
            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            // Double check to see if we already like this user, configured and added to the database.
            var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);
            // Note for future implementation: we can use toggle to add/remove the like.
            if (userLike != null) return BadRequest("You already like this user");

            // If we don't have one, then we're going to create one.
            userLike = new UserLike
            {
                // And that's all we need to do for this, because we've only got two columns.
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id,
            };

            // Add the user like and we'll do this to the source user.
            sourceUser.LikedUsers.Add(userLike);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");
        }
    

        // We're going to take this as a query string parameters, we don't need anything additional in the parameters here.
        [HttpGet]
        // predicate: what are they looking for.
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likeParams)
        {
            // Set the id first.
            // User.GetUserId(): User id of the currently logged in user.

            likeParams.UserId = User.GetUserId();

            

            // The ActionResult doesn't work so well with an interface like IEnumerable so we use return OK.
            var users = await _likesRepository.GetUserLikes(likeParams);

            // Because we're going to get a paginated response back from PagedList, we're going to have access
            // to the current page, total pages, etc, information inside the users.
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);

        }

    }
}   
