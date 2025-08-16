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
  imports: [
    CommonModule,
    ButtonModule,
    DropdownModule,
    TooltipModule
  ],
  template: `
    <div class="d-flex flex-wrap gap-2 align-items-center">
      <p-button 
        *ngIf="actionId"
        label="Relatório de Sessões"
        icon="pi pi-file-pdf"
        severity="secondary"
        size="small"
        [loading]="isGeneratingReport"
        [disabled]="isGenerating"
        (onClick)="generateSessionsReport()"
        pTooltip="Gerar PDF com todas as sessões desta ação"
        tooltipPosition="top">
      </p-button>

      <p-button 
        *ngIf="sessionId"
        label="Detalhe da Sessão"
        icon="pi pi-file-pdf"
        severity="secondary" 
        size="small"
        [loading]="isGeneratingDetail"
        [disabled]="isGenerating"
        (onClick)="generateSessionDetail()"
        pTooltip="Gerar PDF com detalhes desta sessão"
        tooltipPosition="top">
      </p-button>

      <p-button 
        *ngIf="actionId"
        label="Resumo da Ação"
        icon="pi pi-file-pdf"
        severity="info"
        size="small" 
        [loading]="isGeneratingSummary"
        [disabled]="isGenerating"
        (onClick)="generateActionSummary()"
        pTooltip="Gerar PDF com resumo desta ação"
        tooltipPosition="top">
      </p-button>

      <p-dropdown 
        [options]="pdfActions"
        optionLabel="label"
        optionValue="value"
        placeholder="Mais opções PDF..."
        [disabled]="isGenerating"
        size="small"
        (onChange)="onPdfActionSelect($event)"
        [showClear]="false">
        <ng-template pTemplate="selectedItem" let-selectedOption>
          <div class="flex align-items-center gap-2">
            <i [class]="selectedOption?.icon"></i>
            <span>{{ selectedOption?.label }}</span>
          </div>
        </ng-template>
        <ng-template pTemplate="item" let-option>
          <div class="flex align-items-center gap-2" [title]="option.tooltip">
            <i [class]="option.icon"></i>
            <span>{{ option.label }}</span>
          </div>
        </ng-template>
      </p-dropdown>
    </div>
  `
})
export class PdfActionsComponent {
  @Input() actionId?: number;
  @Input() sessionId?: number;
  @Input() actionTitle?: string;

  isGeneratingReport = false;
  isGeneratingDetail = false;
  isGeneratingSummary = false;

  pdfActions: PdfOption[] = [
    {
      label: 'Visualizar',
      value: 'view',
      icon: 'pi pi-eye',
      tooltip: 'Visualizar PDF no navegador'
    },
    {
      label: 'Imprimir',
      value: 'print',
      icon: 'pi pi-print',
      tooltip: 'Imprimir PDF diretamente'
    }
  ];

  constructor(
    private pdfService: PdfService,
    private messageService: MessageService
  ) {}

  get isGenerating(): boolean {
    return this.isGeneratingReport || this.isGeneratingDetail || this.isGeneratingSummary;
  }

  generateSessionsReport(): void {
    if (!this.actionId) return;

    this.isGeneratingReport = true;
    this.pdfService.generateSessionsReport(this.actionId)
      .pipe(finalize(() => this.isGeneratingReport = false))
      .subscribe({
        next: (blob) => {
          const filename = `relatorio-sessoes-acao-${this.actionId}-${this.getCurrentDate()}.pdf`;
          this.pdfService.downloadPdf(blob, filename);
          this.showSuccess('Relatório de sessões gerado com sucesso!');
        },
        error: (error) => {
          console.error('Error generating sessions report:', error);
          this.showError('Erro ao gerar relatório de sessões');
        }
      });
  }

  generateSessionDetail(): void {
    if (!this.sessionId) return;

    this.isGeneratingDetail = true;
    this.pdfService.generateSessionDetail(this.sessionId)
      .pipe(finalize(() => this.isGeneratingDetail = false))
      .subscribe({
        next: (blob) => {
          const filename = `detalhe-sessao-${this.sessionId}-${this.getCurrentDate()}.pdf`;
          this.pdfService.downloadPdf(blob, filename);
          this.showSuccess('Detalhe da sessão gerado com sucesso!');
        },
        error: (error) => {
          console.error('Error generating session detail:', error);
          this.showError('Erro ao gerar detalhe da sessão');
        }
      });
  }

  generateActionSummary(): void {
    if (!this.actionId) return;

    this.isGeneratingSummary = true;
    this.pdfService.generateActionSummary(this.actionId)
      .pipe(finalize(() => this.isGeneratingSummary = false))
      .subscribe({
        next: (blob) => {
          const filename = `resumo-acao-${this.actionId}-${this.getCurrentDate()}.pdf`;
          this.pdfService.downloadPdf(blob, filename);
          this.showSuccess('Resumo da ação gerado com sucesso!');
        },
        error: (error) => {
          console.error('Error generating action summary:', error);
          this.showError('Erro ao gerar resumo da ação');
        }
      });
  }

  onPdfActionSelect(event: any): void {
    const action = event.value;
    
    if (action === 'view') {
      this.viewPdf();
    } else if (action === 'print') {
      this.printPdf();
    }
  }

  private viewPdf(): void {
    if (this.actionId) {
      this.isGeneratingReport = true;
      this.pdfService.generateSessionsReport(this.actionId)
        .pipe(finalize(() => this.isGeneratingReport = false))
        .subscribe({
          next: (blob) => {
            this.pdfService.viewPdf(blob);
            this.showSuccess('PDF aberto em nova aba');
          },
          error: (error) => {
            console.error('Error viewing PDF:', error);
            this.showError('Erro ao visualizar PDF');
          }
        });
    }
  }

  private printPdf(): void {
    if (this.actionId) {
      this.isGeneratingReport = true;
      this.pdfService.generateSessionsReport(this.actionId)
        .pipe(finalize(() => this.isGeneratingReport = false))
        .subscribe({
          next: (blob) => {
            this.pdfService.printPdf(blob);
            this.showSuccess('Preparando impressão...');
          },
          error: (error) => {
            console.error('Error printing PDF:', error);
            this.showError('Erro ao preparar impressão');
          }
        });
    }
  }

  private getCurrentDate(): string {
    return new Date().toISOString().slice(0, 10).replace(/-/g, '');
  }

  private showSuccess(message: string): void {
    this.messageService.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: message,
      life: 3000
    });
  }

  private showError(message: string): void {
    this.messageService.add({
      severity: 'error',
      summary: 'Erro',
      detail: message,
      life: 5000
    });
  }
}