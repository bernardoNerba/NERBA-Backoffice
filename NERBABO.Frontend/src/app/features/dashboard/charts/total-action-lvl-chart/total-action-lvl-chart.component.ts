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
  selector: 'app-total-action-lvl-chart',
  imports: [ChartModule],
  template: `
    <div class="d-flex flex-column h-100">
      <p class="chart-title mb-2">
        Quantidade de <strong>{{ title }}</strong>
        <br />
        <small class="text-muted">(sempre)</small>
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
export class TotalActionLvlChartComponent {
  @Input() chartData: { label: string; value: number }[] = [];

  title: string = 'Ações por nível de habilitações';
  data: any;
  options: any;
  platformId = inject(PLATFORM_ID);
  constructor(private cd: ChangeDetectorRef) {}

  ngOnInit() {
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

      this.data = {
        labels: this.chartData.map((d) => d.label),
        datasets: [
          {
            data: this.chartData.map((d) => d.value),
            backgroundColor: [
              documentStyle.getPropertyValue('--p-lvl1-background'),
              documentStyle.getPropertyValue('--p-lvl2-background'),
              documentStyle.getPropertyValue('--p-lvl3-background'),
            ],
            hoverBackgroundColor: [
              documentStyle.getPropertyValue('--p-lvl1'),
              documentStyle.getPropertyValue('--p-lvl2'),
              documentStyle.getPropertyValue('--p-lvl3'),
            ],
          },
        ],
      };

      this.options = {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '60%',
        plugins: {
          legend: {
            labels: {
              color: textColor,
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
