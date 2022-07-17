import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import {catchError, Observable, throwError} from 'rxjs';
import {ToastrService} from "ngx-toastr";
import {NavigationExtras, Router} from "@angular/router";

@Injectable()
// Because this is an interceptor, we need to provide this in our app module.
export class ErrorInterceptor implements HttpInterceptor {

  constructor(
    private router: Router,
    private toastr: ToastrService
  ) {}

  // We want to catch any errors from this
  // The request, what we get back from this is unobservable, so in order to do something with this, like
  // any other observable, we're going to need to use the pipe method to do whatever functionality we want inside here,
  // we're going to use Rxjs feature called Catch Error.
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError(error => {
        if (error) {
          // Use switch statement so that we can switch depending on what status something is.
          // The idea being is that for the majority of cases when we're going to catch them inside this switch
          // statement, if we don't catch it, we're going to return the error to whatever was calling the HTTP request in the first place.
          switch (error.status) {
            // Start with tricky one: 400
            case 400:
              if (error.error.errors) {
                const modalStateErrors = []
                for (const key in error.error.errors) {
                  if (error.error.errors[key]) {
                    // Flatten the array of errors that we get back from our validation responses and push them into an array.
                    modalStateErrors.push(error.error.errors[key])
                  }
                }
                // We want to display a list of validation errors underneath the form.
                throw modalStateErrors.flat()
              }
                else if (typeof(error.error) === 'object') {
                // If it's just a normal 400 error.
                // Check if the error object is an object.
                this.toastr.error(error.statusText === "OK" ? "Bad Request" : error.statusText, error.status);
              } else {
                this.toastr.error(error.error, error.status);
                console.log(error.error)
              }
              break

            case 401:
              this.toastr.error(error.statusText === "OK" ? "Unauthorised" : error.statusText, error.status);
              break

            case 404:
              this.router.navigateByUrl('/not-found')
              break

            // Redirect them to a server error page, but what we want to do is also get the details
            // of the error that we're going to get returned from the API,
            // use a feature of the router where we can pass it, some states.
            case 500:
              // error.error: exception that we get back from our API
              const navigationExtras: NavigationExtras = {state: {error: error.error}}
              // Pass in the navigationExtras as the states
              this.router.navigateByUrl('/server-error', navigationExtras)
              break

            default:
              this.toastr.error('something unexpected went wrong.')
              console.log(error) // Give us an opportunity to go and tweak our interceptor because we need to add a new case to take care of that.
              break
          }
        }
        // We shouldn't ever hit this low.
        return throwError(error)
      })
    );
  }
}
