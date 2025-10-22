import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { TooltipModule } from 'primeng/tooltip';
import { PdfService } from '../../../core/services/pdf.service';
import { MessageService } from 'primeng/api';
import { finalize } from 'rxjs';

interface PdfOption {
  label: string;
  value: string;
  icon: string;
  tooltip: string;
}

@Component({
  selector: 'app-pdf-actions',
  standalone: true,
  imports: [CommonModule, ButtonModule, DropdownModule, TooltipModule],
  template: `
    <div
      class="d-flex flex-wrap gap-2 align-items-center justify-content-between"
    >
      <div class="me-4">
        <i class="pi pi-file-pdf me-2"> </i>
        {{ title || '' }}
      </div>

      <!-- Generate Report And Download it-->
      <div class="mx-2">
        @if (actionId){
        <p-button
          icon="pi pi-download"
          outlined="true"
          size="small"
          [loading]="isGeneratingReport"
          [disabled]="isGenerating"
          (onClick)="generateReport()"
          [text]="true"
          severity="contrast"
          pTooltip="Gerar PDF"
          class="me-2"
          tooltipPosition="top"
        />
        }
        <p-button
          icon="pi pi-eye"
          outlined="true"
          size="small"
          [loading]="isGeneratingReport"
          [disabled]="isGenerating"
          (onClick)="viewPdf()"
          severity="warn"
          [text]="true"
          pTooltip="Ver PDF"
          tooltipPosition="top"
          class="me-2"
        />
        <p-button
          icon="pi pi-print"
          outlined="true"
          size="small"
          [loading]="isGeneratingReport"
          [disabled]="isGenerating"
          (onClick)="printPdf()"
          severity="danger"
          [text]="true"
          pTooltip="Imprimir PDF"
          tooltipPosition="top"
          class="me-2"
        />
      </div>
    </div>
  `,
})
export class PdfActionsComponent {
  @Input() actionId?: number;
  @Input() sessionId?: number;
  @Input() title?: string;
  @Input() reportType: 'sessions' | 'cover' = 'sessions';

  isGeneratingReport = false;
  isGeneratingDetail = false;
  isGeneratingSummary = false;

  constructor(
    private pdfService: PdfService,
    private messageService: MessageService
  ) {}

  get isGenerating(): boolean {
    return (
      this.isGeneratingReport ||
      this.isGeneratingDetail ||
      this.isGeneratingSummary
    );
  }

  private getReportObservable() {
    if (!this.actionId) return null;

    return this.reportType === 'cover'
      ? this.pdfService.generateCoverReport(this.actionId)
      : this.pdfService.generateSessionsReport(this.actionId);
  }

  private getFilenamePrefix(): string {
    return this.reportType === 'cover' ? 'capa-acao' : 'relatorio-sessoes-acao';
  }

  private getSuccessMessage(action: string): string {
    const reportName =
      this.reportType === 'cover' ? 'Capa' : 'Relatório de sessões';
    const actionMessages: { [key: string]: string } = {
      generate: `${reportName} gerado com sucesso!`,
      view: 'PDF aberto em nova aba',
      print: 'Preparando impressão...',
    };
    return actionMessages[action] || '';
  }

  generateReport(): void {
    if (!this.actionId) return;

    const reportObservable = this.getReportObservable();
    if (!reportObservable) return;

    this.isGeneratingReport = true;
    reportObservable
      .pipe(finalize(() => (this.isGeneratingReport = false)))
      .subscribe({
        next: (blob) => {
          const filename = `${this.getFilenamePrefix()}-${
            this.actionId
          }-${this.getCurrentDate()}.pdf`;
          this.pdfService.downloadPdf(blob, filename);
          this.showSuccess(this.getSuccessMessage('generate'));
        },
        error: (error) => {
          console.error('Error generating report:', error);
          this.showError('Erro ao gerar relatório');
        },
      });
  }

  viewPdf(): void {
    if (!this.actionId) return;

    const reportObservable = this.getReportObservable();
    if (!reportObservable) return;

    this.isGeneratingReport = true;
    reportObservable
      .pipe(finalize(() => (this.isGeneratingReport = false)))
      .subscribe({
        next: (blob) => {
          this.pdfService.viewPdf(blob);
          this.showSuccess(this.getSuccessMessage('view'));
        },
        error: (error) => {
          console.error('Error viewing PDF:', error);
          this.showError('Erro ao visualizar PDF');
        },
      });
  }

  printPdf(): void {
    if (!this.actionId) return;

    const reportObservable = this.getReportObservable();
    if (!reportObservable) return;

    this.isGeneratingReport = true;
    reportObservable
      .pipe(finalize(() => (this.isGeneratingReport = false)))
      .subscribe({
        next: (blob) => {
          this.pdfService.printPdf(blob);
          this.showSuccess(this.getSuccessMessage('print'));
        },
        error: (error) => {
          console.error('Error printing PDF:', error);
          this.showError('Erro ao preparar impressão');
        },
      });
  }

  private getCurrentDate(): string {
    return new Date().toISOString().slice(0, 10).replace(/-/g, '');
  }

  private showSuccess(message: string): void {
    this.messageService.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: message,
      life: 3000,
    });
  }

  private showError(message: string): void {
    this.messageService.add({
      severity: 'error',
      summary: 'Erro',
      detail: message,
      life: 5000,
    });
  }
}
