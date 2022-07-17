using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        // Return the UserLike, we can get it by its primary key.
        Task<UserLike> GetUserLike(int sourceUserId, int likeUserId);
        Task<AppUser> GetUserWithLikes(int userId);
        // Return the PagedList of LikeDto and passing LikesParams to this method.
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likeParams);
    }
}
