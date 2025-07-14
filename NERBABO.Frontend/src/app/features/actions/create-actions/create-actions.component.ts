import { Component, Input, OnInit } from '@angular/core';
import { ActionsService } from '../../../core/services/actions.service';
import {
  FormGroup,
  FormBuilder,
  Validators,
  ReactiveFormsModule,
  FormsModule,
} from '@angular/forms';
import {
  REGIMENTS,
  RegimentTypeEnum,
} from '../../../core/objects/regimentTypes';
import { STATUS, StatusEnum } from '../../../core/objects/status';
import { WEEKDAYS } from '../../../core/objects/weekDays';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { SharedService } from '../../../core/services/shared.service';
import { CoursesService } from '../../../core/services/courses.service';
import { Course } from '../../../core/models/course';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-create-actions',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    ErrorCardComponent,
    MultiSelectModule,
    SelectModule,
    DatePickerModule,
    CommonModule,
  ],
  templateUrl: './create-actions.component.html',
  styleUrl: './create-actions.component.css',
})
export class CreateActionsComponent implements OnInit {
  @Input() courseId?: number | null;
  @Input() courseTitle?: string | null;
  errorMessages: string[] = [];
  submitted: boolean = false;
  loading: boolean = false;
  form: FormGroup = new FormGroup({});
  courses$!: Observable<Course[]>;

  // ❌ REMOVE the separate rangeDates property
  // rangeDates: Date[] = [];

  WEEKDAYS = WEEKDAYS;
  STATUS = STATUS;
  REGIMENTS = REGIMENTS;

  constructor(
    public bsModalRef: BsModalRef,
    private actionsService: ActionsService,
    private formBuilder: FormBuilder,
    private sharedService: SharedService,
    private courseService: CoursesService
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    if (this.courseId === null) {
      this.courses$ = this.courseService.getActive();
    } else {
      this.form.patchValue({
        courseId: this.courseId,
      });
    }
  }

  onSubmit() {
    this.submitted = true;
    this.errorMessages = [];

    // ✅ Get the date range from the form
    const rangeDates = this.form.get('rangeDates')?.value;

    console.log(rangeDates);

    // Validate date range - check for both dates being present
    // The second date in the range can be null if the user has only clicked once.
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

    // Update form with separated dates
    const formData = {
      ...this.form.value,
      startDate: rangeDates[0].toISOString().split('T')[0],
      endDate: rangeDates[1].toISOString().split('T')[0],
    };

    // Remove rangeDates from the final object if your backend doesn't expect it
    delete formData.rangeDates;

    console.log('Form data:', formData);

    this.loading = true;

    this.actionsService.create(formData).subscribe({
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

  private initializeForm() {
    this.form = this.formBuilder.group({
      courseId: [null, [Validators.required]],
      rangeDates: [null, [Validators.required]],
      title: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(255),
        ],
      ],
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
}
