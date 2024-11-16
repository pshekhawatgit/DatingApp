import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';
import { RolesModalComponent } from 'src/app/modals/role-modal/roles-modal.component';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  bsModalRef: BsModalRef<RolesModalComponent> = new BsModalRef<RolesModalComponent>();

  constructor(private adminService: AdminService, 
    private modalService: BsModalService){
  }

  ngOnInit(): void {
    this.getusersWithRoles();
  }

  getusersWithRoles(){
    this.adminService.getUsersWithRoles().subscribe({
      next: users => {
        this.users = users;
      }
    })
  }

  openRolesModal(){
    const initialState: ModalOptions = {
      initialState: {
        list: ['Do thing', 'Another Thing', 'Something else'],
        title: 'test modal'
      }
    }
    this.bsModalRef = this.modalService.show(RolesModalComponent, initialState);
    this.bsModalRef.content!.closeBtnName = 'Close'
  }
}
