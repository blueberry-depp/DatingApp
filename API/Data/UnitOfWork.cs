using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    // What we're going to be doing from this units of work is creating instances of the repositories and we're going to pass it.
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UnitOfWork(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IUserRepository UserRepository => new UserRepository(_context, _mapper);

        public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);

        public ILikesRepository LikesRepository => new LikesRepository(_context);

        public IPhotoRepository PhotoRepository =>  new PhotoRepository(_context);

        // All of the changes that entity framework has tracked, no matter which repository we used to do something,
        // then we are going to be using this one to save all of our changes.
        public async Task<bool> Complete()
        {
            // Because we want to return a boolean from this, we want to make sure that greater than 0 changes have been saved into our database,
            // if something has changed, something has been saved, then it's going to return a value greater than 0
            // because the SaveChangesAsync returns an integer from this particular method for a number of changes that have been saved in a database.
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}
