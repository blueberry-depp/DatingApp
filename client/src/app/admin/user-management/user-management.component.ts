import {Component, OnInit} from '@angular/core';
import {User} from "../../_models/user";
import {AdminService} from "../../_services/admin.service";
import {BsModalRef, BsModalService} from "ngx-bootstrap/modal";
import {RolesModalComponent} from "../../modals/roles-modal/roles-modal.component";

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: Partial<User[]> = []
  // Add a reference to a modal.
  bsModalRef?: BsModalRef;

  constructor(
    private adminService: AdminService,
    private modalService: BsModalService
  ) {
  }

  ngOnInit(): void {
    this.getUsersWithRoles()
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe(users => {
      this.users = users
    })
  }

  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user,
        // What we need for this is we need to know which roles the user is in so that we can populate the checkboxes that are already checked,
        // and for the roles that the user is not in, we need to make them available to be shown inside the modal in an unchecked state.
        roles: this.getRolesArray(user)
      }
    }

    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.content.updateSelectedRoles.subscribe((values: any[]) => {
      const roleToUpdate = {
        // Set this to all the values that have been checked, that we get back from our modal. Everything
        // that's been checked is going to values.
        // Spread the contents of the values and filter out anything that hasn't been checked.
        roles: [...values.filter((el: { checked: boolean }) => el.checked).map((el: { name: string }) => el.name)]
      }
      // Check to make sure that we have something to update.
      if (roleToUpdate) {
        this.adminService.updateUserRoles(user.username, roleToUpdate.roles).subscribe(() => {
          user.roles = [...roleToUpdate.roles]

        })
      }
    })
  }

  private getRolesArray(user: User) {
    const roles: string[] = []
    const userRoles = user.roles
    const availableRoles: any[] = [
      {name: 'Admin', value: 'Admin'},
      {name: 'Moderator', value: 'Moderator'},
      {name: 'Member', value: 'Member'},
    ]

    // Loop over availableRoles and find out if user is a member of those available roles,
    // if they are, then we're going to check the checkbox, basically.
    availableRoles.forEach(role => {
      // Check to see if the user role matches this role.
      let isMatch = false
      // Kind of a loop within a loop is what we're doing here.
      for (const userRole of userRoles) {
        if (role.name === userRole) {
          // If it is a match.
          isMatch = true
          role.checked = true
          roles.push(role)
          // Because we're in a loop within a loop, we're going to break out of this.
          break
        }
      }

      // Check to see if the roll is not a match.
      if (!isMatch) {
        role.checked = false
        roles.push(role)
      }
    })

    return roles

  }

}
