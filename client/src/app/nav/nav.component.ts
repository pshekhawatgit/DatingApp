import { Component } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent {
  model: any = {}

  constructor(public accountService: AccountService, private router: Router, private toastr: ToastrService){} // Made public for accessing in html
  
  ngOnInit(): void{
  }

  login() {
    this.accountService.login(this.model).subscribe({
      next: _ => {
        this.router.navigateByUrl('/members');
        this.model = {}; // this is to reset the form so that the browser doesn't remember the username/password in respective fields on logout
      }, // Instead of _ you can also use ()
      error: error => this.toastr.error(error.error)
  })
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
