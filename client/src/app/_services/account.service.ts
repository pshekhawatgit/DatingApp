import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  // Start - Code to Create an Observable, so that other components in App can know that user is logged in or not.
  private currentUserSource = new BehaviorSubject<User | null>(null); // | null is used to tell it can also be null (when application starts and there is no user info)
  currentUser$ = this.currentUserSource.asObservable(); // $ signifies that is is an observable
  // End - Code to create an Observable
  constructor(private http: HttpClient) { }

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((response : User) => {
        const user = response;
        if(user){
          this.setCurrentUser(user);
        }
      })
    );
  }

  register(model: any){
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map( user => {
        if(user){
          this.setCurrentUser(user);
        }
      })
    )
  }

  // To be used from Component to set the User info in account service
  setCurrentUser(user: User)
  {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  logout(){
    localStorage.removeItem('user')
    this.currentUserSource.next(null);
  }
}