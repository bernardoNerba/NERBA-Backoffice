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
import { Frame } from '../../../core/models/frame';
import { OkResponse } from '../../../core/models/okResponse';

@Component({
  selector: 'app-update-frames',
  templateUrl: './update-frames.component.html',
  imports: [CommonModule, ReactiveFormsModule, ErrorCardComponent],
})
export class UpdateFramesComponent implements OnInit {
  id!: number;
  frame!: Frame;
  form!: FormGroup;
  submitted = false;
  loading = false;
  errorMessages: Array<string> = [];

  constructor(
    public bsModalRef: BsModalRef,
    private fb: FormBuilder,
    private frameService: FrameService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.patchForm();
  }

  initializeForm(): void {
    this.form = this.fb.group({
      program: ['', Validators.required],
      intervention: ['', Validators.required],
      interventionType: ['', Validators.required],
      operation: ['', Validators.required],
      operationType: ['', Validators.required],
    });
  }

  private patchForm(): void {
    this.form.patchValue({
      program: this.frame.program,
      intervention: this.frame.intervention,
      interventionType: this.frame.interventionType,
      operation: this.frame.operation,
      operationType: this.frame.operationType,
    });
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.value;
    this.loading = true;

    this.frameService
      .update(this.id, {
        id: this.id,
        program: value.program,
        intervention: value.intervention,
        interventionType: value.interventionType,
        operation: value.operation,
        operationType: value.operationType,
      })
      .subscribe({
        next: (value: OkResponse) => {
          this.bsModalRef.hide();
          this.frameService.triggerFetchFrames();
          this.sharedService.showSuccess(value.message);
        },
        error: (error) => {
          this.errorMessages = this.sharedService.handleErrorResponse(error);
          this.loading = false;
        },
      });
  }
}
