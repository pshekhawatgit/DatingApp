import { Component } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent {
  model: any = {}

  constructor(public accountService: AccountService, private router: Router){} // Made public for accessing in html
  
  ngOnInit(): void{
  }

  login() {
    this.accountService.login(this.model).subscribe({
      next: _ => this.router.navigateByUrl('/members'), // Instead of _ you can also use ()
      error: error => console.log(error)
  })
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
