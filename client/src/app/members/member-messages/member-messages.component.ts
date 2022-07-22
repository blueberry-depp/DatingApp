import {Component, Input, OnInit, ViewChild} from '@angular/core';
import {Message} from "../../_models/message";
import {MessageService} from "../../_services/message.service";
import {NgForm} from "@angular/forms";

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  // Use the ViewChild to get access to the messageForm from this component.
  @ViewChild('messageForm') messageForm!: NgForm
  // We just need a username, what's the username of the member that we've just clicked on,
  // because that's what we're going to send up to get the thread itself.
  @Input() username!: string;
  @Input() messages: Message[] = []
  messageContent!: string


  constructor(private messageService: MessageService) {
  }

  ngOnInit(): void {
  }

  sendMessage() {
    // We get message back from subscribe.
    this.messageService.sendMessage(this.username, this.messageContent).subscribe(message => {
      // We push the new message that we've created inside here so that we can see it in the array of messages
      this.messages.push(message)
      this.messageForm.reset()
    })
  }


}
