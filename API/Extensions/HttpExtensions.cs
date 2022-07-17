using API.Helpers;
using System.Text.Json;

namespace API.Extensions
{
    // Create an extension method so that we can add a pagination header to a http response.
    public static class HttpExtensions
    {
        // We're not going to return anything from this because we're just going to be adding our pagination headers onto a response.
        // this HttpResponse: the class that we're going extending.
        public static void AddPaginationHeader(this HttpResponse response, int currentPage, int itemPerPage, int totalItems, int totalPages)
        {
            // Create pagination header.
            // In parenthesis for parameters, order is important.
            var paginationHEader = new PaginationHeader(currentPage, itemPerPage, totalItems, totalPages);

            // Convert json to the Camel Case.
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Add pagination to response headers,
            // we need to serialize this because response headers, when we Add this, it takes a key and a string value.
            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHEader, options));
            // Because we're adding a custom header, we need to add a Expose header onto Pagination header.
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");

        }

    }
}
