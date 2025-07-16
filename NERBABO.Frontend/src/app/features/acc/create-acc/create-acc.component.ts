import { Component, OnInit, TemplateRef } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { CommonModule } from '@angular/common';
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';
import { PeopleService } from '../../../core/services/people.service';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { Person } from '../../../core/models/person';
import { AccService } from '../../../core/services/acc.service';
import { SharedService } from '../../../core/services/shared.service';
import { PasswordValidators } from 'ngx-validators';
import { OkResponse } from '../../../core/models/okResponse';
import {
  AutoCompleteCompleteEvent,
  AutoCompleteModule,
} from 'primeng/autocomplete';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-create-acc',
  imports: [
    CommonModule,
    TypeaheadModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    ErrorCardComponent,
    AutoCompleteModule,
    InputTextModule,
    PasswordModule,
    DividerModule,
  ],
  templateUrl: './create-acc.component.html',
  styles: `
  :host ::ng-deep .p-password {
  width: 100% !important;
  max-width: 100% !important;
  }
  :host ::ng-deep .p-password input {
    width: 100% !important;
  }
  `,
})
export class CreateAccComponent implements OnInit {
  registrationForm: FormGroup = new FormGroup({});
  people!: Person[];
  displayPeople!: Person[];
  submitted: boolean = false;
  errorMessages: string[] = [];
  loading = false;

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private accountService: AccService,
    private sharedService: SharedService,
    private peopleService: PeopleService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.loadPeople();
  }

  private initializeForm() {
    this.registrationForm = this.formBuilder.group(
      {
        email: [
          '',
          [
            Validators.required,
            Validators.maxLength(50),
            Validators.minLength(8),
            Validators.email,
          ],
        ],
        userName: [
          '',
          [
            Validators.required,
            Validators.minLength(3),
            Validators.maxLength(30),
          ],
        ],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            Validators.maxLength(30),
          ],
        ],
        confirmPassword: ['', [Validators.required]],
        personId: ['', Validators.required],
      },
      {
        validators: PasswordValidators.mismatchedPasswords(
          'password',
          'confirmPassword'
        ),
      }
    );
  }

  filterPeople(event: AutoCompleteCompleteEvent) {
    let filtered: any[] = [];
    let query = event.query;

    for (let i = 0; i < (this.displayPeople as any[]).length; i++) {
      let person = (this.displayPeople as any[])[i];
      if (person.displayName.toLowerCase().indexOf(query.toLowerCase()) == 0) {
        filtered.push(person);
      }
    }

    this.displayPeople = filtered;
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessages = [];

    if (this.registrationForm.invalid) {
      this.registrationForm.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    this.registrationForm.value.personId =
      this.registrationForm.value.personId.id;

    this.loading = true;

    this.accountService.register(this.registrationForm.value).subscribe({
      next: (value: OkResponse) => {
        // hide modal
        this.bsModalRef.hide();
        // refresh user data
        this.accountService.triggerFetchUsers();
        // display success message.
        this.sharedService.showSuccess(value.title);
      },
      error: (error) => {
        this.errorMessages = this.sharedService.handleErrorResponse(error);
        this.loading = false;
      },
    });
  }

  private loadPeople() {
    // load people for multiple type ahead choices
    this.peopleService.peopleWithoutUser$.subscribe({
      next: (data) => {
        if (data) {
          this.people = data;
          // Prepare data specifically for typeahead
          this.displayPeople = data.map((person) => ({
            ...person,
            displayName: `${person.fullName} - ${person.nif}`,
            id: person.id,
          }));
        }
      },
      error: (err) => console.log(err),
    });
  }
}
