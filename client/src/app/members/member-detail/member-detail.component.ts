import { CommonModule } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Gallery, GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { TimeagoModule } from 'ngx-timeago';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';
import { AccountService } from 'src/app/_services/account.service';
import { User } from 'src/app/_models/user';
import { take } from 'rxjs';

@Component({
  selector: 'app-member-detail',
  standalone: true, // Made standalone to utilize a Standalone component (ngx-gallery) for Photos
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
  imports: [CommonModule, TabsModule, GalleryModule, TimeagoModule, MemberMessagesComponent] // Had to import explicitly as it is now a standalone module
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent; // To get hold of MemberTabs inside this component
  // assigned Empty member here to avoid errors in HTML as we removed  *ngIf="member" from the top div there. 
  //That had to be removed because we made the Child (memberTabs) static above to be accessible onInit 
  member: Member = {} as Member; 
  images: GalleryItem[] = [];
  activeTab?: TabDirective;
  //messages: Message[] = [];
  presenceService = inject(PresenceService);
  user: User | undefined;

  constructor(private accountSerive: AccountService, private route: ActivatedRoute, private messageService: MessageService) {
    this.accountSerive.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if(user)
          this.user = user;
      }
    })
  }
  // Stop Hub connection if user navigates awar from this component (user detail)
  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
  ngOnInit(): void {
    // Get Member from Route Resolver (to read the querystring and open the respective Tab in query string)
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'] // the data[] attribute value 'member' must macth the resolve property name defined in app-routing.module.ts file.
      }
    })

    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    })

    this.getimages()
  }

  onTabActivated(data: TabDirective){
    this.activeTab = data;
    if(this.activeTab.heading === data.heading && this.user){
      this.messageService.createHubConnection(this.user, this.member.userName);
    }
    else{
      this.messageService.stopHubConnection();
    }
  }

  selectTab(heading: string){
    if(this.memberTabs){
      this.memberTabs.tabs.find(t => t.heading === heading)!.active = true;
    }
    else{
      this.messageService.stopHubConnection();
    }
  }

  loadMessages(){
    if(this.member)
    {
      this.accountSerive.currentUser$.subscribe({
        next: user => {
          if(user) {
            this.user = user;
          }
          else {
            return;
          }
        }
      })

      if(this.user)
        this.messageService.createHubConnection(this.user, this.member.userName)
    }
      // this.messageService.getMessageThread(this.member.userName).subscribe({
      //   next: messages => this.messages = messages
      // })
  }

  getimages() {
    if(!this.member)
      return;

    for (const photo of this.member.photos){
      this.images.push(new ImageItem({src: photo.url, thumb: photo.url}));
    }
  }
}
