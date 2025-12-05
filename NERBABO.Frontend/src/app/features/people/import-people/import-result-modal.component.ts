import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { AccordionModule } from 'primeng/accordion';
import { TabsModule } from 'primeng/tabs';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { MessageComponent } from '../../../shared/components/message/message.component';

interface ImportResult {
  success: boolean;
  totalRows: number;
  successCount: number;
  failureCount: number;
  skippedCount: number;
  startedAt: string;
  completedAt: string;
  duration: string;
  summary: string;
  results: ImportRowResult[];
  globalErrors: string[];
}

interface ImportRowResult {
  rowNumber: number;
  success: boolean;
  data: any;
  errors: ImportValidationError[];
  rowData: { [key: string]: string };
}

interface ImportValidationError {
  field: string;
  errorMessage: string;
  attemptedValue?: string;
  severity: 'Warning' | 'Error';
}

@Component({
  selector: 'app-import-result-modal',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    CardModule,
    MessageModule,
    AccordionModule,
    TabsModule,
    TableModule,
    TagModule,
    MessageComponent,
  ],
  templateUrl: './import-result-modal.component.html',
})
export class ImportResultModalComponent implements OnInit {
  result!: ImportResult;
  activeTab: 'summary' | 'success' | 'failed' = 'summary';

  constructor(public bsModalRef: BsModalRef) {}

  ngOnInit(): void {
    console.log('Import result:', this.result);
  }

  get successfulResults(): ImportRowResult[] {
    return this.result.results.filter(r => r.success);
  }

  get failedResults(): ImportRowResult[] {
    return this.result.results.filter(r => !r.success);
  }

  get durationSeconds(): number {
    const match = this.result.duration.match(/(\d+):(\d+):(\d+)/);
    if (match) {
      const hours = parseInt(match[1], 10);
      const minutes = parseInt(match[2], 10);
      const seconds = parseInt(match[3], 10);
      return hours * 3600 + minutes * 60 + seconds;
    }
    return 0;
  }

  get failureWarningMessage(): string {
    return `${this.result.failureCount} linhas falharam. Consulte a aba "Falhadas" para detalhes.`;
  }

  setActiveTab(tab: 'summary' | 'success' | 'failed'): void {
    this.activeTab = tab;
  }

  onClose(): void {
    this.bsModalRef.hide();
  }

  getRowDataString(rowData: { [key: string]: string }): string {
    return Object.entries(rowData)
      .map(([key, value]) => `${key}: ${value}`)
      .join(', ');
  }
}
