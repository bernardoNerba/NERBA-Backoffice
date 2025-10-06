import { Component, OnInit } from '@angular/core';
import { SharedService } from '../../core/services/shared.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { ChartModule } from 'primeng/chart';
import { YearStudentsSexChartComponent } from './charts/year-students-sex-chart/year-students-sex-chart.component';
import { YearStudentsEducationLvlChartComponent } from './charts/year-students-education-lvl-chart/year-students-education-lvl-chart.component';
import { YearStudentsResultChartComponent } from './charts/year-students-result-chart/year-students-result-chart.component';
import { TotalActionLvlChartComponent } from './charts/total-action-lvl-chart/total-action-lvl-chart.component';
import { KpiCardComponent } from '../../shared/components/kpi-card/kpi-card.component';
import { TabsModule } from 'primeng/tabs';
import { TagModule } from 'primeng/tag';
import { DashboardData, TabContent } from '../../core/models/dashboard';

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
  loading: boolean = true;

  // KPI values
  studentPaymentsValue: number = 0;
  teacherPaymentsValue: number = 0;
  totalCompaniesValue: number = 0;

  // Tab data
  tabs: TabContent[] = [];

  // Chart data
  studentsByHabilitationData: { label: string; value: number }[] = [];
  studentResultsData: { label: string; value: number }[] = [];
  actionHabilitationsData: { label: string; value: number }[] = [];
  studentGendersData: any[] = [];

  constructor(
    private sharedService: SharedService,
    private dashboardService: DashboardService
  ) {}

  ngOnInit(): void {
    this.updateBreadcrumbs();
    this.loadDashboardData();
  }

  private loadDashboardData(): void {
    this.loading = true;

    // Fetch dashboard data with predefined intervals for each KPI
    this.dashboardService.getDashboardData().subscribe({
      next: (data: DashboardData) => {
        this.populateDashboard(data);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
        this.loading = false;
      },
    });
  }

  private populateDashboard(data: DashboardData): void {
    // Populate KPI values
    this.studentPaymentsValue = data.kpis.studentPayments.value;
    this.teacherPaymentsValue = data.kpis.teacherPayments.value;
    this.totalCompaniesValue = data.kpis.totalCompanies.value;

    // Populate chart data
    this.studentsByHabilitationData = data.charts.studentsByHabilitationLvl.value;
    this.studentResultsData = data.charts.studentResults.value;
    this.actionHabilitationsData = data.charts.actionHabilitationsLvl.value;
    this.studentGendersData = data.charts.studentGenders.value;

    // Populate tabs
    this.tabs = [
      {
        value: '0',
        title: 'Localidades',
        content: data.top5.actionsByLocality.value.map((item) => ({
          name: item.label,
          count: item.value,
        })),
      },
      {
        value: '1',
        title: 'Regimes',
        content: data.top5.actionsByRegiment.value.map((item) => ({
          name: item.label,
          count: item.value,
        })),
      },
      {
        value: '2',
        title: 'Estados',
        content: data.top5.actionsByStatus.value.map((item) => ({
          name: item.label,
          count: item.value,
        })),
      },
    ];
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
