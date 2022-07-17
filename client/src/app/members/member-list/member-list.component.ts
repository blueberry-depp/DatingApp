import { Component, OnInit } from '@angular/core';
import {Member} from "../../_models/member";
import {MembersService} from "../../_services/members.service";
import {Observable, take} from "rxjs";
import {Pagination} from "../../_models/pagination";
import {UserParams} from "../../_models/userParams";
import {AccountService} from "../../_services/account.service";
import {User} from "../../_models/user";

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  // Create a property to store the members, this is observable of member array
  // And what we'll use inside our member list component is we'll take advantage of the async pipe and we'll head over to the member list component templates.
  // $ conventionZ: this is now an observable rather than just a normal JavaScript object..
  // members$!: Observable<Member[]>
  members!: Member[] | null
  // Store inside here is pagination information.
  pagination!: Pagination
  userParams!: UserParams
  user!: User
  // Also give it key and value.
  genderList = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}]

  constructor(private memberService: MembersService) {
    // Get userParams from memberService
    this.userParams = this.memberService.getUserParams()
  }

  ngOnInit(): void {
    this.loadMembers()
  }

  loadMembers() {
    // But we're still updating these inside this components, so if a user selects a filter, then we're going to need to update the user parameters inside the service.
    this.memberService.setUserParams(this.userParams)

    // username: url parameter in app-routing-module
    // Do not worry about piping(pipe) this and taking one. In this case, this is a HTP response and hasty response is typically complete,
    // it's not compulsory or at least in this training course, it's not compulsory to always use the pipe to go and just take one. We'll just keep things simple.
    this.memberService.getMembers(this.userParams).subscribe(response => {
      // console.log(response)
      this.members = response.result
      this.pagination = response.pagination
    })
  }

  resetFilters() {
    // Reset the params to what they are initialized at, we must reset this from the service too.
    this.userParams = this.memberService.resetUserParams()
    this.loadMembers()
  }

  pageChanged(event: any) {
    this.userParams.pageNumber = event.page
    // We need to remember the user params as well as we change the page number from the component.
    this.memberService.setUserParams(this.userParams)
    this.loadMembers()
  }


}
