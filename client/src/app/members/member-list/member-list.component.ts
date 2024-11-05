import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent {
  members: Member[] = [];
  pagination: Pagination | undefined;
  pageNumber = 1; // Hard coded
  pageSize = 5; // Hard coded

  constructor(private memberService: MembersService) {}

  ngOnInit(): void{
    this.loadmembers();
  }

  loadmembers(){
    this.memberService.getMembers(this.pageNumber, this.pageSize).subscribe({
      next: response => {
        if(response.result && response.pagination)
        {
          this.members = response.result;
          this.pagination = response.pagination;
        }
      }
    })
  }

  pageChanged(event: any)
  {
    if(this.pageNumber !== event.page){
      this.pageNumber = event.page;
      this.loadmembers();
    }
  }
}
