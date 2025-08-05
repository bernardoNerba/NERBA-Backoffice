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
import { SharedService } from '../../../core/services/shared.service';
import { Observable, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { SelectModule } from 'primeng/select';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { ModuleTeachingService } from '../../../core/services/module-teaching.service';
import { TeachersService } from '../../../core/services/teachers.service';
import { ModulesService } from '../../../core/services/modules.service';
import { PeopleService } from '../../../core/services/people.service';
import { Teacher } from '../../../core/models/teacher';
import { Module } from '../../../core/models/module';
import { Person } from '../../../core/models/person';
import {
  CreateModuleTeaching,
  UpdateModuleTeaching,
} from '../../../core/models/moduleTeaching';

interface TeacherWithName extends Teacher {
  fullName: string;
}

@Component({
  selector: 'app-upsert-module-teaching',
  templateUrl: './upsert-module-teaching.component.html',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    ErrorCardComponent,
    SelectModule,
    CommonModule,
  ],
})
export class UpsertModuleTeachingComponent implements IUpsert, OnInit {
  @Input({ required: true }) actionId!: number;
  @Input() actionTitle?: string;
  @Input({ required: true }) id!: number; // For update operations

  teachersWithNames$!: Observable<TeacherWithName[]>;
  modulesWithoutTeacher$!: Observable<Module[]>;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private moduleTeachingService: ModuleTeachingService,
    private teachersService: TeachersService,
    private modulesService: ModulesService,
    private peopleService: PeopleService,
    private formBuilder: FormBuilder,
    private sharedService: SharedService
  ) {}
  patchFormValues(): void {
    throw new Error('Method not implemented.');
  }

  ngOnInit(): void {
    this.initializeForm();
    this.initializeData();

    if (this.id && this.id !== 0) {
      this.isUpdate = true;
      this.loadModuleTeaching();
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      teacherId: [null, [Validators.required]],
      moduleId: [null, [Validators.required]],
      actionId: [this.actionId, [Validators.required]],
    });
  }

  initializeData(): void {
    // Get teachers with their person names
    this.teachersWithNames$ = combineLatest([
      this.teachersService.getAllTeachers(),
      this.peopleService.people$,
    ]).pipe(
      map(([teachers, people]) =>
        teachers.map((teacher) => {
          const person = people.find((p) => p.id === teacher.personId);
          return {
            ...teacher,
            fullName: person?.fullName || 'Nome não encontrado',
          };
        })
      )
    );

    // Get modules without teachers for this action
    this.modulesWithoutTeacher$ =
      this.modulesService.getModulesWithoutTeacherByAction(this.actionId);
  }

  loadModuleTeaching(): void {
    if (this.id) {
      this.moduleTeachingService.getModuleTeachingById(this.id).subscribe({
        next: (moduleTeaching) => {
          this.form.patchValue({
            teacherId: moduleTeaching.teacherId,
            moduleId: moduleTeaching.moduleId,
            actionId: moduleTeaching.actionId,
          });
        },
        error: (error) => {
          this.sharedService.showError('Associação não encontrada.');
          this.bsModalRef.hide();
        },
      });
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessages = [];

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Por favor, preencha todos os campos obrigatórios.'
      );
      return;
    }

    this.loading = true;

    const formData = {
      ...this.form.value,
      id: this.isUpdate ? this.id : undefined,
    };

    this.moduleTeachingService.upsert(formData, this.isUpdate).subscribe({
      next: (response) => {
        this.bsModalRef.hide();
        this.sharedService.showSuccess(response.message);
      },
      error: (error) => {
        this.errorMessages = this.sharedService.handleErrorResponse(error);
        this.loading = false;
      },
    });
  }

  getTeacherDisplayName(teacher: TeacherWithName): string {
    return `${teacher.fullName}`;
  }

  getModuleDisplayName(module: Module): string {
    return `${module.name} (${module.hours}h)`;
  }
}
