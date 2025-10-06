import { isPlatformBrowser } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  inject,
  Input,
  PLATFORM_ID,
  SimpleChanges,
} from '@angular/core';
import { ChartModule } from 'primeng/chart';

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
  @Input() chartData: { label: string; value: number }[] = [];

  data: any;
  options: any;
  title: string = 'Formandos pelos diferentes níveis de habilitações';
  year: number = new Date().getFullYear();
  platformId = inject(PLATFORM_ID);

  constructor(private cd: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.initChart();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['chartData'] && !changes['chartData'].firstChange) {
      this.updateChartData();
    }
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
        labels: this.chartData.map((d) => d.label),
        datasets: [
          {
            label: 'Quantidade Formandos',
            data: this.chartData.map((d) => d.value),
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

  updateChartData() {
    if (this.data && this.chartData.length > 0) {
      this.data = {
        ...this.data,
        labels: this.chartData.map((d) => d.label),
        datasets: [
          {
            ...this.data.datasets[0],
            data: this.chartData.map((d) => d.value),
          },
        ],
      };
      this.cd.markForCheck();
    }
  }
}
