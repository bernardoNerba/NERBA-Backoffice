import { Component, Input, OnInit } from '@angular/core';
import { IUpsert } from '../../../core/interfaces/IUpsert';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Person } from '../../../core/models/person';
import { Student } from '../../../core/models/student';
import { Observable } from 'rxjs';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { PeopleService } from '../../../core/services/people.service';
import { StudentsService } from '../../../core/services/students.service';
import { SharedService } from '../../../core/services/shared.service';
import { Company } from '../../../core/models/company';
import { CompaniesService } from '../../../core/services/companies.service';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { ToggleSwitch, ToggleSwitchModule } from 'primeng/toggleswitch';
import { DropdownModule } from 'primeng/dropdown';
import { CommonModule } from '@angular/common';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-upsert-students',
  imports: [
    ErrorCardComponent,
    ToggleSwitchModule,
    DropdownModule,
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
  ],
  templateUrl: './upsert-students.component.html',
})
export class UpsertStudentsComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: number;
  personId?: number | null;
  currentPerson?: Person | null;
  currentStudent?: Student | null;
  companyAssociated?: Company | null;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  people$!: Observable<Person[]>;
  companies$!: Observable<Company[]>;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private peopleService: PeopleService,
    private companiesService: CompaniesService,
    private formBuilder: FormBuilder,
    private studentsService: StudentsService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.initializeCompanies();

    if (this.id !== 0) {
      this.isUpdate = true;
      this.studentsService.getById(this.id).subscribe({
        next: (student: Student) => {
          this.currentStudent = student;
          this.patchFormValues();
          this.loadPerson(student.personId);
        },
        error: (error: any) => {
          this.sharedService.showError('Informação não foi encontrada.');
          this.bsModalRef.hide();
        },
      });
    } else if (this.personId !== null) {
      // create already associated with a person
      this.loadPerson(this.personId!);
      this.form.patchValue({ personId: this.personId });
    } else {
      // create without any info
      this.initializePeople();
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      personId: ['', [Validators.required]],
      companyId: [''],
      isEmployeed: [false],
      isRegisteredWithJobCenter: [false],
      companyRole: [''],
    });
  }

  initializeCompanies(): void {
    this.companies$ = this.companiesService.companies$;
  }

  initializePeople(): void {
    this.people$ = this.peopleService.people$;
  }

  loadPerson(personId: number): void {
    this.currentPerson = this.peopleService.personById(personId);
  }

  patchFormValues(): void {
    this.form.patchValue({
      personId: this.currentStudent?.personId,
      companyId: this.currentStudent?.companyId,
      isEmployeed: this.currentStudent?.isEmployeed,
      isRegisteredWithJobCenter: this.currentStudent?.isRegisteredWithJobCenter,
      companyRole: this.currentStudent?.companyRole,
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

    if (!this.form.value.companyId) this.form.value.companyId = null;
    console.log(this.form.value);

    this.loading = true;
    this.studentsService
      .upsert({ id: this.id, ...this.form.value }, this.isUpdate)
      .subscribe({
        next: (value) => {
          this.bsModalRef.hide();
          this.sharedService.showSuccess(value.message);
        },
        error: (error) => {
          this.errorMessages = this.sharedService.handleErrorResponse(error);
          this.loading = false;
        },
      });
  }
}
