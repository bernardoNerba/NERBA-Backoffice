import { Component, Input, OnInit } from '@angular/core';
import { IUpsert } from '../../../core/interfaces/IUpsert';
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
import { Module } from '../../../core/models/module';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { InputNumber } from 'primeng/inputnumber';
import { CommonModule } from '@angular/common';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-upsert-modules',
  imports: [
    ErrorCardComponent,
    InputNumber,
    ReactiveFormsModule,
    CommonModule,
    InputTextModule,
  ],
  templateUrl: './upsert-modules.component.html',
})
export class UpsertModulesComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: number;
  currentModule?: Module | null;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private moduleService: ModulesService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    if (this.id !== 0) {
      this.isUpdate = true;
      this.moduleService.getSingleModule(this.id).subscribe({
        next: (module: Module) => {
          this.currentModule = module;
          this.patchFormValues();
        },
        error: () => {
          this.sharedService.showError('Módulo não encontrado.');
          this.bsModalRef.hide();
        },
      });
    }
  }

  initializeForm(): void {
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

  patchFormValues(): void {
    this.form.patchValue({
      name: this.currentModule?.name,
      hours: this.currentModule?.hours,
      isActive: this.currentModule?.isActive,
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

    this.moduleService
      .upsertModule({ id: this.id, ...this.form.value }, this.isUpdate)
      .subscribe({
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
