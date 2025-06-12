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

@Component({
  selector: 'app-create-acc',
  imports: [
    CommonModule,
    TypeaheadModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    ErrorCardComponent,
  ],
  templateUrl: './create-acc.component.html',
  styleUrl: './create-acc.component.css',
})
export class CreateAccComponent implements OnInit {
  modalRef?: BsModalRef;

  registrationForm: FormGroup = new FormGroup({});
  people!: Person[];
  submitted: boolean = false;
  errorMessages: string[] = [];
  filteredPeople: Person[] = [];
  selectedPersonDisplay: string = '';
  peopleForTypeahead: any[] = [];
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
        validators: PasswordValidators.mismatchedPasswords(),
      }
    );
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
          this.peopleForTypeahead = data.map((person) => ({
            ...person,
            displayName: `${person.fullName} - ${person.nif}`,
            id: person.id,
          }));
        }
      },
      error: (err) => console.log(err),
    });
  }

  onPersonSelected(event: any): void {
    // event.item contains the selected person object
    const selectedPerson = event.item;
    this.registrationForm.get('personId')?.setValue(selectedPerson.id);
    this.selectedPersonDisplay = selectedPerson.displayName;
  }
}
