import { Component, Input } from '@angular/core';
import { ICONS } from '../../../../core/objects/icons';
import { IconComponent } from '../../icon/icon.component';

@Component({
  selector: 'app-active-badge',
  imports: [IconComponent],
  template: ` <span
    class="badge bg-light {{ isActive ? 'text-success' : 'text-danger' }}"
  >
    <app-icon [icon]="isActive ? ICONS.success : ICONS.fail" [marginEnd]="1" />
    {{ isActive ? 'Ativo' : 'Inativo' }}
  </span>`,
})
export class ActiveBadgeComponent {
  @Input({ required: true }) isActive!: boolean;
  ICONS = ICONS;
}
