using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        // Update user profile,
        // the updates is not an async method, because all that's going to do is update
        // the tracking status in entity framework to say that something has changed.
        void Update(AppUser user);

        // Save all changes.
        Task<bool> SaveAllAsync();

        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);

        // Return PagedList of member dto.
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);

        // Return member dto.
        Task<MemberDto> GetMemberAsync(string username);

    }
}
