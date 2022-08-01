import {Component, OnInit} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {User} from "./_models/user";
import {AccountService} from "./_services/account.service";
import {PresenceService} from "./_services/presence.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating App';
  users: any;

  constructor(
    private http: HttpClient,
    private accountService: AccountService,
    private presence: PresenceService
  ) {
  }

  ngOnInit() {
    this.setCurrentUser()
  }

  setCurrentUser() {
    // @ts-ignore
    const user: User = JSON.parse(localStorage.getItem('user'))

    if (user) {
      this.accountService.setCurrentUser(user)
      // We want to start the hub connection when a user logs in or when a user registers, and
      // we need to pass the user so that when we create it, we could get access to the users JWT token.
      this.presence.createHubConnection(user)
    }
  }




}
