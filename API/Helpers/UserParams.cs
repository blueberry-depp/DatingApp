namespace API.Helpers
{
    // Create a class so that we can receive the pagination parameters from the user.
    public class UserParams : PaginationParams
    {   
        // Add couples properties. 
        public string? CurrentUsername { get; set;}
        public string? Gender { get; set;}
        public int MinAge { get; set; } = 18;    
        public int MaxAge { get; set; } = 150;
        public string? OrderBy { get; set; } = "lastActive";   


    }
}
