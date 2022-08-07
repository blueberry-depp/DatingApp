import {ChangeDetectionStrategy, Component, Input, OnInit, ViewChild} from '@angular/core';
import {Message} from "../../_models/message";
import {MessageService} from "../../_services/message.service";
import {NgForm} from "@angular/forms";

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
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
  loading = false

  // We'll use the async pipe to subscribe to the messages.
  // Make this public, so we can access it from components template.
  constructor(public messageService: MessageService) {
  }

  ngOnInit(): void {
  }

  sendMessage() {
    this.loading = true
    // We're not going to push the message anymore either. We can simply be receiving this from signalR hub.
    // We use then when we're using promises.
    this.messageService.sendMessage(this.username, this.messageContent).then(() => {
      this.messageForm.reset()
    }).finally(() => this.loading = false)
  }


}
