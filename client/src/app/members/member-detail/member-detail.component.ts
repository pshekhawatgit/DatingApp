import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Gallery, GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { TimeagoModule } from 'ngx-timeago';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-detail',
  standalone: true, // Made standalone to utilize a Standalone component (ngx-gallery) for Photos
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
  imports: [CommonModule, TabsModule, GalleryModule, TimeagoModule, MemberMessagesComponent] // Had to import explicitly as it is now a standalone module
})
export class MemberDetailComponent implements OnInit{
  @ViewChild('memberTabs') memberTabs?: TabsetComponent; // To get hold of MemberTabs inside this component
  member: Member | undefined;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;
  messages: Message[] = [];

  constructor(private memberService: MembersService, private route: ActivatedRoute, private messageService: MessageService) {
  }
  ngOnInit(): void {
    this.loadmember();
  }

  onTabActivated(data: TabDirective){
    this.activeTab = data;
    if(this.activeTab.heading === data.heading){
      this.loadMessages();
    }

  }

  loadMessages(){
    if(this.member)
      this.messageService.getMessageThread(this.member.userName).subscribe({
        next: messages => this.messages = messages
      })
  }

  loadmember()
  {
    const username = this.route.snapshot.paramMap.get('username');

    if(!username)
      return;

    this.memberService.getMember(username).subscribe({
      next: member => {
        this.member = member,
        this.getimages()
      }
    })

  }

  getimages() {
    if(!this.member)
      return;

    for (const photo of this.member.photos){
      this.images.push(new ImageItem({src: photo.url, thumb: photo.url}));
    }
  }
}
