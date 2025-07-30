import { Component, Input, OnInit } from '@angular/core';
import { COUNTRIES } from '../../../core/objects/countries';
import {
  HabilitationEnum,
  HABILITATIONS,
} from '../../../core/objects/habilitations';
import { IDENTIFICATION_TYPES } from '../../../core/objects/identificationType';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { PeopleService } from '../../../core/services/people.service';
import { SharedService } from '../../../core/services/shared.service';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { Select } from 'primeng/select';
import {
  AutoCompleteCompleteEvent,
  AutoCompleteModule,
} from 'primeng/autocomplete';
import { GENDERS } from '../../../core/objects/gender';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { IUpsert } from '../../../core/interfaces/IUpsert';
import { Person } from '../../../core/models/person';
import { convertDateOnlyToPtDate, matchDateOnly } from '../../../shared/utils';

@Component({
  selector: 'app-upsert-people',
  imports: [
    ReactiveFormsModule,
    ErrorCardComponent,
    Select,
    AutoCompleteModule,
    DatePickerModule,
    InputTextModule,
    TextareaModule,
  ],
  templateUrl: './upsert-people.component.html',
})
export class UpsertPeopleComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: number;
  currentPerson?: Person | null;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  allCountries = COUNTRIES;
  filteredCountries: any;
  allGender = GENDERS;
  allHabilitations = HABILITATIONS;
  allIdentificationTypes = IDENTIFICATION_TYPES;

  form: FormGroup = new FormGroup({});
  errorMessages: string[] = [];

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private peopleService: PeopleService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    if (this.id !== 0) {
      this.isUpdate = true;
      const personFromService = this.peopleService.personById(this.id);
      if (personFromService) this.currentPerson = personFromService;
      this.patchFormValues();
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      firstName: [
        '',
        [
          Validators.required,
          Validators.maxLength(100),
          Validators.minLength(3),
        ],
      ],
      lastName: [
        '',
        [
          Validators.required,
          Validators.maxLength(100),
          Validators.minLength(3),
        ],
      ],
      nif: [
        '',
        [Validators.required, Validators.maxLength(9), Validators.minLength(9)],
      ],
      identificationNumber: [
        '',
        [Validators.maxLength(10), Validators.minLength(5)],
      ],
      identificationValidationDate: [''],
      identificationType: ['Não Especificado'],
      niss: ['', [Validators.maxLength(11), Validators.minLength(11)]],
      iban: ['', [Validators.maxLength(25), Validators.minLength(25)]],
      birthDate: [''],
      address: [''],
      zipCode: ['', [Validators.maxLength(8), Validators.minLength(8)]],
      phoneNumber: ['', [Validators.maxLength(9), Validators.minLength(9)]],
      email: [
        '',
        [Validators.email, Validators.maxLength(50), Validators.minLength(5)],
      ],
      naturality: ['', [Validators.maxLength(100), Validators.minLength(3)]],
      nationality: ['', [Validators.maxLength(100), Validators.minLength(3)]],
      gender: ['Não Especificado'],
      habilitation: [HabilitationEnum.WithoutProof],
    });
  }

  patchFormValues(): void {
    this.form.patchValue({
      firstName: this.currentPerson?.firstName,
      lastName: this.currentPerson?.lastName,
      nif: this.currentPerson?.nif,
      identificationNumber: this.currentPerson?.identificationNumber,
      identificationValidationDate: convertDateOnlyToPtDate(
        this.currentPerson?.identificationValidationDate ?? ''
      ),
      identificationType: this.currentPerson?.identificationType,
      niss: this.currentPerson?.niss,
      iban: this.currentPerson?.iban,
      birthDate: convertDateOnlyToPtDate(this.currentPerson?.birthDate ?? ''),
      address: this.currentPerson?.address,
      zipCode: this.currentPerson?.zipCode,
      phoneNumber: this.currentPerson?.phoneNumber,
      email: this.currentPerson?.email,
      naturality: this.currentPerson?.naturality,
      nationality: this.currentPerson?.nationality,
      gender: this.currentPerson?.gender,
      habilitation: this.currentPerson?.habilitation,
    });
  }

  filterCountry(event: AutoCompleteCompleteEvent) {
    let filtered: any[] = [];
    let query = event.query;

    for (let i = 0; i < (this.allCountries as any[]).length; i++) {
      let country = (this.allCountries as any[])[i];
      if (country.gentilico.toLowerCase().indexOf(query.toLowerCase()) == 0) {
        filtered.push(country);
      }
    }

    this.filteredCountries = filtered;
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessages = [];

    // convert date values to match DateOnly fields
    this.form.value.birthDate = matchDateOnly(this.form.value.birthDate);
    this.form.value.identificationValidationDate = matchDateOnly(
      this.form.value.identificationValidationDate
    );
    console.log(this.form.value);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    this.loading = true;

    this.peopleService
      .upsertPerson({ id: this.id, ...this.form.value }, this.isUpdate)
      .subscribe({
        next: (value) => {
          this.bsModalRef.hide();
          this.peopleService.triggerFetchPeople();
          this.sharedService.showSuccess(value.message);
        },
        error: (error) => {
          this.errorMessages = this.sharedService.handleErrorResponse(error);
          this.loading = false;
        },
      });
  }
}
