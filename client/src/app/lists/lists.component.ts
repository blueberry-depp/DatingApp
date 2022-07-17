import { Component, OnInit } from '@angular/core';
import {Member} from "../_models/member";
import {MembersService} from "../_services/members.service";
import {Pagination} from "../_models/pagination";

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  // Not quite full members, so what we'll use we used it before when we were configuring the date picker,
  // but we're going to specify partial so each one of the properties inside the member is now going to be optional.
  members!: Partial<Member[]> | null
  predicate = 'liked'
  pageNumber = 1
  pageSize = 5
  pagination!: Pagination

  constructor(private memberService: MembersService,
  ) { }

  ngOnInit(): void {
    this.loadLikes()
  }

  loadLikes() {
    this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize).subscribe(response => {
      console.log(response)
      this.members = response.result
      this.pagination = response.pagination
    })
  }

  pageChanged(event: any) {
    this.pageNumber = event.page
    this.loadLikes()
  }

}
