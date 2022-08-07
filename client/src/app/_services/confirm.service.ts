import { Injectable } from '@angular/core';
import {BsModalRef, BsModalService} from "ngx-bootstrap/modal";
import {RolesModalComponent} from "../modals/roles-modal/roles-modal.component";
import {ConfirmDialogComponent} from "../modals/confirm-dialog/confirm-dialog.component";
import {Observable} from "rxjs";

@Injectable({
  providedIn: 'root'
})
// We can use this service anywhere we want.
export class ConfirmService {
  bsModalRef!: BsModalRef

  constructor(private modalService: BsModalService) {
  }

  // Return an Observable from a subscription.
  confirm(title = 'Confirmation', message = 'Are sure you want to do this?',
          btnOkText = 'Ok', btnCancelText = 'Cancel'): Observable<boolean> {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        title,
        message,
        btnOkText,
        btnCancelText
      }
    }

    this.bsModalRef = this.modalService.show(ConfirmDialogComponent, config);

    return new Observable<boolean>(this.getResult())
  }

  // In order to get the result, we have to subscribe to it, but that doesn't help us return what we need to from the service, so
  // we must create a helper method,
  // when we subscribe to something, we get a subscription back, but we need to return that as an observable.
  private getResult() {
    // We're going to observe something inside what we're getting here.
    return (observer: any) => {
      // This is what we're going to be getting from the modal and, but we're not going to get it passed in our next inside this subscription.
      // onHidden: as soon as modal closes.
      const subscription = this.bsModalRef.onHidden!.subscribe(() => {
        // Setting up our own observable here that we're returning.
        observer.next(this.bsModalRef.content.result)
        observer.complete()
      })

      return {
        // Give it an unsubscribe method.
        unsubscribe() {
          subscription.unsubscribe()
        }
      }
    }
  }
}
