import { Component, Input, ViewEncapsulation } from '@angular/core';
import { Member } from 'src/app/_models/member';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
  encapsulation: ViewEncapsulation.Emulated // To encapsulate/contain the styles from css, this is also the default value
})
export class MemberCardComponent {
  @Input() member: Member | undefined; // To use data from Parent (member-list) component, set to undefined to initialize
}
