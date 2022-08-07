import { Component, OnInit } from '@angular/core';
import {BsModalRef} from "ngx-bootstrap/modal";

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.css']
})
export class ConfirmDialogComponent implements OnInit {
  title?: string
  message?: string
  btnOkText?: string
  btnCancelText?: string
  // Store whatever they select as the result, which is basically true or false. Do you want to continue? or Do you want to cancel?.
  result?: boolean

  // inject bsModalRef because we access this inside in template as well.
  constructor(public bsModalRef: BsModalRef) { }

  ngOnInit(): void {
  }

  confirm() {
    // If they confirm we'll set this results equal to true.
    this.result = true
    this.bsModalRef.hide()
  }

  decline() {
    this.result = false
    this.bsModalRef.hide()
  }

}
