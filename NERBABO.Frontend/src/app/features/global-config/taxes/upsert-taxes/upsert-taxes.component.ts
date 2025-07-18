import { Component, Input, OnInit } from '@angular/core';
import { ErrorCardComponent } from '../../../../shared/components/error-card/error-card.component';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { IUpsert } from '../../../../core/interfaces/IUpsert';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../../core/services/shared.service';
import { ConfigService } from '../../../../core/services/config.service';
import { Tax } from '../../../../core/models/tax';
import { UniversalValidators } from 'ngx-validators';
import { OkResponse } from '../../../../core/models/okResponse';
import { SelectModule } from 'primeng/select';
import { TAX_TYPES } from '../../../../core/objects/taxType';

@Component({
  selector: 'app-upsert-taxes',
  imports: [
    ErrorCardComponent,
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
  ],
  templateUrl: './upsert-taxes.component.html',
})
export class UpsertTaxesComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: string | number;
  currentTax?: Tax | null;
  type?: string;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  allTaxes = TAX_TYPES;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private sharedService: SharedService,
    private confService: ConfigService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    if (this.id !== 0) {
      this.isUpdate = true;
      this.patchFormValues();
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      id: 0,
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(50),
        ],
      ],
      valuePercent: [
        '',
        [Validators.required, UniversalValidators.isInRange(0, 100)],
      ],
      type: ['', [Validators.required]],
      isActive: true,
    });
  }

  patchFormValues(): void {
    this.form.patchValue({
      id: this.id,
      name: this.currentTax?.name,
      valuePercent: this.currentTax?.valuePercent,
      type: this.type,
      isActive: this.currentTax?.isActive,
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

    this.confService.upsert(this.form.value, this.isUpdate).subscribe({
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
