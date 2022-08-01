import {Component, Input, OnInit} from '@angular/core';
import {Member} from "../../_models/member";
import {MembersService} from "../../_services/members.service";
import {ToastrService} from "ngx-toastr";
import {PresenceService} from "../../_services/presence.service";

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  // Receiving this data from its parent, which is the member list component.
  @Input() member!: Member

  constructor(
    private memberService: MembersService,
    private toastr: ToastrService,
    // We access the observable of the online users using an async pipe from template.
    public presence: PresenceService

  ) { }

  ngOnInit(): void {
  }

  addLike(member: Member) {
    // We're not returning anything from the API that we're going to use inside our client here, so we'll just have to empty parentheses.
    this.memberService.addLike(member.username).subscribe(() => {
      this.toastr.success(`You have liked ${member.knownAs}`)
    // we don't need to do anything with the error because interceptor is taking care of that for us.
    })
  }

}
