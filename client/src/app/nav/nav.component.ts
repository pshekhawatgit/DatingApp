import { Component } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent {
  model: any = {}
  loggedIn = false;

  constructor(private accountService: AccountService){}
  
  ngOnInit(): void{
    this.getCurrentUser();
  }

  // Method to check if User is already logged in, Using Observable I created
  getCurrentUser(){
    this.accountService.currentUser$.subscribe({
      next: user => this.loggedIn = !!user, // !! operator makes it boolean and return true is value present, and false if not 
      error: error => console.log(error)
    })
  }

  login() {
    this.accountService.login(this.model).subscribe({
      next: response => {
        console.log(response);
        this.loggedIn = true;
      },
      error: error => console.log(error)
  })
  }

  logout(){
    this.accountService.logout();
    this.loggedIn = false;
  }
}
