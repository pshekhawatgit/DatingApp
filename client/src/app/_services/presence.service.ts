import { Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { take } from 'rxjs';
import { Route, Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  onlineUsers = signal<string[]>([]);
  
  constructor(private toastr: ToastrService, private router: Router) { }

  createHubConnection(user: User){
    this.hubConnection = new HubConnectionBuilder()
    .withUrl(this.hubUrl + 'presence', {
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on('UserIsOnline', username => {
      //this.toastr.info(username + ' has connected')
      this.onlineUsers.set([...this.onlineUsers(), username]);
    });

    this.hubConnection.on('UserIsOffline', username => {
      //this.toastr.warning(username + ' has disconnected')
      this.onlineUsers.set(this.onlineUsers().filter(u => u !== username))
    });

    // Check for or listen to the event we have created in API to get Online users
    this.hubConnection.on('GetOnlineUsers', usernames => {
      this.onlineUsers.set(usernames);
    });

    // Listen to any new message received, if user is not active in a chat with the sending user
    this.hubConnection.on('NewMessageReceived', ({ username, knownAs}) => {
      this.toastr.info(knownAs + ' has sent you a new message! Click me to see it')
      .onTap
      .pipe(take(1)).subscribe({
        next: () => {
          this.router.navigateByUrl('/members/' + username + '?tab=Messages')
        }
      })
    })
  }

  stopHubConnection(){
    this.hubConnection?.stop().catch(error => console.log(error));
  }
}
