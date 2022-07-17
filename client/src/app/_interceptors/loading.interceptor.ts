import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import {delay, finalize, Observable} from 'rxjs';
import {BusyService} from "../_services/busy.service";

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busyService: BusyService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // When we're about to send request, we're going to call the following method.
    this.busyService.busy()
    // And once the request comes back, we know it's completed, so we can turn off our busy Spinner.
    return next.handle(request).pipe(
      // Add fake delay.
      delay(500),
      // This gives to do something when things have completed.
      finalize(() => {
        this.busyService.idle()
      })
    );
  }
}
