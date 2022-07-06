import { Injectable } from '@angular/core';
import {environment} from "../../environments/environment";
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {Member} from "../_models/member";
import {of} from "rxjs";
import {map} from "rxjs/operators";

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

  constructor(private http: HttpClient
  ) { }

  getMembers() {
    // We need to return this as an observable because our client, our component is going to be observing this data.
    // of: return something of an observable
    // So if we have the members, then we gonna return the members from the service as an observable.
    if (this.members.length > 0) return of(this.members)
    // pipe: Set the members once we've gone and made the effort to get them from the API.
    // map operator: returns the values as an observable.
    return this.http.get<Member[]>(`${this.baseUrl}users`).pipe(
      map(members => {
        this.members = members
        return members
      })
    )
  }

  getMember(username: string) {
    // And what we'll do is we'll attempt to get the member from the members we have inside our service,
    // we still need to accommodate the fact that the user might refresh their page and we don't have anything inside our service at all.
    const member = this.members.find(x => x.username === username) // Find the member with the same username that we're passing in as a parameter.
    if (member !== undefined) return of(member)
    // And if we don't have the member, then we're going to go and make our API call.
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
}
