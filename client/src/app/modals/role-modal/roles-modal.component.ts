import { Component } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent {
  // These 3 properties are declared as they are used in the HTML component (requirement from ngx-bootstrap/modal)
  title = '';
  list: any;
  closeBtnName = '';
  // Notice that this property bsModalRef is made public, as it is used in the HTML component (requirement from ngx-bootstrap/modal)
  constructor(public bsModalRef: BsModalRef){
  }

}
