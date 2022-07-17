using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;

        public LikesRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likeUserId)
        {
            // That's enough to go and find the individual like because sourceUserId, likeUserId make up our primary key of this particular entity.
            return await _context.Likes.FindAsync(sourceUserId, likeUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            // Get the user order by username, we're going to be building up queries, and we're going to be joining these
            // inside query and letting entity framework work out the joint query that's needed to be made, because 
            // what we still want to return is a list of users. But we're going to select just the properties we need in that like DTO.
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            // This is the users that are currently logged in user has liked.
            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);

                // This gives us is our users from likes table and our like user is an AppUser and we're
                // selecting that and we're passing it into users query.
                users = likes.Select(like => like.LikedUser);
            }

            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
                // TThis give us a list of users that have liked the currently logged in user.
                users = likes.Select(like => like.SourceUser);
            }

            // We'll projects this into new LikeDto, so we'll use select, we won't use Automapper for this,
            // we'll use just a manual select statement so that we can project directly into LikeDto and selecting user from LikeDto.
            var likedUsers = users.Select(user => new LikeDto
            {   
                // Manual mapping.
                Id = user.Id,
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City
            });

            // Return a PagedList of type LikeDto.
            // CreateAsync: the source is likedUsers and and create PagedList instance.
            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize) ;
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            // Get the list of users that this user has liked/getting a user with their collection of liked,
            // when they add alike is we're going to be adding it to the user that we return from _context.Users.
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}
