import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { PeopleService } from '../../../core/services/people.service';
import { SharedService } from '../../../core/services/shared.service';
import { ImportResultModalComponent } from './import-result-modal.component';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { FileUploadModule } from 'primeng/fileupload';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageComponent } from '../../../shared/components/message/message.component';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-import-people',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    CardModule,
    MessageModule,
    FileUploadModule,
    ProgressSpinnerModule,
    MessageComponent,
    DialogModule,
    TooltipModule,
  ],
  templateUrl: './import-people.component.html',
  styleUrl: './import-people.component.css',
})
export class ImportPeopleComponent implements OnInit {
  selectedFile: File | null = null;
  fileType: 'csv' | 'excel' | null = null;
  isLoading = false;
  errorMessage: string | null = null;
  showInstructions = false;

  constructor(
    public bsModalRef: BsModalRef,
    private peopleService: PeopleService,
    private sharedService: SharedService,
    private modalService: BsModalService
  ) {}

  ngOnInit(): void {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile = file;
      this.errorMessage = null;

      // Determine file type
      if (file.name.endsWith('.csv')) {
        this.fileType = 'csv';
      } else if (file.name.endsWith('.xlsx') || file.name.endsWith('.xls')) {
        this.fileType = 'excel';
      } else {
        this.errorMessage = 'Tipo de ficheiro inválido. Por favor selecione um ficheiro CSV ou Excel (.xlsx, .xls)';
        this.selectedFile = null;
        this.fileType = null;
      }
    }
  }

  onUpload(): void {
    if (!this.selectedFile || !this.fileType) {
      this.errorMessage = 'Por favor selecione um ficheiro válido';
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    const uploadObservable = this.fileType === 'csv'
      ? this.peopleService.importPeopleFromCsv(this.selectedFile)
      : this.peopleService.importPeopleFromExcel(this.selectedFile);

    uploadObservable.subscribe({
      next: (response) => {
        this.isLoading = false;
        this.bsModalRef.hide();

        // Show result modal
        const initialState = {
          result: response.data
        };
        this.modalService.show(ImportResultModalComponent, {
          initialState,
          class: 'modal-xl',
        });

        this.sharedService.showInfo(
          response.data.summary
        );
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Import error:', err);

        if (err.error?.message) {
          this.errorMessage = err.error.message;
        } else if (err.error?.title) {
          this.errorMessage = err.error.title;
        } else {
          this.errorMessage = 'Erro ao importar ficheiro. Por favor tente novamente.';
        }

        this.sharedService.showError(this.errorMessage || 'Erro ao importar ficheiro');
      },
    });
  }

  onCancel(): void {
    this.bsModalRef.hide();
  }

  downloadCsvTemplate(): void {
    this.peopleService.downloadCsvTemplate().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'People_Import_Template.csv';
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Error downloading CSV template:', err);
        this.sharedService.showError('Erro ao descarregar template CSV');
      },
    });
  }

  downloadExcelTemplate(): void {
    this.peopleService.downloadExcelTemplate().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'People_Import_Template.xlsx';
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Error downloading Excel template:', err);
        this.sharedService.showError('Erro ao descarregar template Excel');
      },
    });
  }

  clearFile(): void {
    this.selectedFile = null;
    this.fileType = null;
    this.errorMessage = null;
  }

  showInstructionsDialog(): void {
    this.showInstructions = true;
  }
}
