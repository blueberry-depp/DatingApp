import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, CanDeactivate, RouterStateSnapshot, UrlTree} from '@angular/router';
import {Observable} from 'rxjs';
import {MemberEditComponent} from "../members/member-edit/member-edit.component";

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<any> {
  // We need return boolean
  canDeactivate(
    // this is give us access to edit form because need to check the status of the form inside here.
    component: MemberEditComponent): boolean {
    if (component.editForm.dirty) {
      return confirm('Are you sure you want to continue? Any unsaved changes Will be lost')
    }
    // If yes, we can deactivate this component
    return true;
  }

}
