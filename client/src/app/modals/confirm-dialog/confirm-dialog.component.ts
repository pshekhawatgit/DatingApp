import { Component } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.css']
})
export class ConfirmDialogComponent {
  title = '';
  message = '';
  btnOktext = '';
  btnCancelText = '';
  result = false;
  // Made bsModalRef public as we need to access this in template
  constructor(public bsModalRef: BsModalRef){}

  confirm(){
    this.result = true;
    this.bsModalRef.hide();
  }

  decline(){
    this.bsModalRef.hide();
  }

}
