import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import {Observable, take} from 'rxjs';
import {AccountService} from "../_services/account.service";
import {User} from "../_models/user";

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private accountService: AccountService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // In order to use the token from inside there, we're going to need to get the current user outside of that observable.
    // We don't have a current user yet.
    // After subscribe, now we have the contents of that current user or null.
    let currentUser: User

    // We need to subscribe to go and get what's inside the observable out of the observable,
    // we're saying that we want to complete after we've received one of these current users,
    // and this way, we don't need to unsubscribe because once an observable has completed, then we are effectively not subscribe to it anymore,
    // so whenever we're not sure if we need to unsubscribe from something, then what we can do is just simply
    // add that pipe and then take one in this case and then we can go and subscribe and we kind of guarantee we unsubscribe from that.
    // I'm not saying it's a big deal if we didn't do this, by the way, because our interceptors are going
    // to be initialized when we start our application because they're part of our module and we add them to
    // the providers and they're always going to be around until we close our application,
    // so whether we subscribe or unsubscribe from this one isn't a big deal,
    // but what we will do is use this technique continuously to ensure that anywhere else we use this, we do take care of completing our subscription.
    // Shorten current user = user, we set our current user from the account service to this current user variable
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => currentUser = user)
    // @ts-ignore
    if (currentUser) {
      // We want to clone this request and add our authentication header onto it,
      // because we've clone this request here when we return from this, it's that request if we're logged
      // in, that's going to receive our authorization header and send this up with our request.
      request = request.clone({
        setHeaders: {
          // Attach our token for every request when we're logged in and send that up with our request.
          Authorization: `Bearer ${currentUser.token}`
        }
      })
    }
    return next.handle(request);
  }
}
