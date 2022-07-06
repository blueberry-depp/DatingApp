import {Injectable} from '@angular/core';
import {NgxSpinnerService} from "ngx-spinner";

@Injectable({
  providedIn: 'root'
})

// How to use this spinner service? we've got a facility in Angular to intercept requests as they go in and they go out and
// we can use an interceptor to take care of this.
export class BusyService {
  busyRequestCount = 0

  constructor(private spinnerService: NgxSpinnerService) {
  }

  busy() {
    this.busyRequestCount++
    // Specify undefine because we're not going to give each spinner a name and then add some configuration properties.
    this.spinnerService.show(undefined, {
      type: 'line-scale-party',
      bdColor: 'rgba(255,255,255,0)',
      color: '#333333'
    })
  }

  idle() {
    this.busyRequestCount--
    if (this.busyRequestCount <= 0) {
      this.busyRequestCount = 0
      this.spinnerService.hide()
    }
  }
}
