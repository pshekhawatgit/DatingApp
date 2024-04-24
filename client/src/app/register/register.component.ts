import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  @Output() cancelRegister = new EventEmitter(); // for Child to parent communication
  model:any={}

  constructor(private accountService: AccountService, private toast: ToastrService){}

  register(){
    this.accountService.register(this.model).subscribe({
      next: () => {
        this.cancel();
      },
      error: error => this.toast.error(error.error)
    })
  }

  cancel(){
    this.cancelRegister.emit(false);
  }
}
