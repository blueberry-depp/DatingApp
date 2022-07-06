using API.DTOs;
using API.Entities;

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

        // Return list IEnumerable member dto.
        Task<IEnumerable<MemberDto>> GetMembersAsync();

        // Return member dto.
        Task<MemberDto> GetMemberAsync(string username);

    }
}
