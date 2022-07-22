import { Injectable } from '@angular/core';
import {
  Router, Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot
} from '@angular/router';
import { Observable, of } from 'rxjs';
import {Member} from "../_models/member";
import {MembersService} from "../_services/members.service";

// Because this is not a component, we need to add the injectable operator onto this.
@Injectable({
  providedIn: 'root'
})
// NOTE: there's no way to access navigation extras inside resolver. And that's the reason we did the
// extra work earlier on to go and get the member from cache.
export class MemberDetailedResolver implements Resolve<Member> {

  constructor(private memberService: MembersService) {
  }
  resolve(route: ActivatedRouteSnapshot): Observable<Member> {
    // Get the member.
    // We don't need to subscribe inside resolvers because the router is going to subscribe automatically and deal with unsubscription as well,
    // and we can use these for anything, we have a reason to use this now, but you can use this to go and get any type
    // of data you want if you want to get your data before you constructs your template.
    // username: url parameter in app-routing-module.
    return this.memberService.getMember(<string>route.paramMap.get('username'))


  }
}
