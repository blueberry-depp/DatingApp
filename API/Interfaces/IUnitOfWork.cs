namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        // Return IUserRepository.
        IUserRepository UserRepository { get; }
        IMessageRepository MessageRepository { get; }
        ILikesRepository LikesRepository { get; }
        IPhotoRepository PhotoRepository { get; }
        // This is going to be the method to save all of changes.
        Task<bool> Complete();
        // This is going to be the helper method to save all of changes. This one for to see if entity
        // framework has been tracking or has any changes, we'll need to use that in one specific place.
        bool HasChanges();

    }
}
