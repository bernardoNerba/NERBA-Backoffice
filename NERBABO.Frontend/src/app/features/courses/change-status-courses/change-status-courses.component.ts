import { Component, OnInit } from '@angular/core';
import { Course } from '../../../core/models/course';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { SelectModule } from 'primeng/select';
import { STATUS } from '../../../core/objects/status';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { SharedService } from '../../../core/services/shared.service';
import { Subject } from 'rxjs';
import { CoursesService } from '../../../core/services/courses.service';

@Component({
  selector: 'app-change-status-courses',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    SelectModule,
    ErrorCardComponent,
  ],
  templateUrl: './change-status-courses.component.html',
  styleUrl: './change-status-courses.component.css',
})
export class ChangeStatusCoursesComponent implements OnInit {
  course!: Course;
  loading: boolean = false;
  submitted: boolean = false;
  form: FormGroup = new FormGroup({});
  errorMessages: string[] = [];
  STATUS = STATUS;

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private sharedService: SharedService,
    private coursesService: CoursesService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.form.patchValue({
      status: this.course.status,
    });
  }

  onSubmit() {
    this.submitted = true;
    this.errorMessages = [];

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    this.loading = true;

    this.coursesService
      .changeStatus(this.course.id, this.form.value.status)
      .subscribe({
        next: (value) => {
          this.bsModalRef.hide();
          this.coursesService.triggerFetchCourses();
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
      status: ['', Validators.required],
    });
  }
}
