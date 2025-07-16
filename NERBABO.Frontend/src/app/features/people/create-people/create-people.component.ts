import { Component, OnInit } from '@angular/core';
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
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';

@Component({
  selector: 'app-create-people',
  imports: [ReactiveFormsModule, ErrorCardComponent, TypeaheadModule],
  templateUrl: './create-people.component.html',
})
export class CreatePeopleComponent implements OnInit {
  allCountries = [...COUNTRIES];
  allHabilitations = [...HABILITATIONS];
  allIdentificationTypes = [...IDENTIFICATION_TYPES];
  submitted = false;
  loading = false;
  errorMessages: string[] = [];
  registPersonForm: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private peopleService: PeopleService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm() {
    this.registPersonForm = this.formBuilder.group({
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

  onSubmit() {
    this.submitted = true;
    this.errorMessages = [];

    if (this.registPersonForm.invalid) {
      this.registPersonForm.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    this.loading = true;

    this.peopleService.createPerson(this.registPersonForm.value).subscribe({
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
