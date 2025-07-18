import { Component, Input } from '@angular/core';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-dropdown-menu',
  imports: [Menu, Button],
  template: `
    <p-menu #menu [model]="menuItems" [popup]="true" [appendTo]="'body'" />
    <p-button
      icon="pi pi-ellipsis-v"
      [rounded]="true"
      severity="contrast"
      (click)="menu.toggle($event)"
    />
  `,
})
export class DropdownMenuComponent {
  @Input({ required: true }) menuItems: MenuItem[] | undefined;
}
