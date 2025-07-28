import { Component, Input, OnInit } from '@angular/core';
import { IUpsert } from '../../../core/interfaces/IUpsert';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { CompaniesService } from '../../../core/services/companies.service';
import { SharedService } from '../../../core/services/shared.service';
import { OkResponse } from '../../../core/models/okResponse';
import { Company } from '../../../core/models/company';
import { CommonModule } from '@angular/common';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { ACTIVITY_SECTOR } from '../../../core/objects/ativitySector';
import { COMPANY_SIZE } from '../../../core/objects/companySize';

@Component({
  selector: 'app-upsert-companies',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    ErrorCardComponent,
    InputTextModule,
    TextareaModule,
    SelectModule,
  ],
  templateUrl: './upsert-companies.component.html',
})
export class UpsertCompaniesComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: number;
  currentCompany?: Company | null;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  activitySectors = ACTIVITY_SECTOR;
  companySizes = COMPANY_SIZE;

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private companiesService: CompaniesService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();

    if (this.id !== 0) {
      this.isUpdate = true;

      this.companiesService.getCompanyById(this.id).subscribe({
        next: (company: Company) => {
          this.currentCompany = company;
          this.patchFormValues();
        },
        error: (error) => {
          this.sharedService.showError('Empresa não encontrada.');
          this.bsModalRef.hide();
        },
      });
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      id: this.id,
      name: [
        '',
        [
          Validators.required,
          Validators.maxLength(155),
          Validators.minLength(3),
        ],
      ],
      address: [''],
      phoneNumber: ['', [Validators.maxLength(9), Validators.minLength(9)]],
      locality: ['', [Validators.maxLength(55), Validators.minLength(3)]],
      zipCode: ['', [Validators.maxLength(8), Validators.minLength(8)]],
      email: ['', [Validators.email]],
      ativitySector: ['', [Validators.required]],
      size: ['', [Validators.required]],
    });
  }

  patchFormValues(): void {
    this.form.patchValue({
      name: this.currentCompany?.name,
      address: this.currentCompany?.address,
      phoneNumber: this.currentCompany?.phoneNumber,
      locality: this.currentCompany?.locality,
      zipCode: this.currentCompany?.zipCode,
      email: this.currentCompany?.email,
      ativitySector: this.currentCompany?.ativitySector,
      size: this.currentCompany?.size,
    });
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }
    this.loading = true;

    this.companiesService
      .upsert({ id: this.id, ...this.form.value }, this.isUpdate)
      .subscribe({
        next: (value: OkResponse) => {
          this.bsModalRef.hide();
          this.companiesService.triggerFetch();
          this.sharedService.showSuccess(value.message);
        },
        error: (error) => {
          this.errorMessages = this.sharedService.handleErrorResponse(error);
          this.loading = false;
        },
      });
  }
}
