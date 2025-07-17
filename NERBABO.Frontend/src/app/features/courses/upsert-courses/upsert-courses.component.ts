import { Component, Input, OnInit } from '@angular/core';
import { IUpsert } from '../../../core/interfaces/IUpsert';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { STATUS } from '../../../core/objects/status';
import { HABILITATIONS } from '../../../core/objects/habilitations';
import { DESTINATORS } from '../../../core/objects/destinators';
import { Module } from '../../../core/models/module';
import { Frame } from '../../../core/models/frame';
import { map, Observable } from 'rxjs';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { CoursesService } from '../../../core/services/courses.service';
import { SharedService } from '../../../core/services/shared.service';
import { FrameService } from '../../../core/services/frame.service';
import { ModulesService } from '../../../core/services/modules.service';
import { Course } from '../../../core/models/course';
import { SelectModule } from 'primeng/select';
import { CommonModule } from '@angular/common';
import { MultiSelectModule } from 'primeng/multiselect';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { TextareaModule } from 'primeng/textarea';

@Component({
  selector: 'app-upsert-courses',
  imports: [
    ReactiveFormsModule,
    SelectModule,
    CommonModule,
    MultiSelectModule,
    InputTextModule,
    InputNumberModule,
    ErrorCardComponent,
    TextareaModule,
  ],
  templateUrl: './upsert-courses.component.html',
})
export class UpsertCoursesComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: number;
  currentCourse?: Course | null;
  frames$!: Observable<Frame[]>;
  modules$!: Observable<Module[]>;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  STATUS = STATUS;
  HABILITATIONS = HABILITATIONS;
  DESTINATORS = DESTINATORS;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private coursesService: CoursesService,
    private sharedService: SharedService,
    private frameService: FrameService,
    private modulesService: ModulesService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.initializeFrame();
    this.initializeModules();

    if (this.id !== 0) {
      this.isUpdate = true;
      this.coursesService.getSingleCourse(this.id).subscribe({
        next: (course: Course) => {
          this.currentCourse = course;
          this.patchFormValues();
        },
        error: (error: any) => {
          this.sharedService.showError('Curso não encontrado.');
          this.bsModalRef.hide();
        },
      });
    }
  }

  private initializeFrame(): void {
    this.frames$ = this.frameService.frames$;
  }

  private initializeModules(): void {
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

  initializeForm(): void {
    this.form = this.formBuilder.group({
      frameId: ['', [Validators.required]],
      title: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(255),
        ],
      ],
      objectives: ['', [Validators.minLength(3), Validators.maxLength(510)]],
      destinators: [[]],
      modules: [[]],
      area: ['', [Validators.minLength(3), Validators.maxLength(55)]],
      totalDuration: [
        '',
        [Validators.required, Validators.min(0), Validators.max(1000)],
      ],
      status: ['Não Iniciado'],
      minHabilitationLevel: ['Sem Comprovativo'],
    });
  }

  patchFormValues(): void {
    this.form.patchValue({
      id: this.currentCourse?.id,
      frameId: this.currentCourse?.frameId,
      title: this.currentCourse?.title,
      objectives: this.currentCourse?.objectives,
      destinators: this.currentCourse?.destinators,
      modules: this.currentCourse?.modules.map((module) => module.id),
      area: this.currentCourse?.area,
      totalDuration: this.currentCourse?.totalDuration,
      status: this.currentCourse?.status,
      minHabilitationLevel: this.currentCourse?.minHabilitationLevel,
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessages = [];

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não esão de acordo com as diretrizes.'
      );
      return;
    }

    this.loading = true;

    this.coursesService
      .upsert({ id: this.id, ...this.form.value }, this.isUpdate)
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
