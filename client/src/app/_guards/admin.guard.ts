import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, UrlTree} from '@angular/router';
import {Observable} from 'rxjs';
import {AccountService} from "../_services/account.service";
import {ToastrService} from "ngx-toastr";
import {map} from "rxjs/operators";

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(
    private accountService: AccountService,
    private toastr: ToastrService
  ) {
  }

  canActivate(): Observable<boolean> {
    // Check to see if the user either of these roles based on the information that's stored inside a token, user roles is an array,
    // so we can use the includes method to see if they are included in that.
    return this.accountService.currentUser$.pipe(
      map(user => {
          if (user.roles.includes('Admin') || user.roles.includes('Moderator')) return true
          this.toastr.error('You cannot enter this area')
          return false
        })
    )
  }
}
