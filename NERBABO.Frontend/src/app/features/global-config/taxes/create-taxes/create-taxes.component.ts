import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { UniversalValidators } from 'ngx-validators';
import { ErrorCardComponent } from '../../../../shared/components/error-card/error-card.component';
import { SharedService } from '../../../../core/services/shared.service';
import { ConfigService } from '../../../../core/services/config.service';
import { OkResponse } from '../../../../core/models/okResponse';
import { TAX_TYPES } from '../../../../core/objects/taxType';

@Component({
  selector: 'app-create-taxes',
  imports: [ReactiveFormsModule, ErrorCardComponent],
  templateUrl: './create-taxes.component.html',
  styleUrl: './create-taxes.component.css',
})
export class CreateTaxesComponent implements OnInit {
  allTaxes = [...TAX_TYPES];
  form: FormGroup = new FormGroup({});
  submitted = false;
  loading = false;
  errorMessages = [];

  constructor(
    public bsModalRef: BsModalRef,
    private fb: FormBuilder,
    private sharedService: SharedService,
    private confService: ConfigService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm() {
    this.form = this.fb.group({
      name: [
        '',
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50),
      ],
      valuePercent: [
        '',
        [Validators.required, UniversalValidators.isInRange(0, 100)],
      ],
      type: ['', [Validators.required]],
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

    this.confService.createIvaTax(model).subscribe({
      next: (value: OkResponse) => {
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
