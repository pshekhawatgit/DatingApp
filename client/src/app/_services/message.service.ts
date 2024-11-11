import { Injectable } from '@angular/core';
import { Message } from '../_models/message';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

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
}
