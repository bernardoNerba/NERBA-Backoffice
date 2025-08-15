import { Component, Input, OnInit } from "@angular/core";
import { IUpsert } from "../../../core/interfaces/IUpsert";
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { BsModalRef } from "ngx-bootstrap/modal";
import { AccService } from "../../../core/services/acc.service";
import { SharedService } from "../../../core/services/shared.service";
import { UserInfo } from "../../../core/models/userInfo";
import { PasswordValidators } from "ngx-validators";
import { OkResponse } from "../../../core/models/okResponse";
import { ErrorCardComponent } from "../../../shared/components/error-card/error-card.component";
import { DividerModule } from "primeng/divider";
import { PasswordModule } from "primeng/password";
import {
  AutoCompleteCompleteEvent,
  AutoCompleteModule,
} from "primeng/autocomplete";
import { InputTextModule } from "primeng/inputtext";
import { PeopleService } from "../../../core/services/people.service";
import { Person } from "../../../core/models/person";

@Component({
  selector: "app-upsert-acc",
  imports: [
    ReactiveFormsModule,
    ErrorCardComponent,
    DividerModule,
    PasswordModule,
    AutoCompleteModule,
    InputTextModule,
  ],
  templateUrl: "./upsert-acc.component.html",
  styleUrl: "./upsert-acc.component.css",
})
export class UpsertAccComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: string | number;
  currentUser?: UserInfo | null;
  currentPerson?: Person | null;
  people: Person[] = [];
  displayPeople: Person[] = [];
  personId?: number | null;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private userService: AccService,
    private formBuilder: FormBuilder,
    private sharedService: SharedService,
    private peopleService: PeopleService,
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    if (this.id !== 0) {
      this.isUpdate = true;
      this.userService.getUserById(this.id.toString()).subscribe({
        next: (user) => {
          this.currentUser = user;
          this.patchFormValues();
        },
        error: (error) => {
          this.sharedService.showError("Utilizador não encontrado.");
          this.bsModalRef.hide();
        },
      });
      this.patchFormValues();
    } else {
      // create
      if (this.personId !== null) {
        this.peopleService.getSinglePerson(this.personId!).subscribe({
          next: (person) => {
            this.currentPerson = person;
            this.form.patchValue({
              personId: {
                ...person,
                displayName: `${person.fullName} - ${person.nif}`,
                id: person.id,
              },
            });
          },
        });
      } else {
        this.loadPeople();
      }
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group(
      {
        email: [
          "",
          [
            Validators.required,
            Validators.maxLength(50),
            Validators.minLength(8),
            Validators.email,
          ],
        ],
        userName: [
          "",
          [
            Validators.required,
            Validators.minLength(3),
            Validators.maxLength(30),
          ],
        ],
        password: [
          "",
          [
            Validators.required,
            Validators.minLength(8),
            Validators.maxLength(30),
          ],
        ],
        confirmPassword: ["", [Validators.required]],
        personId: [""], // verify required on submit if register
      },
      {
        validators: PasswordValidators.mismatchedPasswords(
          "password",
          "confirmPassword",
        ),
      },
    );
  }

  patchFormValues(): void {
    this.form.patchValue({
      email: this.currentUser?.email,
      userName: this.currentUser?.userName,
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessages = [];

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        "Os dados fornecidos não estão de acordo com as diretrizes.",
      );
      return;
    }

    let obj: any = {
      id: this.id,
      userName: this.form.value.userName,
      email: this.form.value.email,
      newPassword: this.form.value.password,
    };

    if (!this.isUpdate) {
      // verify person id included
      if (!this.form.value.personId.id) {
        this.sharedService.showError("Pessoa é um campo obrigatório!");
        return;
      }

      obj = {
        email: this.form.value.email,
        userName: this.form.value.userName,
        password: this.form.value.password,
        personId: this.form.value.personId.id,
      };
    }

    console.log(obj);

    this.loading = true;
    this.userService.upsert(obj, this.isUpdate).subscribe({
      next: (response: OkResponse) => {
        console.log(response);
        this.sharedService.showSuccess(response.title);
        this.bsModalRef.hide();
        this.userService.triggerFetchUsers();
      },
      error: (err) => {
        this.sharedService.handleErrorResponse(err);
        this.loading = false;
      },
    });
  }

  private loadPeople() {
    // load people for multiple type ahead choices
    this.peopleService.fetchPeopleWithoutUser().subscribe({
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
}
