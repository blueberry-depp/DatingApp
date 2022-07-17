namespace API.Helpers
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;

        // This is default page size, we're also going to compare against the MaxPageSize
        // so that if the client chooses a different value from 10, then we want to compare against 50. And
        // if it's over this, then we're going to set it to 50.
        private int _pageSize = 10;

        public int PageSize
        {
            // Get the current page size.
            get => _pageSize;
            // Set the current page size.
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}
