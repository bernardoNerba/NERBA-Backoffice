import { Component, Input, Output, EventEmitter } from '@angular/core';
import { IconComponent } from '../icon/icon.component';
import { CommonModule } from '@angular/common';
import { MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-title',
  template: `
    <div class="card-header">
      <div class="container-fluid">
        <div class="row align-items-center">
          <div class="col-8">
            <h3 class="text-truncate mb-3">
              {{ title }}
            </h3>
          </div>
          <div class="col-4 text-end">
            @if (!isDropdown) {
            <button type="button" class="btn-main" (click)="onButtonClick()">
              <app-icon [icon]="buttonIcon" [marginEnd]="1" />
              {{ buttonText }}
            </button>
            } @else {
            <p-menu
              #menu
              [model]="menuItems"
              [popup]="true"
              [appendTo]="'body'"
            />
            <p-button
              icon="pi pi-ellipsis-v"
              [rounded]="true"
              (click)="menu.toggle($event)"
            />
            }
          </div>
        </div>
      </div>
    </div>
  `,
  standalone: true,
  imports: [IconComponent, CommonModule, MenuModule, ButtonModule],
})
export class TitleComponent {
  @Input() title: string = '';
  @Input() buttonText: string = '';
  @Input() buttonIcon: any;
  @Output() buttonClick = new EventEmitter<void>();

  @Input() menuItems: MenuItem[] | undefined;
  @Input() isDropdown: boolean = false;

  onButtonClick() {
    this.buttonClick.emit();
  }
}
