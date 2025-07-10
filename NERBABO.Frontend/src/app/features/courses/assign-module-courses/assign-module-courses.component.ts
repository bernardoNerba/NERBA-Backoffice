import { Component, OnInit } from '@angular/core';
import { Course } from '../../../core/models/course';
import { FormGroup, FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { map, Observable } from 'rxjs';
import { Module } from '../../../core/models/module';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { CoursesService } from '../../../core/services/courses.service';
import { MultiSelectModule } from 'primeng/multiselect';
import { CommonModule } from '@angular/common';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { ModulesService } from '../../../core/services/modules.service';

@Component({
  selector: 'app-assign-module-courses',
  imports: [
    ErrorCardComponent,
    CommonModule,
    ReactiveFormsModule,
    MultiSelectModule,
  ],
  templateUrl: './assign-module-courses.component.html',
  styleUrl: './assign-module-courses.component.css',
})
export class AssignModuleCoursesComponent implements OnInit {
  course!: Course;
  loading: boolean = false;
  submitted: boolean = false;
  form: FormGroup = new FormGroup({});
  errorMessages: string[] = [];
  modules$!: Observable<Module[]>;

  constructor(
    public bsModalRef: BsModalRef,
    private FormBuilder: FormBuilder,
    private sharedService: SharedService,
    private coursesService: CoursesService,
    private modulesService: ModulesService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.form.patchValue({
      modules: this.course.modules.map((module) => module.id),
    });

    this.modules$ = this.modulesService.modules$.pipe(
      map((modules) =>
        modules.map((module) => ({
          ...module,
          displayName: `${module.name} - ${
            module.isActive ? 'Ativo' : 'Inativo'
          }`,
        }))
      )
    );
  }

  private initializeForm(): void {
    this.form = this.FormBuilder.group({
      modules: [[]],
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

    this.loading = true;
    console.log(this.form.value.modules);
    this.coursesService
      .assignModules(this.form.value.modules, this.course.id)
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
}
