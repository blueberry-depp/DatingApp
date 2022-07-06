using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    // This is the implementation class of user repository.
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        // Because UserRepository accessing DB context, we're going to need a constructor,
        // and inject data context.
        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            // Queries the database.
            // SingleOrDefaultAsync: execute the request.
            return await _context.Users
                .Where(x => x.UserName == username)
                // This is using AutoMapper queryable extensions
                // This is for selecting the properties that we want directly from the database.
                // Projects to MemberDto,
                // we can get the configuration that we provided in our AutoMapperProfiles here,
                // when we use projection, we don't actually need to include because the entity framework is going to
                // work out the correct query to join the table and get what we need from the database,
                // so this can be more efficient way of doing things.
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
               .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            return await _context.Users
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
               .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            // "Find" is useful for getting something by its primary key. 
            return await _context.Users.FindAsync(id); 
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            // If you want return the photos too, don't use eager loading because it can cause circular reference, use DTO instead.
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        // Returning a boolean to say that if our changes have been saved.
        public async Task<bool> SaveAllAsync()
        {
            // Because we want to return a boolean from this, we want to make sure that greater than 0 changes have been saved into our database,
            // if something has changed, something has been saved, then it's going to return a value greater than 0
            // because the SaveChangesAsync returns an integer from this particular method for a number of changes that have been saved in a database.
            return await _context.SaveChangesAsync() > 0;
        }

        // We're not actually changing anything in the database, we are going to do is mark this entity as it has been modified.
        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}
