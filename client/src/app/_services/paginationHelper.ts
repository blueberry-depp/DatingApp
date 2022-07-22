import {PaginatedResult} from "../_models/pagination";
import {map} from "rxjs/operators";
import {HttpClient, HttpParams} from "@angular/common/http";

// <T> is generic type parameter.
// We need pass in http as parameter here.
export function getPaginatedResult<T>(url: any, params: any, http: HttpClient) {
  // Also initialize it and store the results in.
  const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>()

  // We need to return this as an observable because our client, our component is going to be observing this data.
  // of: return something of an observable
  // So if we have the members, then we gonna return the members from the service as an observable. This is work as the caching.
  // if (this.members.length > 0) return of(this.members)
  // pipe: Set the members once we've gone and made the effort to get them from the API.
  // map operator: returns the values as an observable.
  // observe: "response": when we're observing the response and we use params to pass the parameters,
  // then we get the full response back. We must get it the full response back by ourselves.
  return http.get<T>(url, {observe: "response", params}).pipe(
    map(response => {
      // Members[] is going to be contained inside response.body.
      paginatedResult.result = response.body
      // Check pagination headers.
      if (response.headers.get('Pagination') !== null) {
        paginatedResult.pagination = JSON.parse(<string>response.headers.get('Pagination'))
      }

      return paginatedResult
    })
  )
}

// To make life a bit easier we'll create a private method so that we can just go and get these pagination headers.
export function getPaginationHeaders(pageNumber: number, pageSize: number) {
  // This gives us the ability to serialize parameters and this is going to take care of adding this on to query string.
  let params = new HttpParams()

  // pageNumber is query string, because a query string is a string we need to convert to string.
  params = params.append('pageNumber', pageNumber.toString())
  params = params.append('pageSize', pageSize.toString())

  return params
}
