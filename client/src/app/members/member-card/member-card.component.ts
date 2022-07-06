import {Component, Input, OnInit} from '@angular/core';
import {Member} from "../../_models/member";

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  // Receiving this data from its parent, which is the member list component.
  // @ts-ignore
  @Input() member: Member

  constructor() { }

  ngOnInit(): void {
  }

}
