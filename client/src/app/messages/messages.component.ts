import { Component, OnInit } from '@angular/core';
import {Message} from "../_models/message";
import {Pagination} from "../_models/pagination";
import {MessageService} from "../_services/message.service";

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[] | null   = []
  pagination!: Pagination
  container = 'Unread'
  pageNumber = 1
  pageSize = 2
  loading = false

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
    this.loadMessages()
  }

  // This is going to be paginated results response.
  loadMessages() {
    this.loading = true
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe(response => {
      this.messages = response.result
      this.pagination = response.pagination
      this.loading = false
    })
  }

  deleteMessage(id: number) {
    // We don't get anything back from deleted. We use splice to find the index and specify how many messages we want to delete
    this.messageService.deleteMessage(id).subscribe(() => {
      this.messages?.splice(this.messages?.findIndex(m => m.id === id), 1)
    })
  }

  // Add the page changed events because we're adding the pagination.
  pageChanged(event: any) {
    // We must check page number against page event to avoid infinite loop/loading and only then are we going
    // to attempt to change the page number and load the messages.
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page
      this.loadMessages()
    }
  }



}
