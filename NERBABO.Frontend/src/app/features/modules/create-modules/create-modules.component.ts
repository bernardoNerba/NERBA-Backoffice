import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ModulesService } from '../../../core/services/modules.service';
import { SharedService } from '../../../core/services/shared.service';
import { UniversalValidators } from 'ngx-validators';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-create-modules',
  imports: [ReactiveFormsModule, ErrorCardComponent, CommonModule],
  templateUrl: './create-modules.component.html',
})
export class CreateModulesComponent implements OnInit {
  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});
  submitted: boolean = false;
  loading: boolean = false;

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private moduleService: ModulesService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm() {
    this.form = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(255),
        ],
      ],
      hours: [
        '',
        [Validators.required, UniversalValidators.isInRange(0, 1000)],
      ],
      isActive: [true],
    });
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

    this.moduleService.createModule(this.form.value).subscribe({
      next: (value) => {
        this.bsModalRef.hide();
        this.moduleService.triggerFetch();
        this.sharedService.showSuccess(value.message);
      },
      error: (error) => {
        this.errorMessages = this.sharedService.handleErrorResponse(error);
        this.loading = false;
      },
    });
  }
}
