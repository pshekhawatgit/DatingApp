import { Component } from '@angular/core';
import { Member } from '../_models/member';
import { MembersService } from '../_services/members.service';
import { Pagination } from '../_models/pagination';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent {
  members: Member[] | undefined;
  predicate = 'liked';
  pageNumber = 1;
  pageSize = 5;
  pagination: Pagination | undefined;

  constructor(private memberService: MembersService){

  }

  ngOnInit() : void {
    console.log(this.predicate + ' start Init');
    this.loadLikes(); 
    console.log(this.predicate + ' end Init');
  }

  loadLikes(){
    this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize).subscribe({
      next: response => {
        this.members = response.result;
        this.pagination = response.pagination;
        console.log(this.predicate + ' from Load Likes');
      }
    })
  }

  pageChanged(event: any)
  {
    if(this.pageNumber !== event.page){
      this.pageNumber = event.page;
      this.loadLikes();
    }
  }
}
