import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { KpiCardComponent } from '../kpi-card/kpi-card.component';

export interface KpiRowData {
  totalStudents: number;
  totalApproved: number;
  totalVolumeHours: number;
  totalVolumeDays: number;
}

@Component({
  selector: 'app-kpi-row',
  imports: [CommonModule, KpiCardComponent],
  templateUrl: './kpi-row.component.html',
  styleUrl: './kpi-row.component.css'
})
export class KpiRowComponent {
  @Input() kpis!: KpiRowData;
  @Input() layout: 'scroll' | 'responsive' = 'responsive';
}