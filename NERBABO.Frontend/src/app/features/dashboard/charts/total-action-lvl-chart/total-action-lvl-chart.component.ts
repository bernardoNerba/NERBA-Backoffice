import { isPlatformBrowser } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  inject,
  PLATFORM_ID,
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
  title: string = 'Ações por nível de habilitações';
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
      const textColor = documentStyle.getPropertyValue('--p-text-color');

      this.data = {
        labels: ['Nível 1', 'Nível 2', 'Nível 3'],
        datasets: [
          {
            data: [300, 50, 100],
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
}
