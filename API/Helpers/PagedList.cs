using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace API.Helpers
{
    // <T>: it can take any type of entity, And that's what we mean when we specify a type, we're going to say,
    // we're going to have a type in here and <T> could be MemberDto, for instance, and it gets swapped
    // at compile time, depending on what we use with our PagedList. And our PagedList
    // is going to be a type of list that inherit from the List from systems.collections.generic,
    // and List<T> also takes a type parameter and will make it the same, a list of T, which could mean a list of users, a list of members, etc..
    public class PagedList<T> : List<T>
    {
        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            TotalCount = count;
            // So that we have access to these items inside our page list.
            AddRange(items);
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        // Return the task of PagedList of type whatever this is.
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            // How many items are left from this query.
            // source.CountAsync(): this does make a database call. It's unavoidable,
            // if we want to work out the total number of records that we're going to return, we have to use this
            // count because what we get back from the database is not gonna to equal what the total count of available,
            // so there's no avoiding making two separate queries from this.
            var count = await source.CountAsync();
            // ToListAsync(): execute the query.
            // So if we're on page number one, for instance, then we say page number one or page number minus one
            // gives us 0 times by the page size, which could be five, then we're going to say we're going
            // to skip no records and we're going to take, let's say, five.
            // For example, if we were on page number two, then it would minus one to make it one times by the page size, which could be five,
            // and then we're going to take five, so we will be on the second page of the next five records and execute to a list.
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

    }
}
