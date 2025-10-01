import { Component, OnInit } from '@angular/core';
import { SharedService } from '../../core/services/shared.service';
import { ChartModule } from 'primeng/chart';
import { YearStudentsSexChartComponent } from './charts/year-students-sex-chart/year-students-sex-chart.component';
import { YearStudentsEducationLvlChartComponent } from './charts/year-students-education-lvl-chart/year-students-education-lvl-chart.component';
import { YearStudentsResultChartComponent } from './charts/year-students-result-chart/year-students-result-chart.component';
import { TotalActionLvlChartComponent } from './charts/total-action-lvl-chart/total-action-lvl-chart.component';
import { KpiRowComponent } from '../../shared/components/kpi-row/kpi-row.component';
import { KpiCardComponent } from '../../shared/components/kpi-card/kpi-card.component';

@Component({
  selector: 'app-dashboard',
  imports: [
    ChartModule,
    YearStudentsSexChartComponent,
    YearStudentsEducationLvlChartComponent,
    YearStudentsResultChartComponent,
    TotalActionLvlChartComponent,
    KpiRowComponent,
    KpiCardComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
  title: string = 'Dashboard';

  dashboardKpis = {
    totalStudents: 1547,
    totalApproved: 1238,
    totalVolumeHours: 45680.50,
    totalVolumeDays: 5710
  };

  top5Companies = [
    { name: 'Tech Solutions Lda', count: 156 },
    { name: 'Global Enterprises', count: 142 },
    { name: 'Innovative Systems', count: 128 },
    { name: 'Digital Ventures', count: 115 },
    { name: 'Future Corp', count: 98 }
  ];

  constructor(private sharedService: SharedService) {}

  ngOnInit(): void {
    this.updateBreadcrumbs();
  }

  private updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: 'inactive',
      },
    ]);
  }
}
