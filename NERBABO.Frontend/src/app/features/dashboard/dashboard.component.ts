import { Component, OnInit } from '@angular/core';
import { SharedService } from '../../core/services/shared.service';
import { ChartModule } from 'primeng/chart';
import { YearStudentsSexChartComponent } from './charts/year-students-sex-chart/year-students-sex-chart.component';
import { YearStudentsEducationLvlChartComponent } from './charts/year-students-education-lvl-chart/year-students-education-lvl-chart.component';
import { YearStudentsResultChartComponent } from './charts/year-students-result-chart/year-students-result-chart.component';
import { TotalActionLvlChartComponent } from './charts/total-action-lvl-chart/total-action-lvl-chart.component';
import { KpiCardComponent } from '../../shared/components/kpi-card/kpi-card.component';
import { TabsModule } from 'primeng/tabs';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-dashboard',
  imports: [
    ChartModule,
    YearStudentsSexChartComponent,
    YearStudentsEducationLvlChartComponent,
    YearStudentsResultChartComponent,
    TotalActionLvlChartComponent,
    KpiCardComponent,
    TabsModule,
    TagModule,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
  title: string = 'Dashboard';

  dashboardKpis = {
    totalStudents: 1547,
    totalApproved: 1238,
    totalVolumeHours: 45680.5,
    totalVolumeDays: 5710,
  };

  top5Companies = [
    { name: 'Tech Solutions Lda', count: 156 },
    { name: 'Global Enterprises', count: 142 },
    { name: 'Innovative Systems', count: 128 },
    { name: 'Digital Ventures', count: 115 },
    { name: 'Future Corp', count: 98 },
  ];

  tabs = [
    {
      value: '0',
      title: 'Localidades',
      content: [
        { name: 'Tech Solutions Lda', count: 156 },
        { name: 'Global Enterprises', count: 142 },
        { name: 'Innovative Systems', count: 128 },
      ],
    },
    {
      value: '1',
      title: 'Areas',
      content: [
        { name: 'Formação em Gestão', count: 89 },
        { name: 'Certificação Técnica', count: 76 },
        { name: 'Workshop de Liderança', count: 68 },
      ],
    },
    {
      value: '2',
      title: 'Regimes',
      content: [
        { name: 'João Silva', count: 12 },
        { name: 'Maria Santos', count: 11 },
        { name: 'Pedro Costa', count: 10 },
      ],
    },
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
