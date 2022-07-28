import {Directive, Input, OnInit, TemplateRef, ViewContainerRef} from '@angular/core';
import {AccountService} from "../_services/account.service";
import {take} from "rxjs";
import {User} from "../_models/user";

// This is a structural directive. It's a custom directive.
@Directive({
  selector: '[appHasRole]' // *appHasRole="['Admin']"
})
export class HasRoleDirective implements OnInit {
  // So we can get access to those parameters.
  @Input() appHasRole: string[] = []
  user!: User

  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private accountService: AccountService,
  ) {
    // Get access to current user.
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user
    })
  }

  ngOnInit(): void {
    // Clear a view if no roles so the user doesn't have any roles, then we're just going to clear the container,
    // check if they've got no roles, then it's going to be optional. There should be no user that have this, but anyway, we'll check.
    // this.user === null: they're not authenticated effectively.
    if (!this.user?.roles || this.user == null) {
      this.viewContainerRef.clear()
      return
    }

    // Check the user has some roles.
    if (this.user?.roles.some(r => this.appHasRole.includes(r))) {
      // If the user has a role that's in that list, then we're going to create this embedded view.
      this.viewContainerRef.createEmbeddedView(this.templateRef)
    } else {
      // If they're not in that role, we're going to clear it, so they do not see the admin link.
      this.viewContainerRef.clear()
    }

  }

}
