using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    // Specify photos as the table name.
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public string PublicId { get; set; }
        public bool IsApproved { get; set; }

        // This is fully defining the relationship
        // Set the relation between AppUser and Photo
        public AppUser AppUser { get; set; }    
        public int AppUserId { get; set; }
    }
}