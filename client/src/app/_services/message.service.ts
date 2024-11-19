import { Injectable, signal } from '@angular/core';
import { Message } from '../_models/message';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  messageThread = signal<Message[]>([]);

  constructor(private http: HttpClient) { }

  createHubConnection(user: User, otherUsername: string){
    this.hubConnection = new HubConnectionBuilder()
    .withUrl(this.hubUrl + 'message?user=' + otherUsername, { // "message" is the name we defined in API's program.cs
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    // Start Hub Connection
    this.hubConnection.start().catch(error => console.log(error));

    // ReceiveMessageThread is the name defined in and sent from API's MessageHub
    this.hubConnection.on('RecieveMessageThread', messages => {
      this.messageThread.set(messages)
    });
    // NewMessage is the name defined in and sent from API's MessageHub
    this.hubConnection.on("NewMessage", message => {
      this.messageThread.update(messages => [...messages, message]) // This updates the existing messages[] with new message
    });
  }

  stopHubConnection(){
    if(this.hubConnection?.state === HubConnectionState.Connected)
    {
      this.hubConnection?.stop().catch(error => console.log(error));
    }
  }

  // Method to get messages from the API
  getMessages(pageNumber: number, pageSize: number, container: string){
    // Get Pagination headers to send along the request to API 
    let params = getPaginationHeaders(pageNumber, pageSize);
    // Add Container (for Messages) as additional parameter to be sent
    params = params.append('Container', container);
    // Call API (using GetPaginatedResult - Helper method) and return the response (of type Message[])
    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params, this.http);
  }

  getMessageThread(userName: string){
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/'+ userName);
  }

  async sendMessage(username: string, content: string){
    // Rather than API controller, Invoke the SendMessage method from MessageHub on API side
    // recipientUsername and content are the two properties of CreateMemberDTO, which "SendMessage" expects as input param
    return this.hubConnection?.invoke('SendMessage', {recipientUsername: username, content})
      .catch(error => console.log(error));
  }

  deleteMessage(messageid: number){
    return this.http.delete(this.baseUrl + 'messages/' + messageid)
  }
}
