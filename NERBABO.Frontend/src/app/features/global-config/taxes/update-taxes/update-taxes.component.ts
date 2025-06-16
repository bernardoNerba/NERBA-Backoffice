import { Component, Input, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { UniversalValidators } from 'ngx-validators';
import { ErrorCardComponent } from '../../../../shared/components/error-card/error-card.component';
import { CommonModule } from '@angular/common';
import { SharedService } from '../../../../core/services/shared.service';
import { ConfigService } from '../../../../core/services/config.service';
import { Tax } from '../../../../core/models/tax';
import { OkResponse } from '../../../../core/models/okResponse';

@Component({
  selector: 'app-update-taxes',
  imports: [ErrorCardComponent, CommonModule, ReactiveFormsModule],
  templateUrl: './update-taxes.component.html',
  styleUrl: './update-taxes.component.css',
})
export class UpdateTaxesComponent implements OnInit {
  currentTax!: Tax;
  form: FormGroup = new FormGroup({});
  submitted = false;
  loading = false;
  errorMessages = [];
  type!: string;

  constructor(
    public bsModalRef: BsModalRef,
    private fb: FormBuilder,
    private sharedService: SharedService,
    private confService: ConfigService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.pathForm();
  }

  private initializeForm() {
    console.log(this.currentTax);
    this.form = this.fb.group({
      name: [
        this.currentTax.name,

        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(50),
        ],
      ],
      valuePercent: [
        this.currentTax.valuePercent,
        [Validators.required, UniversalValidators.isInRange(0, 100)],
      ],
      type: ['', [Validators.required]],
    });
  }

  private pathForm(): void {
    this.form.patchValue({
      name: this.currentTax.name,
      valuePercent: this.currentTax.valuePercent,
      type: this.type,
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

    const model = this.form.value;
    model.id = this.currentTax.id;

    this.confService.updateIvaTax(model).subscribe({
      next: (value: OkResponse) => {
        console.log(value);
        this.confService.triggerFetchConfigs();
        this.sharedService.showSuccess(value.message);
        this.loading = false;
        this.bsModalRef.hide();
      },
      error: (error) => {
        this.sharedService.handleErrorResponse(error);
        this.loading = false;
      },
    });

    this.submitted = false;
  }
}
