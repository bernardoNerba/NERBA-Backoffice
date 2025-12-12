import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Person } from '../../../core/models/person';
import { TabMenuModule } from 'primeng/tabmenu';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-nav-header',
  standalone: true,
  imports: [TabMenuModule],
  template: `<p-tabMenu [model]="items"></p-tabMenu>`,
})
export class NavHeaderComponent implements OnChanges {
  @Input({ required: true }) person!: Person;

  items: MenuItem[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['person'] && changes['person'].currentValue) {
      this.buildMenuItems();
    }
  }

  private buildMenuItems(): void {
    const menu: MenuItem[] = [
      {
        label: 'Pessoa',
        routerLink: ['/people', this.person.id],
        routerLinkActiveOptions: { exact: true },
      },
    ];

    if (this.person.isStudent) {
      menu.push({
        label: 'Formando',
        routerLink: ['/people', this.person.id, 'student'],
      });
    }

    if (this.person.isTeacher) {
      menu.push({
        label: 'Formador',
        routerLink: ['/people', this.person.id, 'teacher'],
      });
    }

    if (this.person.isColaborator) {
      menu.push({
        label: 'Colaborador',
        routerLink: ['/people', this.person.id, 'acc'],
      });
    }

    this.items = menu;
  }
}
