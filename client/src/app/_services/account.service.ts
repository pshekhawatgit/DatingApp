import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';
import { environment } from 'src/environments/environment';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  // Start - Code to Create an Observable, so that other components in App can know that user is logged in or not.
  private currentUserSource = new BehaviorSubject<User | null>(null); // | null is used to tell it can also be null (when application starts and there is no user info)
  currentUser$ = this.currentUserSource.asObservable(); // $ signifies that is is an observable
  // End - Code to create an Observable
  constructor(private http: HttpClient, private presenceService: PresenceService) { }

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
    // set user roles an an empty array to fill in the role(s) later
    user.roles = [];
    // Decode user token. The name of the property which contains role(s) information in token is "role"
    const roles = this.getDecodedToken(user.token).role;

    // Set user roles. If it is one role then it is just a string, if multiple roles then its a string array
    Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);

    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
    // Create HUB connection
    this.presenceService.createHubConnection(user);
  }

  logout(){
    localStorage.removeItem('user')
    this.currentUserSource.next(null);
    this.presenceService.stopHubConnection();
  }

  getDecodedToken(token: string){
    // Get the part (2nd part) of the JSON Token which has Username and Role(s) Information 
    return JSON.parse(atob(token.split('.')[1]));
  }
}