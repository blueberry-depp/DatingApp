import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {map} from "rxjs/operators";
import {User} from "../_models/user";
import {ReplaySubject} from "rxjs";
import {environment} from "../../environments/environment";

// Note: Services are injectable,
// Services are singleton,
// Component are different when we move from component to component, there are destroyed as soon as they're not in use
@Injectable({
  providedIn: 'root'
})
export class AccountService {
  // Angular is going to automatically use the right one depending on which environment we're in.
  baseUrl = environment.apiUrl

  // Create a special observable, store the User, and store just 1 value(user)/
  // it's size of buffer because we just need 1 user/current user object
  private currentUserSource = new ReplaySubject<User>(1)
  // currentUser$: By convention because this is going to be an observable we give it the dollar sign.
  currentUser$ = this.currentUserSource.asObservable()

  constructor(
    private http: HttpClient
  ) {
  }

  login(model: any) {
    return this.http.post(`${this.baseUrl}account/login`, model).pipe(
      map((response: any) => { // Temporary: map((response: User) => {
        const user = response
        if (user) {
          this.setCurrentUser(user)
        }
      }))
  }

  register(model: any) {
    return this.http.post(`${this.baseUrl}account/register`, model).pipe(
      map((user: any) => {
        if (user) {
          this.setCurrentUser(user)
        }
      }))
  }

  // Set the current user.
  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user))
    // Set the user.
    this.currentUserSource.next(user)
  }

  logout() {
    localStorage.removeItem('user')
    // @ts-ignore
    this.currentUserSource.next(null)


  }
}
