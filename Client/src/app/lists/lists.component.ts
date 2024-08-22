import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { MemberCardComponent } from '../members/member-card/member-card.component';
import { Member } from '../_models/member';
import { Pagination } from '../_models/pagination';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-lists',
  standalone: true,
  imports: [CommonModule, FormsModule, MemberCardComponent,PaginationModule],
  templateUrl: './lists.component.html',
  styleUrl: './lists.component.css'
})
export class ListsComponent {

  match: Member[]=[];
  members : Partial<Member[]>;
  predicate ='matches';
  pageNumber= 1;
  pageSize = 5;
  pagination: Pagination;
  constructor(private memberService: MembersService){
    this.getMatches();
  }

  loadLikes(){
    this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize).subscribe(response=>{
      this.members = response.result;
      this.pagination= response.pagination;

    })
  }
  getMatches(){
    this.memberService.getMatches().subscribe(response=>{
     this.members=response;
    })
    
  }

  pageChange(event:any){
    this.pageNumber= event.page;
    this.loadLikes();
  }
}
