import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import {  DefaultUrlSerializer, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Member } from '../../_models/member';
import { MembersService } from '../../_services/members.service';
import { PresenceService } from '../../_services/presence.service';

@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [RouterLink,CommonModule],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css'
})
export class MemberCardComponent {

  @Input() member : Member;
  members: Member[]=[];
  match:{[key:number]:boolean}={};

  constructor(private memberService: MembersService,
     private toastr : ToastrService,public presence: PresenceService){
      this.getMatches();
  }

  addLike(member: Member){
    this.memberService.addLike(member.username).subscribe(()=>{
      this.toastr.success('You have liked ' + member.knownAs);
      
    })
  }
  getMatches(){
    this.memberService.getMatches().subscribe(response=>{
    this.members= response;
    this.members.forEach(element => {
      this.CheckifMatch(element.id)
    });
    })   
  }

  CheckifMatch(id:number){
    const matchid =this.members.some(member=>member.id===id);
    if(matchid){

      this.match[id]=matchid;
    }else{
      this.match[id]=false;
    }
    
  }
}
