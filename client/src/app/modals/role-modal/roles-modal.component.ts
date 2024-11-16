import { Component } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent {
  username = '';
  availableRoles: any[] = [];
  selectedRoles: any[] = [];

  // Notice that this property bsModalRef is made public, as it is used in the HTML component (requirement from ngx-bootstrap/modal)
  constructor(public bsModalRef: BsModalRef){
  }

  updateCheckbox(checkedValue: string){
    const index = this.selectedRoles.indexOf(checkedValue);

    index !== -1 ? this.selectedRoles.splice(index, 1) : this.selectedRoles.push(checkedValue);
  }

}
