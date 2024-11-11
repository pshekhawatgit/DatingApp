import { Component } from '@angular/core';
import { Message } from '../_models/message';
import { MessageService } from '../_services/message.service';
import { Pagination } from '../_models/pagination';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent {
  messages?: Message[];
  pagination?: Pagination;
  pageNumber = 1;
  pageSize = 5;
  container = 'Outbox';
  
  constructor(private messageService: MessageService){}

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages(){
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe({
      next : response => {
        // Set local Messages with the result from response
        this.messages = response.result;
        this.pagination = response.pagination;
      } 
    })
  }

  pageChanged(event: any)
  {
    if(this.pageNumber !== event.page){
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }
}
