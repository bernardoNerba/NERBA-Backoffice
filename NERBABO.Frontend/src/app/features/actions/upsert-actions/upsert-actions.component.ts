import { Component, Input, OnInit } from '@angular/core';
import { IUpsert } from '../../../core/interfaces/IUpsert';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ActionsService } from '../../../core/services/actions.service';
import { SharedService } from '../../../core/services/shared.service';
import { CoursesService } from '../../../core/services/courses.service';
import { Course } from '../../../core/models/course';
import { Observable } from 'rxjs';
import { WEEKDAYS } from '../../../core/objects/weekDays';
import { STATUS, StatusEnum } from '../../../core/objects/status';
import {
  REGIMENTS,
  RegimentTypeEnum,
} from '../../../core/objects/regimentTypes';
import { Action } from '../../../core/models/action';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-upsert-actions',
  templateUrl: './upsert-actions.component.html',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    ErrorCardComponent,
    MultiSelectModule,
    SelectModule,
    DatePickerModule,
    CommonModule,
  ],
})
export class UpsertActionsComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: number;
  @Input() courseId?: number | null;
  @Input() courseTitle?: string | null;
  courses$!: Observable<Course[]>;
  currentAction?: Action | null;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  WEEKDAYS = WEEKDAYS;
  STATUS = STATUS;
  REGIMENTS = REGIMENTS;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private actionsService: ActionsService,
    private formBuilder: FormBuilder,
    private sharedService: SharedService,
    private courseService: CoursesService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.initializeCourses();

    if (this.id !== 0) {
      this.isUpdate = true;
      this.actionsService.getActionById(this.id).subscribe({
        next: (action: Action) => {
          this.currentAction = action;
          console.log(this.currentAction);
          this.patchFormValues();
        },
        error: (error: any) => {
          this.sharedService.showError('Ação Formação não encontrada.');
          this.bsModalRef.hide();
        },
      });
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      courseId: [this.courseId || null, [Validators.required]],
      rangeDates: [null, [Validators.required]],
      administrationCode: [
        '',
        [
          Validators.required,
          Validators.minLength(5),
          Validators.maxLength(10),
          Validators.pattern(/^\d+$/),
        ],
      ],
      address: [''],
      locality: ['', [Validators.required]],
      weekDays: [[]],
      regiment: [RegimentTypeEnum.Presential],
      status: [StatusEnum.NotStarted],
    });
  }

  initializeCourses(): void {
    if (this.courseId === null || this.courseId === undefined) {
      this.courses$ = this.courseService.getActive();
    } else {
      this.form.patchValue({ courseId: this.courseId });
    }
  }

  patchFormValues(): void {
    if (this.currentAction) {
      this.form.patchValue({
        courseId: this.currentAction.courseId || this.courseId,
        rangeDates: [
          this.currentAction.startDate
            ? new Date(this.currentAction.startDate)
            : null,
          this.currentAction.endDate
            ? new Date(this.currentAction.endDate)
            : null,
        ],
        administrationCode: this.currentAction.administrationCode,
        address: this.currentAction.address || '',
        locality: this.currentAction.locality,
        weekDays: this.currentAction.weekDays || [],
        regiment: this.currentAction.regiment || RegimentTypeEnum.Presential,
        status: this.currentAction.status || StatusEnum.NotStarted,
      });
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessages = [];

    const rangeDates = this.form.get('rangeDates')?.value;

    // Validate date range - check for both dates being present
    if (!rangeDates || !rangeDates[0] || !rangeDates[1]) {
      this.sharedService.showError(
        'Por favor, selecione uma data de início e fim.'
      );
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    // Prepare form data for submission
    const formData = {
      ...this.form.value,
      id: this.isUpdate ? this.id : undefined, // Include ID for updates
      startDate: rangeDates[0].toISOString().split('T')[0],
      endDate: rangeDates[1].toISOString().split('T')[0],
    };

    // Remove rangeDates from the final object since backend doesn't expect it
    delete formData.rangeDates;

    this.loading = true;

    this.actionsService.upsert(formData, this.isUpdate).subscribe({
      next: (value) => {
        if (this.form.value.id === 0) {
          this.courseService.notifyModifiedAction(this.form.value.courseId);
        }
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
