import { Injectable } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {environment} from "../../environments/environment";
import {MembersService} from "./members.service";
import {getPaginatedResult, getPaginationHeaders} from "./paginationHelper";
import {Member} from "../_models/member";
import {map} from "rxjs/operators";
import {Message} from "../_models/message";

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl

  constructor(private http: HttpClient, ) { }

  getMessages(pageNumber: number, pageSize: number, container: any) {
    let params = getPaginationHeaders(pageNumber, pageSize)
    params = params.append('Container', container)

    return getPaginatedResult<Message[]>(`${this.baseUrl}messages`, params, this.http)
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${username}`)
  }

  // username: who we're sending the message to.
  sendMessage(username: string, content: string) {
    // <Message>: we know we're going to get a message dto back from this and pass message is what we're getting.
    // For the body content, we past the object and must match to the CreateMessageDto.
    return this.http.post<Message>(`${this.baseUrl}messages/`, {recipientUsername: username, content} )
  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}messages/${id}` )

  }
}
