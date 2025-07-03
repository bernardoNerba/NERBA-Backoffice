import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { CompaniesService } from '../../../core/services/companies.service';
import { SharedService } from '../../../core/services/shared.service';
import { ACTIVITY_SECTOR } from '../../../core/objects/ativitySector';
import { COMPANY_SIZE } from '../../../core/objects/companySize';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { OkResponse } from '../../../core/models/okResponse';

@Component({
  selector: 'app-create-companies',
  imports: [ReactiveFormsModule, ErrorCardComponent],
  templateUrl: './create-companies.component.html',
  styleUrl: './create-companies.component.css',
})
export class CreateCompaniesComponent implements OnInit {
  submitted = false;
  loading = false;
  errorMessages: string[] = [];
  createCompanyForm: FormGroup = new FormGroup({});
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
  }

  private initializeForm() {
    this.createCompanyForm = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.maxLength(155),
          Validators.minLength(3),
        ],
      ],
      address: ['', []],
      phoneNumber: ['', [Validators.maxLength(9), Validators.minLength(9)]],
      locality: ['', [Validators.maxLength(55), Validators.minLength(3)]],
      zipCode: ['', [Validators.maxLength(8), Validators.minLength(8)]],
      email: ['', [Validators.email]],
      ativitySector: [this.activitySectors[0].value, [Validators.required]],
      size: [this.companySizes[0].value, [Validators.required]],
    });
  }

  onSubmit() {
    this.submitted = true;

    if (this.createCompanyForm.invalid) {
      this.createCompanyForm.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    const formValue = this.createCompanyForm.value;
    this.loading = true;

    this.companiesService.createCompany(formValue).subscribe({
      next: (value: OkResponse) => {
        console.log(value);
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
