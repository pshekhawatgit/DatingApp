import { CommonModule, NgFor } from '@angular/common';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  imports: [CommonModule, TimeagoModule, FormsModule]
})
export class MemberMessagesComponent implements OnInit {
  // This component will receive Username as an INPUT from the member-detail component as this component is a child of member-detail
  @Input() username?: string; 
  @Input() messages: Message[] = [];
  @ViewChild('messageForm') messageForm?: NgForm // Added to access Message form so that it can be cleared once the message is sent
  messageContent = '';

  constructor(private messageService: MessageService){
  }

  ngOnInit(): void {
  }

  sendMessage(){
    if(!this.username)
      return;

    this.messageService.sendMessage(this.username, this.messageContent).subscribe({
      next: message => {
        this.messages.push(message);
        this.messageForm?.reset();
      }
    })
  }
}
