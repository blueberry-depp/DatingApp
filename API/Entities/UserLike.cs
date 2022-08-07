namespace API.Entities
{
    public class UserLike
    {
        public AppUser? SourceUser { get; set; }
        public int  SourceUserId { get; set; }

        // The other side
        public AppUser? LikedUser { get; set; }
        public int LikedUserId { get; set; }


    }
}
    