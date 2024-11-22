import { CommonModule, NgFor } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, Input, OnInit, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';
//import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  standalone: true,
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  imports: [CommonModule, TimeagoModule, FormsModule]
})
export class MemberMessagesComponent implements OnInit {
  // This component will receive Username as an INPUT from the member-detail component as this component is a child of member-detail
  @Input() username?: string; 
  @ViewChild('messageForm') messageForm?: NgForm // Added to access Message form so that it can be cleared once the message is sent
  messageContent = '';
  messageService = inject(MessageService);

  constructor(){
  }

  ngOnInit(): void {
  }

  sendMessage(){
    if(!this.username)
      return;

    this.messageService.sendMessage(this.username, this.messageContent).then(() => { 
      this.messageForm?.reset();
    })
  }
}
