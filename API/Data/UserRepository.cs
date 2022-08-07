using API.DTOs;
using API.Entities;
using API.Helpers;
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

        public async Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser)
        {
            // Queries the database.
            var query = _context.Users
                .Where(x => x.UserName == username)
                // This is using AutoMapper queryable extensions
                // This is for selecting the properties that we want directly from the database.
                // Projects to MemberDto,
                // we can get the configuration that we provided in our AutoMapperProfiles here,
                // when we use projection, we don't actually need to include because the entity framework is going to
                // work out the correct query to join the table and get what we need from the database,
                // so this can be more efficient way of doing things.
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .AsQueryable();

            // Ignore Query filter for the current user.
            if (isCurrentUser) query = query.IgnoreQueryFilters();

            return await query.FirstOrDefaultAsync();
        }


        // Because this is something that we're only ever going to read from. We're not going to do anything with these entities.
        // Get the PagedList. 
        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            // This is an expression tree that's going to go to our database or entity framework is going to build up this query as an expression tree,
            // and then when we execute the ToListAsync, that's when it goes and executes the request in database.
            // This is IQueryable.
            // Because this is something that we're only ever going to read from. We're not going to do anything with these entities. So all
            // we need to do is read this.
            // AsQueryable(): This will give us an opportunity to do something with this query and decide what we want to filter by, for instance.
            var query = _context.Users.AsQueryable();

            // Filtering it before ProjectTo.
            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            // Filter by the age of the user. 
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1); // because this is based on today's dates we give it minus 1.
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            // Filtering by sorting and we use switch expression.
            query = userParams.OrderBy switch
            {
                // Case for created.
                "created" => query.OrderByDescending(u => u.Created),
                // For default case.
                _ => query.OrderByDescending(u => u.LastActive),

            };

            // And create this PagedList and passing those parameters, and we are no longer executing this,
            // we're simply passing this to another method that's simply going to execute ToListAsync(),
            // we're still not executing anything inside this method because that's still being taken care of inside reates async method.
            // ProjectTo before we send it to lists.
            // .AsNoTracking() Turns off to tracking in entity framework. 
            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking(), 
                userParams.PageNumber, userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            // "Find" is useful for getting something by its primary key. 
            return await _context.Users.FindAsync(id); 
        }

        public async Task<AppUser> GetUserByPhotoId(int photoId)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .IgnoreQueryFilters()
                .Where(p => p.Photos.Any(p => p.Id == photoId))
                .FirstOrDefaultAsync();
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        // Get the user's gender.
        public async Task<string> GetUserGender(string username)
        {
            // This gives us single property from the database.
            return await _context.Users
                .Where(x => x.UserName == username)
                .Select(x => x.Gender).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            // If you want return the photos too, don't use eager loading because it can cause circular reference, use DTO instead.
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }


        // We're not actually changing anything in the database, we are going to do is mark this entity as it has been modified.
        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}
