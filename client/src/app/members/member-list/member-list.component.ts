import { Component, OnInit } from '@angular/core';
import {Member} from "../../_models/member";
import {MembersService} from "../../_services/members.service";
import {Observable} from "rxjs";

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  // Create a property to store the members, this is observable of member array
  // And what we'll use inside our member list component is we'll take advantage of the async pipe and we'll head over to the member list component templates.
  // $ conventionZ: this is now an observable rather than just a normal JavaScript object..
  members$!: Observable<Member[]>

  constructor(private memberService: MembersService) { }

  ngOnInit(): void {
    this.members$ = this.memberService.getMembers()
  }
}
