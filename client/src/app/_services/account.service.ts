import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {map} from "rxjs/operators";
import {User} from "../_models/user";
import {ReplaySubject} from "rxjs";
import {environment} from "../../environments/environment";
import {tokenize} from "ngx-bootstrap/typeahead";

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
    user.roles = []
    const roles = this.getDecodedToken(user.token).role
    // Check to see if the roles that we have here been an array or just a simple string. If not we push the role into an array.
    Array.isArray(roles) ? user.roles = roles : user.roles.push(roles)

    localStorage.setItem('user', JSON.stringify(user))
    // Set the user.
    this.currentUserSource.next(user)
  }

  logout() {
    localStorage.removeItem('user')
    // @ts-ignore
    this.currentUserSource.next(null)
  }

  // Get the information inside the token.
  getDecodedToken(token: string) {
    // Token come in three part, we have  header, we have payload, and then the signature,
    // the part that we're interested is the payload([1] is index).
    return JSON.parse(atob(token.split('.')[1]))
  }
}
