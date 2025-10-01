import { isPlatformBrowser } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  inject,
  PLATFORM_ID,
} from '@angular/core';
import { ChartModule } from 'primeng/chart';

@Component({
  selector: 'app-year-students-result-chart',
  imports: [ChartModule],
  template: `
    <div class="d-flex flex-column h-100">
      <p class="chart-title mb-2">
        Diferentes <strong>{{ title }}</strong>
        <br />
        <small class="text-muted">({{ year }})</small>
      </p>
      <div
        class="flex-grow-1 d-flex align-items-center justify-content-center"
        style="position: relative; min-height: 0;"
      >
        <div
          style="width: 100%; height: 100%; display: flex; align-items: center; justify-content: center;"
        >
          <p-chart
            type="doughnut"
            [data]="data"
            [options]="options"
            [style]="{ width: '100%', height: '100%' }"
          />
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: flex;
        flex-direction: column;
        height: 100%;
      }
    `,
  ],
})
export class YearStudentsResultChartComponent {
  title: string = 'Resultados dos Formandos';
  year: number = new Date().getFullYear();
  data: any;
  options: any;

  platformId = inject(PLATFORM_ID);

  constructor(private cd: ChangeDetectorRef) {}

  ngOnInit() {
    this.initChart();
  }

  initChart() {
    if (isPlatformBrowser(this.platformId)) {
      const documentStyle = getComputedStyle(document.documentElement);
      const textColor = documentStyle.getPropertyValue('--text-color');

      this.data = {
        labels: ['Aprovado', 'Reprovado', 'Desistiu'],
        datasets: [
          {
            data: [540, 325, 702],
            backgroundColor: [
              documentStyle.getPropertyValue('--p-approved-background'),
              documentStyle.getPropertyValue('--p-not-approved-background'),
              documentStyle.getPropertyValue('--p-other-background'),
            ],
            hoverBackgroundColor: [
              documentStyle.getPropertyValue('--p-approved'),
              documentStyle.getPropertyValue('--p-not-approved'),
              documentStyle.getPropertyValue('--p-other'),
            ],
          },
        ],
      };

      this.options = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            labels: {
              usePointStyle: true,
              color: textColor,
            },
          },
        },
      };
      this.cd.markForCheck();
    }
  }
}
