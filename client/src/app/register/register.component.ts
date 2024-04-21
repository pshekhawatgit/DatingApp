import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  @Input() usersFromHomeComponent: any; // for parent to chile communication
  @Output() cancelRegister = new EventEmitter(); // for Child to parent communication
  model:any={}

  register(){
    console.log(this.model);
  }

  cancel(){
    this.cancelRegister.emit(false);
  }
}
