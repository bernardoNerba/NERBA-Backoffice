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
import { ActionsService } from '../../../core/services/actions.service';
import { Teacher } from '../../../core/models/teacher';
import { Module } from '../../../core/models/module';
import { Person } from '../../../core/models/person';
import { Action } from '../../../core/models/action';
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
export class UpsertModuleTeachingComponent implements OnInit {
  @Input({ required: true }) actionId!: number;
  @Input() actionTitle?: string;
  @Input({ required: true }) id!: number; // For update operations
  @Input() moduleId?: number; // For pre-selecting module in update context
  @Input() moduleName?: string; // For display purposes
  @Input() teacherId?: number | null; // For pre-selecting teacher in update context
  @Input() isUpdate: boolean = false; // Flag to indicate update mode

  teachersWithNames$!: Observable<TeacherWithName[]>;
  modulesWithoutTeacher$!: Observable<Module[]>;
  actions$!: Observable<Action[]>;

  submitted: boolean = false;
  loading: boolean = false;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private moduleTeachingService: ModuleTeachingService,
    private teachersService: TeachersService,
    private modulesService: ModulesService,
    private peopleService: PeopleService,
    private actionsService: ActionsService,
    private formBuilder: FormBuilder,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.initializeData();

    // Handle traditional update mode (with existing ModuleTeaching ID)
    if (this.id && this.id !== 0 && !this.isUpdate) {
      this.isUpdate = true;
      this.loadModuleTeaching();
    }
    // Handle new update mode (from modules table)
    else if (this.isUpdate && this.moduleId) {
      this.loadModuleTeachingFromActionAndModule();
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      teacherId: [null, [Validators.required]],
      moduleId: [null, [Validators.required]],
      actionId: [this.actionId || null, [Validators.required]],
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

    // Load actions if no specific actionId is provided
    if (!this.actionId) {
      this.actions$ = this.actionsService.getActiveActions();
    }

    // For module table updates, we need all modules with teacher info to show the current module
    if (this.isUpdate && this.moduleId && this.actionId) {
      this.modulesWithoutTeacher$ = this.modulesService
        .getModulesWithTeacherByAction(this.actionId)
        .pipe(map((modules) => modules.filter((m) => m.id === this.moduleId)));
    } else if (this.actionId) {
      // Get modules without teachers for this action (normal create flow)
      this.modulesWithoutTeacher$ =
        this.modulesService.getModulesWithoutTeacherByAction(this.actionId);
    }
    // If no actionId, we'll load modules when an action is selected
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

  loadModuleTeachingFromActionAndModule(): void {
    if (this.actionId && this.moduleId) {
      this.moduleTeachingService
        .getModuleTeachingByActionAndModule(this.actionId, this.moduleId)
        .subscribe({
          next: (moduleTeaching) => {
            // Set the ID for the update
            this.id = moduleTeaching.id;
            this.form.patchValue({
              teacherId: moduleTeaching.teacherId,
              moduleId: moduleTeaching.moduleId,
              actionId: moduleTeaching.actionId,
            });
          },
          error: (error) => {
            console.error('ModuleTeaching association not found:', error);
            // If no association exists, we can still pre-fill what we know
            this.form.patchValue({
              moduleId: this.moduleId,
              teacherId: this.teacherId || null,
              actionId: this.actionId,
            });
            // Set isUpdate to false since no association exists yet
            this.isUpdate = false;
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

  getActionDisplayName(action: Action): string {
    return `${action.actionNumber} - ${action.title}`;
  }

  onActionChange(actionId: number): void {
    if (actionId) {
      // Load modules without teachers for the selected action
      this.modulesWithoutTeacher$ =
        this.modulesService.getModulesWithoutTeacherByAction(actionId);
      // Reset module selection
      this.form.patchValue({ moduleId: null });
    }
  }
}
