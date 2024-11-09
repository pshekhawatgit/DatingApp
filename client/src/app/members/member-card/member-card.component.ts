import { Component, Input, ViewEncapsulation } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
  encapsulation: ViewEncapsulation.Emulated // To encapsulate/contain the styles from css, this is also the default value
})
export class MemberCardComponent {
  @Input() member: Member | undefined; // To use data from Parent (member-list) component, set to undefined to initialize

  constructor(private memberService: MembersService, private toastr: ToastrService){

  }

  addLike(member: Member){
    this.memberService.addLike(member.userName).subscribe({
      next: () => this.toastr.success('You have liked ' + member.knownAs)
    })
  }
}
