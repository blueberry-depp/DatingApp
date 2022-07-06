import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import {AccountService} from "../_services/account.service";
import {ToastrService} from "ngx-toastr";
import {map} from "rxjs/operators";

@Injectable({
  providedIn: 'root'
})
// The AuthGuard will subscribe automatically, so we don't to subscribe it
export class AuthGuard implements CanActivate {
  constructor(
    private accountService: AccountService,
    private toastr: ToastrService
  ) {
  }

  canActivate(): Observable<boolean> {
    // Check the current user observable if there's authenticated user
    return this.accountService.currentUser$.pipe(
      map(user => {
        if (user) return true
        this.toastr.error('You shall not pass!')
        return false
      })

    )

  }

}
