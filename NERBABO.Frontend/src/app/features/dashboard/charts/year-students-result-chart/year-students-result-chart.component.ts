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
  @Input() chartData: { label: string; value: number }[] = [];

  title: string = 'Resultados dos Formandos';
  year: number = new Date().getFullYear();
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
      const textColor = documentStyle.getPropertyValue('--text-color');

      const colors = this.getColorsForLabels(this.chartData.map((d) => d.label));

      this.data = {
        labels: this.chartData.map((d) => d.label),
        datasets: [
          {
            data: this.chartData.map((d) => d.value),
            backgroundColor: colors.background,
            hoverBackgroundColor: colors.hover,
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

  updateChartData() {
    if (this.data && this.chartData.length > 0) {
      const colors = this.getColorsForLabels(this.chartData.map((d) => d.label));

      this.data = {
        ...this.data,
        labels: this.chartData.map((d) => d.label),
        datasets: [
          {
            ...this.data.datasets[0],
            data: this.chartData.map((d) => d.value),
            backgroundColor: colors.background,
            hoverBackgroundColor: colors.hover,
          },
        ],
      };
      this.cd.markForCheck();
    }
  }

  private getColorsForLabels(labels: string[]): {
    background: string[];
    hover: string[];
  } {
    return {
      background: labels.map((label) => this.getColorForLabel(label, false)),
      hover: labels.map((label) => this.getColorForLabel(label, true)),
    };
  }

  private getColorForLabel(label: string, isHover: boolean): string {
    const normalizedLabel = label.toLowerCase();

    // "Aprovado" - Green
    if (normalizedLabel.includes('aprovado')) {
      return isHover ? '#22c55e' : 'rgba(34, 197, 94, 0.6)';
    }
    // "Reprovado" - Red
    if (normalizedLabel.includes('reprovado')) {
      return isHover ? '#ef4444' : 'rgba(239, 68, 68, 0.6)';
    }
    // "Desistiu" - Gray
    if (normalizedLabel.includes('desistiu')) {
      return isHover ? '#9ca3af' : 'rgba(156, 163, 175, 0.6)';
    }
    // "Não especificado" or default - Black
    return isHover ? '#374151' : 'rgba(55, 65, 81, 0.6)';
  }
}
