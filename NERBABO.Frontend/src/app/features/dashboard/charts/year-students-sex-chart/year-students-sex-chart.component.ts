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
import { GenderTimeSeries } from '../../../../core/models/dashboard';

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
        <p-chart type="bar" [data]="data" [options]="options" />
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
  @Input() chartData: GenderTimeSeries[] = [];

  data: any;
  options: any;
  title: string = 'Formandos por Sexo';
  year: number = new Date().getFullYear();
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
      const textColorSecondary = documentStyle.getPropertyValue(
        '--p-text-muted-color'
      );
      const surfaceBorder = documentStyle.getPropertyValue(
        '--p-content-border-color'
      );

      // Extract labels from first dataset (all should have same months)
      const labels =
        this.chartData.length > 0
          ? this.chartData[0].data.map((d) => d.month)
          : [];

      // Create datasets for each gender with distinct colors
      const datasets = this.chartData.map((genderSeries) => {
        const colors = this.getGenderColors(genderSeries.gender);
        return {
          label: genderSeries.gender,
          backgroundColor: colors.background,
          borderColor: colors.border,
          data: genderSeries.data.map((d) => d.count),
        };
      });

      this.data = {
        labels: labels,
        datasets: datasets,
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

  updateChartData() {
    if (this.data && this.chartData.length > 0) {
      const labels = this.chartData[0].data.map((d) => d.month);

      const datasets = this.chartData.map((genderSeries) => {
        const colors = this.getGenderColors(genderSeries.gender);
        return {
          label: genderSeries.gender,
          backgroundColor: colors.background,
          borderColor: colors.border,
          data: genderSeries.data.map((d) => d.count),
        };
      });

      this.data = {
        ...this.data,
        labels: labels,
        datasets: datasets,
      };
      this.cd.markForCheck();
    }
  }

  private getGenderColors(gender: string): {
    background: string;
    border: string;
  } {
    const normalizedGender = gender.toLowerCase();

    // Masculino - Blue
    if (normalizedGender.includes('masculino')) {
      return {
        background: 'rgba(59, 130, 246, 0.6)',
        border: '#3b82f6',
      };
    }
    // Feminino - Pink
    if (normalizedGender.includes('feminino')) {
      return {
        background: 'rgba(236, 72, 153, 0.6)',
        border: '#ec4899',
      };
    }

    if (normalizedGender.includes('Não Especificado'.toLocaleLowerCase())) {
      return {
        background: 'rgba(41, 38, 39, 0.6)',
        border: '#342f31ff',
      };
    }
    // Default - Gray
    return {
      background: 'rgba(156, 163, 175, 0.6)',
      border: '#9ca3af',
    };
  }
}
