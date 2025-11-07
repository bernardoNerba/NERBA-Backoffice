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
import { IUpsert } from '../../../../core/interfaces/IUpsert';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../../core/services/shared.service';
import { ConfigService } from '../../../../core/services/config.service';
import { OkResponse } from '../../../../core/models/okResponse';
import { ModuleCategory } from '../module-category.model';

@Component({
  selector: 'app-upsert-module-categories',
  imports: [
    ErrorCardComponent,
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
  ],
  templateUrl: './upsert-module-categories.component.html',
})
export class UpsertModuleCategoriesComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: string | number;
  currentCategory?: ModuleCategory | null;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

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
    if (this.currentCategory) {
      this.isUpdate = true;
      this.patchFormValues();
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      id: [null],
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(155),
        ],
      ],
      shortenName: [
        '',
        [
          Validators.required,
          Validators.minLength(1),
          Validators.maxLength(15),
        ],
      ],
    });
  }

  patchFormValues(): void {
    this.form.patchValue({
      id: this.currentCategory?.id,
      name: this.currentCategory?.name,
      shortenName: this.currentCategory?.shortenName,
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

    console.log(this.form.value)

    this.loading = true;

    this.confService.upsertModuleCategory(this.form.value, this.isUpdate).subscribe({
      next: (value: OkResponse) => {
        console.log(value);
        this.confService.triggerFetchConfigs();
        this.sharedService.showSuccess(value.message);
        this.loading = false;
        this.bsModalRef.hide();
      },
      error: (error) => {
        console.error(error)
        this.sharedService.handleErrorResponse(error);
        this.loading = false;
      },
    });

    this.submitted = false;
  }
}
