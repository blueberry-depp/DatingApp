import { Injectable } from '@angular/core';
import {environment} from "../../environments/environment";
import {HttpClient, HttpHeaders, HttpParams} from "@angular/common/http";
import {Member} from "../_models/member";
import {of, pipe, take} from "rxjs";
import {map} from "rxjs/operators";
import {PaginatedResult} from "../_models/pagination";
import {UserParams} from "../_models/userParams";
import {AccountService} from "./account.service";
import {User} from "../_models/user";

// We no longer need to use the HTTP options because we use interceptor.
// The advantage now is that we never need to add this on to any request ever again,
// we do it in one place and then our interceptor is going to do this work for us for all time.
/*const httpOptions = {
  headers: new HttpHeaders({
    // @ts-ignore
    Authorization: `Bearer ${JSON.parse(localStorage.getItem('user'))?.token}`
  })
}*/

// Services are singletons, they're instantiated when a component needs the service,
// and that it operates as a singleton and it stays alive until the application is closed,
// so services make a good candidate for storing our application state,
// so what we can do is use our service to store our data and the term is caching data in Angular services,
// If we don't do caching data in services, we will make an API calls for every request when a more sensible way to go about
// it would be to store the data in a service. And because services are singletons, then our data remains


// and we can access that if we have it, rather than going out to the API every time.
@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl
  members: Member[] = []
  user!: User
  userParams!: UserParams

  // Implement cache.
  // When we want to store something with a key and value, a good thing to use is a Map object. And a map is like a dictionary,
  memberCache = new Map()

  constructor(private http: HttpClient,
              private accountService: AccountService
  ) {
    // We're going to need to get current user, because when we initialize this UserParams class,
    // what we need is user details so that we can set the gender property inside here.
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user
      // new UserParams(): because this is a class we need to instantiate it and pass the user as its parameter.
      this.userParams = new UserParams(user)
    })
  }

  // Helper methods so we can get and set these user params.
  getUserParams() {
    return this.userParams
  }

  setUserParams(params: UserParams) {
     this.userParams = params
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user)
    return this.userParams
  }

  // Use  this user params as some kind of key and then for each key, each query, we can store the response,
  getMembers(userParams: UserParams) {
    // console.log(Object.values(userParams).join('-'))

    // Check to see if we have in our cache the results of that particular query,
    // when using a Map, we can set, and we can get from this, and we store everything with a key.
    let response = this.memberCache.get(Object.values(userParams).join('-'))
    // If we do have a response for that key.
    if (response) {
      return of(response)
    }

    let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize)

    params = params.append('minAge', userParams.minAge.toString())
    params = params.append('maxAge', userParams.maxAge.toString())
    params = params.append('gender',  userParams.gender)
    params = params.append('orderBy',  userParams.orderBy)

    // Because we're returning an observable, we must use pipe and map is rxjs, we use it to transform the data that we get back.
    return this.getPaginatedResult<Member[]>(`${this.baseUrl}users`, params)
      .pipe(map(response => {
        // Object.values(userParams) is a key and the value is going to be the response we get back from our server.
        this.memberCache.set(Object.values(userParams).join('-'), response)
        return response
      }))

  }

  getMember(username: string) {
    // And what we'll do is we'll attempt to get the member from the members we have inside our service,
    // we still need to accommodate the fact that the user might refresh their page, and we don't have anything inside our service at all.
    // And if we don't have the member, then we're going to go and make our API call.
    // values() method returns value from Map object.
    const member = [...this.memberCache.values()]
      // arr: previous value, elem: each element in the array.
      // reduce: reduce array into something else, we want the results of each array in a single
      // array that we can then search and find the first member that matches a condition.
      .reduce((arr, elem) =>
        // []: initial value of empty array.
        arr.concat(elem.result), []
        // find: method is going to look for the first instance of this and return the username result to us.
    ).find((member: Member) => member.username === username)
    console.log(member)
    if (member) {
      return of(member)
    }
    return this.http.get<Member>(`${this.baseUrl}users/${username}`)
  }

  // Update our updateMember to use pipe, because if we're getting our members now from our service,
  // then if we update a member and we don't do anything with that, then when they go back, they're
  // going to go and see the old data that's stored inside here,
  // and finally after we updated member profile, we don't needing to go to the API again to go and get it.
  updateMember(member: Member) {
    return this.http.put(`${this.baseUrl}users/`, member).pipe(
      map(() => {
        // Get the member from the service and find the member that matches that member in index.
        const index = this.members.indexOf(member)
        this.members[index] = member
      } )
    )
  }

  setMainPhoto(photoId: number) {
    // In put request, we don't need to send something in the body. But what we can send up here is an empty object.
    return this.http.put(`${this.baseUrl}users/set-main-photo/${photoId}`, {})
  }

  deletePhoto(photoId: number) {
    return this.http.delete(`${this.baseUrl}users/delete-photo/${photoId}`)
  }

  addLike(username: string) {
    return this.http.post(`${this.baseUrl}likes/${username}`, {})
  }

  // We only need three thing: predicate, page number, page size.
  // Note for future implementation: create a separate class.
  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = this.getPaginationHeaders(pageNumber, pageSize)

    params = params.append('predicate', predicate)
    return this.getPaginatedResult<Partial<Member[]>>(`${this.baseUrl}likes`, params)
  }

  // <T> is generic type parameter.
  private getPaginatedResult<T>(url: any, params: any) {
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
    return this.http.get<T>(url, {observe: "response", params}).pipe(
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
  private getPaginationHeaders(pageNumber: number, pageSize: number) {
    // This gives us the ability to serialize parameters and this is going to take care of adding this on to query string.
    let params = new HttpParams()

    // pageNumber is query string, because a query string is a string we need to convert to string.
    params = params.append('pageNumber', pageNumber.toString())
    params = params.append('pageSize', pageSize.toString())

    return params
  }
}
