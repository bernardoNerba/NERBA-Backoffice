import { Component, Input, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Observable } from 'rxjs';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { CommonModule } from '@angular/common';
import { CheckboxModule } from 'primeng/checkbox';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { IUpsert } from '../../../core/interfaces/IUpsert';
import { Teacher } from '../../../core/models/teacher';
import { Person } from '../../../core/models/person';
import { Tax } from '../../../core/models/tax';
import { ConfigService } from '../../../core/services/config.service';
import { PeopleService } from '../../../core/services/people.service';
import { TeachersService } from '../../../core/services/teachers.service';
import { SharedService } from '../../../core/services/shared.service';
import { ToggleSwitchModule } from 'primeng/toggleswitch';

@Component({
  selector: 'app-upsert-teachers',
  templateUrl: './upsert-teachers.component.html',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ErrorCardComponent,
    CheckboxModule,
    DropdownModule,
    InputTextModule,
    ToggleSwitchModule,
  ],
})
export class UpsertTeachersComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: number;
  @Input({ required: true }) personId!: number | null;
  currentTeacher?: Teacher;
  currentPerson?: Person;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  ivas$!: Observable<Tax[]>;
  irss$!: Observable<Tax[]>;
  people$!: Observable<Person[] | null>;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private configService: ConfigService,
    private peopleService: PeopleService,
    private formBuilder: FormBuilder,
    private teachersService: TeachersService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.initializeTaxes();

    if (this.id !== 0) {
      // update
      this.isUpdate = true;
      this.teachersService.getTeacherById(this.id).subscribe({
        next: (teacher: Teacher) => {
          this.currentTeacher = teacher;
          this.patchFormValues();
          this.loadPerson(teacher.personId);
        },
        error: (error: any) => {
          this.sharedService.showError('Informação não foi encontrada.');
          this.bsModalRef.hide();
        },
      });
    } else if (this.personId !== null) {
      // create with personId
      this.loadPerson(this.personId);
      this.form.patchValue({ personId: this.personId });
    } else {
      // create without personId
      this.initializePeople();
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      ivaRegimeId: ['', [Validators.required]],
      irsRegimeId: ['', [Validators.required]],
      personId: ['', [Validators.required]],
      ccp: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(55),
        ],
      ],
      competences: ['', [Validators.minLength(3), Validators.maxLength(55)]],
      isLecturingFM: [false],
      isLecturingCQ: [false],
    });
  }

  initializeTaxes(): void {
    this.ivas$ = this.configService.fetchTaxesByType('IVA');
    this.irss$ = this.configService.fetchTaxesByType('IRS');
  }

  initializePeople(): void {
    this.people$ = this.peopleService.people$;
  }

  loadPerson(personId: number): void {
    this.currentPerson = this.peopleService.personById(personId);
  }

  patchFormValues(): void {
    this.form.patchValue({
      ivaRegimeId: this.currentTeacher?.ivaRegimeId,
      irsRegimeId: this.currentTeacher?.irsRegimeId,
      personId: this.currentTeacher?.personId,
      ccp: this.currentTeacher?.ccp,
      competences: this.currentTeacher?.competences,
      isLecturingFM: this.currentTeacher?.isLecturingFM,
      isLecturingCQ: this.currentTeacher?.isLecturingCQ,
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

    console.log(this.form.value);

    this.loading = true;
    this.teachersService
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
