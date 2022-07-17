namespace API.Helpers
{
    public class PaginationHeader
    {
        public PaginationHeader(int currentPage, int itemPerPage, int totalItems, int totalPages)
        {
            CurrentPage = currentPage;
            ItemPerPage = itemPerPage;
            TotalItems = totalItems;
            TotalPages = totalPages;
        }

        // This is going to contain similar properties to what we had inside Pagedlist class,
        // but we can't reuse that, we need to do this in a separate class.
        // This is all the information we want to send back to the client and we'll just add a constructor in here to make it easier to use.
        public int CurrentPage { get; set; }    
        public int ItemPerPage { get; set; }    
        public int TotalItems { get; set; }    
        public int TotalPages { get; set; }    

    }
}
