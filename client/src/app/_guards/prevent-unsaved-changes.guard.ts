import {Injectable} from '@angular/core';
import {CanDeactivate} from '@angular/router';
import {Observable} from 'rxjs';
import {MemberEditComponent} from "../members/member-edit/member-edit.component";
import {ConfirmService} from "../_services/confirm.service";

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<any> {
  constructor(private confirmService: ConfirmService) {
  }

  // We need return boolean
  canDeactivate(
    // This is give us access to edit form because need to check the status of the form inside here.
    // We want return true, not boolean true. so we give union type "| boolean".
    // We didn't need to subscribe to this because we're inside a route guard and the guard is automatically going to subscribe for us.
    component: MemberEditComponent): Observable<boolean> | boolean {
    if (component.editForm.dirty) {
      return this.confirmService.confirm()
    }
    // We want return simply true, not boolean true. So that they can move on to a different component.
    return true;
  }

}
