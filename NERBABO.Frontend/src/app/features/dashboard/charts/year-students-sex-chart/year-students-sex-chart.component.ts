import { isPlatformBrowser } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  inject,
  PLATFORM_ID,
} from '@angular/core';
import { ChartModule } from 'primeng/chart';

@Component({
  selector: 'app-year-students-sex-chart',
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
          type="bar"
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
export class YearStudentsSexChartComponent {
  data: any;
  options: any;
  title: string = 'Formandos por Sexo';
  year: number = new Date().getFullYear();
  platformId = inject(PLATFORM_ID);

  constructor(private cd: ChangeDetectorRef) {}

  ngOnInit() {
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
        labels: [
          'Janeiro',
          'Fevereiro',
          'Março',
          'Abril',
          'Maio',
          'Junho',
          'Julho',
        ],
        datasets: [
          {
            label: 'Homens',
            backgroundColor: documentStyle.getPropertyValue(
              '--p-male-background'
            ),
            borderColor: documentStyle.getPropertyValue('--p-male'),
            data: [65, 59, 80, 81, 56, 55, 40],
          },
          {
            label: 'Mulheres',
            backgroundColor: documentStyle.getPropertyValue(
              '--p-female-background'
            ),
            borderColor: documentStyle.getPropertyValue('--p-female'),
            data: [28, 48, 40, 19, 86, 27, 90],
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
              font: {
                weight: 500,
              },
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
