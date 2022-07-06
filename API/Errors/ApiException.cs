namespace API.Errors
{
    public class ApiException
    {
        // We make constructor to make it easy to use.
        // Exception always 500 in status code.
        public ApiException(int statusCode, string message = null, string details = null) 
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
