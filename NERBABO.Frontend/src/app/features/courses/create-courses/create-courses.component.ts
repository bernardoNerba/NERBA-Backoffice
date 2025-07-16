import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { CoursesService } from '../../../core/services/courses.service';
import { SharedService } from '../../../core/services/shared.service';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { CommonModule } from '@angular/common';
import { STATUS } from '../../../core/objects/status';
import { HABILITATIONS } from '../../../core/objects/habilitations';
import { DESTINATORS } from '../../../core/objects/destinators';
import { FrameService } from '../../../core/services/frame.service';
import { map, Observable } from 'rxjs';
import { Frame } from '../../../core/models/frame';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { ModulesService } from '../../../core/services/modules.service';
import { Module } from '../../../core/models/module';

@Component({
  selector: 'app-create-courses',
  imports: [
    ErrorCardComponent,
    CommonModule,
    ReactiveFormsModule,
    MultiSelectModule,
    SelectModule,
  ],
  templateUrl: './create-courses.component.html',
})
export class CreateCoursesComponent implements OnInit {
  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});
  submitted: boolean = false;
  loading: boolean = false;
  frames$!: Observable<Frame[]>;
  modules$!: Observable<Module[]>;

  STATUS = STATUS;
  HABILITATIONS = HABILITATIONS;
  DESTINATORS = DESTINATORS;

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
    this.frames$ = this.frameService.frames$;
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

  private initializeForm() {
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

  onSubmit() {
    this.submitted = true;
    this.errorMessages = [];

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não esão de acordo com as diretrizes.'
      );
      return;
    }

    console.log(this.form.value);

    this.loading = true;

    this.coursesService.create(this.form.value).subscribe({
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
