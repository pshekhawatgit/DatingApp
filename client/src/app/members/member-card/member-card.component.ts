import { Component, computed, inject, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
  encapsulation: ViewEncapsulation.Emulated // To encapsulate/contain the styles from css, this is also the default value
})
export class MemberCardComponent implements OnInit {
  @Input() member: Member | undefined; // To use data from Parent (member-list) component, set to undefined to initialize
  // Inject presence service
  private presenceService = inject(PresenceService);
  isOnline = computed(() => {
    const userName = this.member?.userName ?? ''; // provide an empty string as the fallback value if this.member?.userName is undefined
    console.log('user is: ' + userName)
    return this.presenceService.onlineUsers().includes(userName);
  });

  constructor(private memberService: MembersService, private toastr: ToastrService){
  }
  
  ngOnInit(): void {
  }

  addLike(member: Member){
    this.memberService.addLike(member.userName).subscribe({
      next: () => this.toastr.success('You have liked ' + member.knownAs)
    })
  }
}
