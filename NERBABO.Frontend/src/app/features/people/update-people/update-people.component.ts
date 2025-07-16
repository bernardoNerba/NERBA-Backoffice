import { Component, Input, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { HABILITATIONS } from '../../../core/objects/habilitations';
import { COUNTRIES } from '../../../core/objects/countries';
import { PeopleService } from '../../../core/services/people.service';
import { IDENTIFICATION_TYPES } from '../../../core/objects/identificationType';
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';
import { CommonModule } from '@angular/common';
import { SharedService } from '../../../core/services/shared.service';
import { Person } from '../../../core/models/person';
import { GENDERS } from '../../../core/objects/gender';
import { convertDateOnlyToPtDate } from '../../../shared/utils';
import { Select } from 'primeng/select';
import {
  AutoCompleteCompleteEvent,
  AutoCompleteModule,
} from 'primeng/autocomplete';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';

@Component({
  selector: 'app-update-people',
  imports: [
    CommonModule,
    ErrorCardComponent,
    ReactiveFormsModule,
    TypeaheadModule,
    Select,
    AutoCompleteModule,
    DatePickerModule,
    InputTextModule,
    TextareaModule,
  ],
  templateUrl: './update-people.component.html',
})
export class UpdatePeopleComponent implements OnInit {
  allCountries = [...COUNTRIES];
  filteredCountries: any;
  allGender = [...GENDERS];
  allHabilitations = [...HABILITATIONS];
  allIdentificationTypes = [...IDENTIFICATION_TYPES];
  @Input() id!: number;
  submitted = false;
  errorMessages: string[] = [];
  updatePersonForm: FormGroup = new FormGroup({});
  loading = false;

  currentPerson!: Person;

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private peopleService: PeopleService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    const personFromService = this.peopleService.personById(this.id);
    if (personFromService) this.currentPerson = personFromService;

    this.initializeForm();
    this.updatePersonForm.patchValue({
      birthDate: convertDateOnlyToPtDate(this.currentPerson.birthDate),
      identificationValidationDate: convertDateOnlyToPtDate(
        this.currentPerson.identificationValidationDate
      ),
    });
  }

  private initializeForm() {
    this.updatePersonForm = this.formBuilder.group({
      firstName: [
        this.currentPerson.firstName,
        [
          Validators.required,
          Validators.maxLength(100),
          Validators.minLength(3),
        ],
      ],
      lastName: [
        this.currentPerson.lastName,
        [
          Validators.required,
          Validators.maxLength(100),
          Validators.minLength(3),
        ],
      ],
      nif: [
        this.currentPerson.nif,
        [Validators.required, Validators.maxLength(9), Validators.minLength(9)],
      ],
      identificationNumber: [
        this.currentPerson.identificationNumber,
        [Validators.maxLength(10), Validators.minLength(5)],
      ],
      identificationValidationDate: [
        this.currentPerson.identificationValidationDate,
      ],
      identificationType: [this.currentPerson.identificationType],
      niss: [
        this.currentPerson.niss,
        [Validators.maxLength(11), Validators.minLength(11)],
      ],
      iban: [
        this.currentPerson.iban,
        [Validators.maxLength(25), Validators.minLength(25)],
      ],
      birthDate: [''],
      address: [this.currentPerson.address],
      zipCode: [
        this.currentPerson.zipCode,
        [Validators.maxLength(8), Validators.minLength(8)],
      ],
      phoneNumber: [
        this.currentPerson.phoneNumber,
        [Validators.maxLength(9), Validators.minLength(9)],
      ],
      email: [
        this.currentPerson.email,
        [Validators.email, Validators.maxLength(50), Validators.minLength(5)],
      ],
      naturality: [
        this.currentPerson.nationality,
        [Validators.maxLength(100), Validators.minLength(3)],
      ],
      nationality: [
        this.currentPerson.nationality,
        [Validators.maxLength(100), Validators.minLength(3)],
      ],
      gender: [this.currentPerson.gender],
      habilitation: [this.currentPerson.habilitation],
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

  onSubmit() {
    this.submitted = true;

    if (this.updatePersonForm.invalid) {
      this.updatePersonForm.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    const formValue = this.updatePersonForm.value;

    this.loading = true;

    this.peopleService
      .updatePerson(this.id, {
        id: this.id,
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        nif: formValue.nif,
        niss: formValue.niss,
        address: formValue.address,
        birthDate: formValue.birthDate,
        email: formValue.email,
        gender: formValue.gender,
        habilitation: formValue.habilitation,
        iban: formValue.iban,
        identificationNumber: formValue.identificationNumber,
        identificationType: formValue.identificationType,
        identificationValidationDate: formValue.identificationValidationDate,
        nationality: formValue.nationality,
        naturality: formValue.naturality,
        phoneNumber: formValue.phoneNumber,
        zipCode: formValue.zipCode,
      })
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
