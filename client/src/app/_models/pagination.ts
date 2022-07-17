export interface Pagination {
  // This needs to match exactly the information that we saw inside the response header.
  currentPage: number
  itemPerPage: number
  totalItems: number
  totalPages: number
}

// Create class.
// This is going to be used for any of our different type and typescript also takes type
// parameters and we can specify T here, which means we can use paginated results with any of different types.
export class PaginatedResult<T> {
  // T for the initial example is going to represent an array of members(Member[]), but we'll call it T so we can use it for anything.
  result!: T | null
  // Pagination is a type of pagination the interface we've created above.
  pagination!: Pagination
}
