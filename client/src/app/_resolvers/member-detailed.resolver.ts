import { ResolveFn } from '@angular/router';
import { Member } from '../_models/member';
import { inject } from '@angular/core';
import { MembersService } from '../_services/members.service';

export const memberDetailedResolver: ResolveFn<Member> = (route, state) => {
  // Inject member service to return the Member
  const memberService = inject(MembersService);
  // get the member from MembersService and pass in the Username (from QueryString)
  return memberService.getMember(route.paramMap.get('username')!);
};
