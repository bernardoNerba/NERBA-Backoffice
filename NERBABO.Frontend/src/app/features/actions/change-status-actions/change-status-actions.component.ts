import { Component, OnInit } from '@angular/core';
import { Action } from '../../../core/models/action';
import { STATUS } from '../../../core/objects/status';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { ActionsService } from '../../../core/services/actions.service';
import { SelectModule } from 'primeng/select';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-change-status-actions',
  imports: [
    SelectModule,
    ErrorCardComponent,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './change-status-actions.component.html',
})
export class ChangeStatusActionsComponent implements OnInit {
  action!: Action;
  loading: boolean = false;
  submitted: boolean = false;
  form: FormGroup = new FormGroup({});
  errorMessages: string[] = [];
  STATUS = STATUS;

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private sharedService: SharedService,
    private actionsService: ActionsService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.form.patchValue({ status: this.action.status });
  }

  onSubmit() {
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

    this.actionsService
      .changeStatus(this.action.id, this.form.value.status)
      .subscribe({
        next: (value) => {
          this.bsModalRef.hide();
          this.sharedService.showSuccess(value.message);
        },
        error: (error) => {
          this.errorMessages = this.sharedService.handleErrorResponse(error);
          this.loading = false;
        },
      });
  }

  private initializeForm(): void {
    this.form = this.formBuilder.group({
      status: ['', Validators.required],
    });
  }
}
