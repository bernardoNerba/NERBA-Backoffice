import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-kpi-card',
  imports: [CommonModule, FontAwesomeModule, CardModule],
  templateUrl: './kpi-card.component.html',
  styleUrl: './kpi-card.component.css',
})
export class KpiCardComponent {
  @Input({ required: true }) title!: string;
  @Input({ required: true }) value!: string | number;
  @Input({ required: true }) icon!: any;
}
