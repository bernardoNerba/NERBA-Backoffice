import { Component, Input, OnInit } from '@angular/core';
import { IUpsert } from '../../../core/interfaces/IUpsert';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { FrameService } from '../../../core/services/frame.service';
import { SharedService } from '../../../core/services/shared.service';
import { OkResponse } from '../../../core/models/okResponse';
import { CommonModule } from '@angular/common';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { InputTextModule } from 'primeng/inputtext';
import { Frame } from '../../../core/models/frame';
import {
  FileUploadComponent,
  FileUploadData,
} from '../../../shared/components/file-upload/file-upload.component';

@Component({
  selector: 'app-upsert-frames',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ErrorCardComponent,
    InputTextModule,
    FileUploadComponent,
  ],
  templateUrl: './upsert-frames.component.html',
})
export class UpsertFramesComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: number;
  currentFrame?: Frame | null;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  // File handling properties
  programLogoFile?: File;
  financementLogoFile?: File;
  programLogoPreview?: string;
  financementLogoPreview?: string;

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private frameService: FrameService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    if (this.id !== 0) {
      this.isUpdate = true;
      this.frameService.getSingle(this.id).subscribe({
        next: (frame: Frame) => {
          this.currentFrame = frame;
          this.patchFormValues();
          this.setExistingLogoPreviews();
        },
        error: (error) => {
          this.sharedService.showError('Enquadramento não encontrado.');
          this.bsModalRef.hide();
        },
      });
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      program: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(150),
        ],
      ],
      intervention: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(55),
        ],
      ],
      interventionType: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(150),
        ],
      ],
      operation: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(150),
        ],
      ],
      operationType: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(150),
        ],
      ],
    });
  }

  patchFormValues(): void {
    this.form.patchValue({
      program: this.currentFrame?.program,
      intervention: this.currentFrame?.intervention,
      interventionType: this.currentFrame?.interventionType,
      operation: this.currentFrame?.operation,
      operationType: this.currentFrame?.operationType,
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessages = [];

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    // Validate required financement logo for new frames
    if (!this.isUpdate && !this.financementLogoFile) {
      this.sharedService.showError('Logo de financiamento é obrigatório.');
      return;
    }

    this.loading = true;

    const frameData = {
      id: this.id,
      ...this.form.value,
      programLogoFile: this.programLogoFile,
      financementLogoFile: this.financementLogoFile,
    };

    this.frameService.upsert(frameData, this.isUpdate).subscribe({
      next: (value: OkResponse) => {
        this.bsModalRef.hide();
        this.frameService.triggerFetchFrames();
        this.sharedService.showSuccess(value.title);
      },
      error: (error) => {
        this.errorMessages = this.sharedService.handleErrorResponse(error);
        this.loading = false;
      },
    });
  }

  onProgramLogoSelect(data: FileUploadData): void {
    this.programLogoFile = data.file;
    this.programLogoPreview = data.preview;
  }

  onFinancementLogoSelect(data: FileUploadData): void {
    this.financementLogoFile = data.file;
    this.financementLogoPreview = data.preview;
  }

  onProgramLogoClear(): void {
    this.programLogoFile = undefined;
    this.programLogoPreview = undefined;
  }

  onFinancementLogoClear(): void {
    this.financementLogoFile = undefined;
    this.financementLogoPreview = undefined;
  }

  onFileValidationError(error: string): void {
    this.sharedService.showError(error);
  }

  private setExistingLogoPreviews(): void {
    if (this.currentFrame?.programLogoUrl) {
      this.programLogoPreview = this.currentFrame.programLogoUrl;
    }
    if (this.currentFrame?.financementLogoUrl) {
      this.financementLogoPreview = this.currentFrame.financementLogoUrl;
    }
  }
}
