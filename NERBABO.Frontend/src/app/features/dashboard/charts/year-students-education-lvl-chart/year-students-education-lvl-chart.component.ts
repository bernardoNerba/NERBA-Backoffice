import { isPlatformBrowser } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  inject,
  PLATFORM_ID,
} from '@angular/core';
import { ChartModule } from 'primeng/chart';
import { HABILITATIONS } from '../../../../core/objects/habilitations';

@Component({
  selector: 'app-year-students-education-lvl-chart',
  imports: [ChartModule],
  template: `
    <div class="d-flex flex-column h-100">
      <p class="chart-title mb-2">
        Quantidade de <strong>{{ title }}</strong>
        <br />
        <small class="text-muted">({{ year }})</small>
      </p>
      <div class="chart-container">
        <p-chart
          type="line"
          [data]="data"
          [options]="options"
        />
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
      .chart-container {
        flex: 1;
        position: relative;
        min-height: 0;
      }
    `,
  ],
})
export class YearStudentsEducationLvlChartComponent {
  data: any;
  options: any;
  title: string = 'Formandos pelos diferentes níveis de habilitações';
  year: number = new Date().getFullYear();
  platformId = inject(PLATFORM_ID);

  constructor(private cd: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.initChart();
  }

  initChart() {
    if (isPlatformBrowser(this.platformId)) {
      const documentStyle = getComputedStyle(document.documentElement);
      const textColor = documentStyle.getPropertyValue('--p-text-color');
      const textColorSecondary = documentStyle.getPropertyValue(
        '--p-text-muted-color'
      );
      const surfaceBorder = documentStyle.getPropertyValue(
        '--p-content-border-color'
      );

      this.data = {
        labels: [...HABILITATIONS.map((h) => h.value)],
        datasets: [
          {
            label: 'Quantidade Formandos',
            data: [
              10, 15, 30, 35, 40, 38, 35, 33, 39, 40, 99, 40, 50, 200, 150, 50,
              80, 30, 20,
            ],
            fill: false,
            borderColor: '#000',
            tension: 0.4,
          },
        ],
      };

      this.options = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            labels: {
              color: textColor,
            },
          },
        },
        scales: {
          x: {
            ticks: {
              color: textColorSecondary,
            },
            grid: {
              color: surfaceBorder,
              drawBorder: false,
            },
          },
          y: {
            ticks: {
              color: textColorSecondary,
            },
            grid: {
              color: surfaceBorder,
              drawBorder: false,
            },
          },
        },
      };
      this.cd.markForCheck();
    }
  }
}
