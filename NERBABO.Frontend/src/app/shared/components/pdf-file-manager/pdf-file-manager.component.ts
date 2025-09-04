import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressBarModule } from 'primeng/progressbar';

export interface PdfFileConfig {
  title: string;
  description: string;
  icon?: string;
  color?: 'primary' | 'success' | 'info' | 'warning' | 'danger';
  fileNamePrefix: string;
  maxSize?: number; // in MB, defaults to 10
  accept?: string; // defaults to 'application/pdf'
}

export interface PdfFileActions {
  upload: (file: File) => void;
  download: () => void;
  view: () => void;
  delete: () => void;
}

@Component({
  selector: 'app-pdf-file-manager',
  standalone: true,
  imports: [CommonModule, Button, TooltipModule, ProgressBarModule],
  templateUrl: './pdf-file-manager.component.html',
})
export class PdfFileManagerComponent implements OnInit, AfterViewInit {
  @Input() config!: PdfFileConfig;
  @Input() pdfId: number | null | undefined = null;
  @Input() personName: string = '';
  @Input() isUploading: boolean = false;
  @Input() disabled: boolean = false;

  @Output() fileSelected = new EventEmitter<File>();
  @Output() uploadClick = new EventEmitter<void>();
  @Output() downloadClick = new EventEmitter<void>();
  @Output() viewClick = new EventEmitter<void>();
  @Output() deleteClick = new EventEmitter<void>();

  selectedFile: File | null = null;
  dragOver = false;

  ngOnInit(): void {
    // Set default values
    if (!this.config.maxSize) {
      this.config.maxSize = 10;
    }
    if (!this.config.accept) {
      this.config.accept = 'application/pdf';
    }
    if (!this.config.icon) {
      this.config.icon = 'bi-file-earmark-pdf';
    }
    if (!this.config.color) {
      this.config.color = 'primary';
    }

    // Set file input display value if file exists
    setTimeout(() => this.updateFileInputDisplay(), 0);
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    this.handleFileSelection(file);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFileSelection(files[0]);
    }
  }

  private handleFileSelection(file: File | null): void {
    if (!file) {
      this.selectedFile = null;
      return;
    }

    // Validate file type
    if (file.type !== this.config.accept) {
      this.selectedFile = null;
      // Emit error event if needed
      return;
    }

    // Validate file size
    const maxSize = (this.config.maxSize || 10) * 1024 * 1024; // Convert to bytes
    if (file.size > maxSize) {
      this.selectedFile = null;
      // Emit error event if needed
      return;
    }

    this.selectedFile = file;
    this.fileSelected.emit(file);
  }

  onUploadClick(): void {
    this.uploadClick.emit();
  }

  onDownloadClick(): void {
    this.downloadClick.emit();
  }

  onViewClick(): void {
    this.viewClick.emit();
  }

  onDeleteClick(): void {
    this.deleteClick.emit();
  }

  clearFileSelection(): void {
    this.selectedFile = null;
    // Clear the file input
    const fileInput = document.getElementById(
      this.getFileInputId()
    ) as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  getFileInputId(): string {
    return `pdf-file-input-${this.config.fileNamePrefix
      .toLowerCase()
      .replace(/[^a-z0-9]/g, '-')}`;
  }

  getFileName(): string {
    if (this.personName) {
      const sanitizedName = this.personName.replace(/\s+/g, '_');
      return `${this.config.fileNamePrefix}_${sanitizedName}.pdf`;
    }
    return `${this.config.fileNamePrefix}.pdf`;
  }

  getColorClasses(): { [key: string]: boolean } {
    const baseClass = 'pdf-manager';
    return {
      [baseClass]: true,
      [`${baseClass}--${this.config.color}`]: true,
      [`${baseClass}--disabled`]: this.disabled,
      [`${baseClass}--drag-over`]: this.dragOver,
    };
  }

  hasFile(): boolean {
    return this.pdfId !== null && this.pdfId !== undefined;
  }

  private updateFileInputDisplay(): void {
    if (this.hasFile()) {
      const fileInput = document.getElementById(
        this.getFileInputId()
      ) as HTMLInputElement;
      if (fileInput) {
        const fileName = this.getFileName();
        fileInput.setAttribute('data-filename', fileName);
        fileInput.style.setProperty('--filename', `"${fileName}"`);
      }
    }
  }

  ngAfterViewInit(): void {
    this.updateFileInputDisplay();
  }

  openFileDialog(): void {
    if (this.disabled) return;
    
    const fileInput = document.getElementById(
      this.getFileInputId()
    ) as HTMLInputElement;
    if (fileInput) {
      fileInput.click();
    }
  }

  getDisplayText(): string {
    if (this.selectedFile) {
      return this.selectedFile.name;
    }
    if (this.hasFile()) {
      return this.getFileName();
    }
    return 'Nenhum ficheiro...';
  }

  onUploadSuccess(): void {
    // Clear the selected file after successful upload
    this.selectedFile = null;
    
    // Clear the file input value
    const fileInput = document.getElementById(
      this.getFileInputId()
    ) as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
    
    // Update the display to show the new file exists
    this.updateFileInputDisplay();
  }
}
