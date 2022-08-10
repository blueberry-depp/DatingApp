import {Injectable} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {environment} from "../../environments/environment";
import {getPaginatedResult, getPaginationHeaders} from "./paginationHelper";
import {Message} from "../_models/message";
import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {User} from "../_models/user";
import {BehaviorSubject, take} from "rxjs";
import {Group} from "../_models/group";
import {BusyService} from "./busy.service";

@Injectable({
  providedIn: 'root'
})

// What we want to do here is deal with what happens when we receive the messages, when we connect to this hub.
export class MessageService {
  baseUrl = environment.apiUrl
  hubUrl = environment.hubUrl
  private hubConnection!: HubConnection
  private messageThreadSource = new BehaviorSubject<Message[]>([])
  messageThread$ = this.messageThreadSource.asObservable()

  constructor(private http: HttpClient, private busyService: BusyService) {
  }

  createHubConnection(user: User, otherUsername: string) {
    this.busyService.busy()
    this.hubConnection = new HubConnectionBuilder()
      // Pass up the other username as a query string with the key of users.
      .withUrl(`${this.hubUrl}message?user=${otherUsername}`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build()

    // Start the connection.
    // We have here is a promise that's returned from the start().
    this.hubConnection.start()
      .catch(error => console.log(error))
      .finally(() => this.busyService.idle())

    // Listen for the server event for ReceiveMessageThread methods, we'll get the messages.
    // We're going to get the message thread when we join a message group.
    // We want to make sure of is that if we join another message group, then that user automatically
    // marks the messages as red because when we join it, that means we're reading the messages at the same time.
    this.hubConnection.on('ReceiveMessageThread', messages => {
      this.messageThreadSource.next(messages)
    })

    // What we're doing is we're receiving a new message from our message hub and what we need to do inside client is update
    // message thread observable to show this new message when it is received,
    // we want to slightly careful with what we do to add on new message to this messageThreadSource's array and so that everything behaves correctly,
    // and what we're going to do is not mutate this messageThreadSource's array, we're going to get hold of the messages. We're going
    // to add the new message on, and then we're going to update the BehaviorSubject with a new message array that includes the message we've just received.
    // Any new message that a user receives is automatically going to be marked as sent.
    this.hubConnection.on('NewMessage', message => {
      this.messageThread$.pipe(take(1)).subscribe(messages => {
        // The way that we can do this without mutating the array is to make use of the spread operator. Now this is
        // going to create a new array that's going to populate behavior subject. So we're not going to be mutating state inside here.
        this.messageThreadSource.next([...messages, message])
      })
    })

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      // We want to take a look inside messageThread$ and see if there's any unread messages for the user that just join this group,
      // and if there are, then we're going to mark them as read inside here.
      if (group.connections.some(x => x.username === otherUsername)) {
        this.messageThread$.pipe(take(1)).subscribe(messages => {
          messages.forEach(message => {
            // If not read.
            if (!message.dateRead) {
              message.dateRead = new Date(Date.now())
            }
          })
          // Pass the updated messages now. This creates a new array, so it shouldn't interfere with angular change tracking.
          this.messageThreadSource.next([...messages])
        })
      }
    })
  }

  // Stopping the connection when the user moves away from the member detail component.
  stopHubConnection() {
    // Add safety conditional so that we only try and stop it if it is actually in existence due to method inside ngDestroy.
    if (this.hubConnection) {
      // Clear the messages when the hub connection is stopped.
      this.messageThreadSource.next([])
      this.hubConnection.stop().catch(error => console.log(error))
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: any) {
    let params = getPaginationHeaders(pageNumber, pageSize)
    params = params.append('Container', container)

    return getPaginatedResult<Message[]>(`${this.baseUrl}messages`, params, this.http)
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${username}`)
  }

  // username: who we're sending the message to.
  // Convert to async function because it's return promise not observable again because we use invoke method, we need to use the promise, that return, because in
  // member message component, we're resetting the form as well once the message has gone.
  async sendMessage(username: string, content: string) {
    // The way that we execute a method on server is to use the invoke method, and it returns promise.
    // For the body content, we past the object and must match to the CreateMessageDto.

      return await this.hubConnection.invoke('SendMessage', {recipientUsername: username, content})
        .catch(error => console.log(error));

  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}messages/${id}`)

  }
}
