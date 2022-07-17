import {Component, HostListener, OnInit, ViewChild} from '@angular/core';
import {Member} from "../../_models/member";
import {User} from "../../_models/user";
import {MembersService} from "../../_services/members.service";
import {AccountService} from "../../_services/account.service";
import {take} from "rxjs";
import {ToastrService} from "ngx-toastr";
import {NgForm} from "@angular/forms";

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
// What we'll need to do in here is get hold of the current user from account service,
// and we'll use that to get the username for that user, so we can go and fetch that particular member.
export class MemberEditComponent implements OnInit {
  // editForm: specify the child, this gives us access to this template form inside our components.
  @ViewChild('editForm') editForm!: NgForm
  member!: Member
  // Populate this user object with our current user from our account service, remember, the current user is an observable.
  user!: User // Now our user will have the current user from our account service.
  // This allows us to access our browser events and the browser events that we want to access is the window:beforeunload,
  // so if user close/move from the app while still editing, there will be a prompt,
  // so if we want to do something before the browser is closed, then we've got an option to do so via this host listener.
  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true
    }
  }

  constructor(
    private accountService: AccountService,
    private memberService: MembersService,
    private toastr: ToastrService
  ) {
    // What we need to do is get the user out of that observable and use that for our 'user',
    // we need to access this object inside our component, and we can't really work with an observable to do that. So we need to get it out of there.
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user)
  }

  ngOnInit(): void {
    this.loadMembers()
  }

  loadMembers() {
    // username: url parameter in app-routing-module
    this.memberService.getMember(this.user.username).subscribe(member => {
      this.member = member
    })
  }

  updateMember() {
    this.memberService.updateMember(this.member).subscribe(() => {
      this.toastr.success('Profile updated successfully')
      // In order to keep and preserve the values that we have in form, we'll pass in this.member,
      // and this is going to be the updated member after we've submitted form.
      this.editForm.reset(this.member)

    })
  }

}
