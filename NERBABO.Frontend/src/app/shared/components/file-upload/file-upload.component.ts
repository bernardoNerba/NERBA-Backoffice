import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

export interface FileUploadData {
  file?: File;
  preview?: string;
}

@Component({
  selector: 'app-file-upload',
  imports: [CommonModule, Button, TooltipModule],
  templateUrl: './file-upload.component.html',
})
export class FileUploadComponent implements OnInit, OnChanges {
  @Input() label: string = 'Selecionar arquivo';
  @Input() accept: string = 'image/*';
  @Input() maxFileSize: number = 5000000; // 5MB
  @Input() required: boolean = false;
  @Input() disabled: boolean = false;
  @Input() existingFileUrl?: string;
  @Input() uploadButtonSeverity:
    | 'primary'
    | 'secondary'
    | 'success'
    | 'info'
    | 'warn'
    | 'danger'
    | 'help'
    | 'contrast' = 'primary';
  @Input() placeholderText: string = 'Nenhum arquivo selecionado';
  @Input() requiredText: string = 'Arquivo obrigatório';
  @Input() previewMaxWidth: number = 120;
  @Input() previewMaxHeight: number = 80;
  @Input() thumbnailSize: number = 32;
  @Input() uploadTooltip: string = 'Selecionar arquivo';
  @Input() clearTooltip: string = 'Limpar arquivo';

  @Output() fileSelect = new EventEmitter<FileUploadData>();
  @Output() fileClear = new EventEmitter<void>();
  @Output() validationError = new EventEmitter<string>();

  selectedFile?: File;
  previewUrl?: string;

  ngOnInit(): void {
    this.initializeExistingFile();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['existingFileUrl']) {
      this.initializeExistingFile();
    }
  }

  private initializeExistingFile(): void {
    if (this.existingFileUrl) {
      this.previewUrl = this.existingFileUrl;
    }
  }

  onFileSelect(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    if (!this.validateFile(file)) {
      // Reset file input
      event.target.value = '';
      return;
    }

    this.selectedFile = file;
    this.createImagePreview(file);

    this.fileSelect.emit({
      file: file,
      preview: this.previewUrl,
    });
  }

  private validateFile(file: File): boolean {
    // Validate file size
    if (file.size > this.maxFileSize) {
      const sizeMB = (this.maxFileSize / (1024 * 1024)).toFixed(0);
      this.validationError.emit(`O arquivo deve ser menor que ${sizeMB}MB.`);
      return false;
    }

    // Validate file type if it's an image
    if (this.accept.includes('image/') && !file.type.startsWith('image/')) {
      this.validationError.emit(
        'Por favor, selecione apenas arquivos de imagem.'
      );
      return false;
    }

    return true;
  }

  private createImagePreview(file: File): void {
    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = () => {
        this.previewUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  onClearFile(): void {
    this.selectedFile = undefined;
    this.previewUrl = this.existingFileUrl || undefined;

    this.fileClear.emit();
  }

  hasFile(): boolean {
    return !!(this.selectedFile || this.previewUrl);
  }

  getStatusText(): string {
    if (this.selectedFile) {
      return this.selectedFile.name;
    }

    if (this.previewUrl && !this.selectedFile) {
      return 'Arquivo existente';
    }

    if (this.required) {
      return this.requiredText;
    }

    return this.placeholderText;
  }
}
