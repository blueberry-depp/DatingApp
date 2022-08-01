import {Injectable} from '@angular/core';
import {environment} from "../../environments/environment";
import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {ToastrService} from "ngx-toastr";
import {User} from "../_models/user";
import {BehaviorSubject, take} from "rxjs";
import {Router} from "@angular/router";

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl
  private hubConnection!: HubConnection

  // Create an observable, subject is a generic observable
  // <string[]>([]): array of username and initialize to an empty array.
  private onlineUsersSource = new BehaviorSubject<string[]>([])

  // This is going to be an observable.
  onlineUsers$ = this.onlineUsersSource.asObservable()

  constructor(private toastr: ToastrService, private router: Router
  ) {
  }

  // Create the hub connection so that when a user does connect to our application and they're authenticated,
  // then we're going to automatically create a hub connection that's going to connect them to our presence hub.
  // We pass our user, cecause we're going to need to send up user token for our JWT token when we make this connection as well. We
  // cannot use our JWT interceptor. These are no longer going to be HTTP requests. And typically they'll
  // be using WebSocket, which has no support for an authentication header.
  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}presence`, {
        accessTokenFactory(): string {
          return user.token
        }
      })
      .withAutomaticReconnect()
      .build()

    // Start the connection.
    this.hubConnection
      .start()
      .catch(error => console.log(error))

    // Listen for the server event for UserIsOnline methods and UserIsOffline methods,  we'll get access to the
    // username, because that's what we're returning from this method.
    this.hubConnection.on('UserIsOnline', username => {
      // Update the online users that we're tracking inside here, and we don't want to mutate
      // data here just in case that interferes with how angular tracks changes in our observable.
      this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
        this.onlineUsersSource.next([...usernames, username])
      })
    })

    this.hubConnection.on('UserIsOffline', username => {
      // Remove offline user from a list of online users.
      this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
        this.onlineUsersSource.next([...usernames.filter(x => x !== username)])
      })
    })

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsersSource.next(usernames)
    })

    // Receiving object.
    this.hubConnection.on('NewMessageReceived', ({username, knownAs}) => {
      this.toastr.info(`${knownAs} has sent you a new message!`)
        // Route them to a new location, tab 3 is message tab.
        .onTap
        .pipe((take(1)))
        .subscribe(() => this.router.navigateByUrl(`/members/${username}?tab=3`))
    })
  }

  stopHubConnection() {
    this.hubConnection.stop().catch(error => console.log(error))
  }
}
