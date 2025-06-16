import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { FrameService } from '../../../core/services/frame.service';
import { CommonModule } from '@angular/common';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { SharedService } from '../../../core/services/shared.service';
import { OkResponse } from '../../../core/models/okResponse';

@Component({
  selector: 'app-create-frames',
  templateUrl: './create-frames.component.html',
  styleUrl: './create-frames.component.css',
  imports: [CommonModule, ReactiveFormsModule, ErrorCardComponent],
})
export class CreateFramesComponent implements OnInit {
  errorMessages: Array<string> = [];
  form: FormGroup = new FormGroup({});
  submitted = false;
  loading = false;

  constructor(
    public bsModalRef: BsModalRef,
    private fb: FormBuilder,
    private frameService: FrameService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(): void {
    this.form = this.fb.group({
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

    this.loading = true;

    this.frameService.create(this.form.value).subscribe({
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
}
