import { Component, Input } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ACTIVITY_SECTOR } from '../../../core/objects/ativitySector';
import { COMPANY_SIZE } from '../../../core/objects/companySize';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { CompaniesService } from '../../../core/services/companies.service';
import { SharedService } from '../../../core/services/shared.service';
import { Company } from '../../../core/models/company';
import { OkResponse } from '../../../core/models/okResponse';
import { CommonModule } from '@angular/common';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';

@Component({
  selector: 'app-update-companies',
  imports: [ReactiveFormsModule, CommonModule, ErrorCardComponent],
  templateUrl: './update-companies.component.html',
  styleUrl: './update-companies.component.css',
})
export class UpdateCompaniesComponent {
  @Input({ required: true }) id!: number;
  currentCompany!: Company;
  submitted = false;
  loading = false;
  errorMessages: string[] = [];
  updateCompanyForm: FormGroup = new FormGroup({});

  activitySectors = ACTIVITY_SECTOR.map((value) => value.value.toLowerCase());
  companySizes = COMPANY_SIZE.map((value) => value.value.toLowerCase());

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private companiesService: CompaniesService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    // Initialize form first
    this.initializeForm();

    this.loading = true;
    this.companiesService.getCompanyById(this.id).subscribe({
      next: (company: Company) => {
        this.currentCompany = company;
        // Update form values after initialization
        this.updateCompanyForm.patchValue({
          name: this.currentCompany.name,
          address: this.currentCompany.address,
          phoneNumber: this.currentCompany.phoneNumber,
          locality: this.currentCompany.locality,
          zipCode: this.currentCompany.zipCode,
          email: this.currentCompany.email,
          ativitySector: this.currentCompany.ativitySector.toLowerCase(),
          size: this.currentCompany.size.toLowerCase(),
        });
        this.loading = false;
      },
      error: (error) => {
        this.errorMessages = this.sharedService.handleErrorResponse(error);
        this.loading = false;
      },
    });
  }

  private initializeForm() {
    // Initialize form with default values
    this.updateCompanyForm = this.formBuilder.group({
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

  onSubmit() {
    this.submitted = true;

    if (this.updateCompanyForm.invalid) {
      this.updateCompanyForm.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    const formValue = this.updateCompanyForm.value;
    console.log(formValue);
    this.loading = true;

    this.companiesService.updateCompany(formValue, this.id).subscribe({
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
