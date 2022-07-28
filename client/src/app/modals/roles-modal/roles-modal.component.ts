import {Component, EventEmitter, Input, OnInit} from '@angular/core';
import {BsModalRef} from "ngx-bootstrap/modal";
import {User} from "../../_models/user";

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {
  // Receive something from component and emit the roles from this particular component.
  @Input() updateSelectedRoles = new EventEmitter
  user!: User
  roles: any[] = []

  // We're passing property above through BsModalRef.
  constructor(public bsModalRef: BsModalRef) { }

  ngOnInit(): void {
  }
  updateRoles() {
    this.updateSelectedRoles.emit(this.roles)
    // Turn off the modal
    this.bsModalRef.hide()
  }

}
