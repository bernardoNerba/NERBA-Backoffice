import { Component, Input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@Component({
  selector: 'app-icon',
  imports: [FontAwesomeModule],
  template: `
    <fa-icon class="me-{{ marginEnd ? marginEnd : 1 }}" [icon]="icon"></fa-icon>
  `,
})
export class IconComponent {
  @Input({ required: true }) icon!: any;
  @Input() marginEnd?: number;
}
