import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { initialState } from 'ngx-bootstrap/timepicker/reducer/timepicker.reducer';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModalRef?: BsModalRef<ConfirmDialogComponent>;

  constructor(private modalService: BsModalService) { }

  confirm(
    title = 'Confirmation', 
    message = 'Are you sure you want to navigate away from this page?', 
    btnOktext = 'ok', 
    btnCancelText='Cancel') : Observable<boolean>
    {
    const config = {
      initialState : {
        title,
        message,
        btnOktext,
        btnCancelText
      }
    }
    this.bsModalRef = this.modalService.show(ConfirmDialogComponent, config);
    
    return this.bsModalRef.onHidden!.pipe(
      map( () => {
        return this.bsModalRef!.content!.result;
      })
    )
  }
}
