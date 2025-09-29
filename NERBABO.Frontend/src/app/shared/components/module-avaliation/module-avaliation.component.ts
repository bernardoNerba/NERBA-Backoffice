import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormsModule,
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
} from '@angular/forms';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { ModuleAvaliationsService } from '../../../core/services/module-avaliations.service';
import { ActionEnrollmentService } from '../../../core/services/action-enrollment.service';
import {
  AvaliationByModule,
  UpdateModuleAvaliation,
} from '../../../core/models/moduleAvaliation';
import { MessageService } from 'primeng/api';
import { ICONS } from '../../../core/objects/icons';

// PrimeNG imports
import { TableModule, Table } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { BadgeModule } from 'primeng/badge';
import { SpinnerComponent } from '../spinner/spinner.component';
import { SharedService } from '../../../core/services/shared.service';

@Component({
  selector: 'app-module-avaliation',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputNumberModule,
    TagModule,
    ToastModule,
    ProgressSpinnerModule,
    BadgeModule,
    SpinnerComponent,
  ],
  providers: [MessageService],
  templateUrl: './module-avaliation.component.html',
})
export class ModuleAvaliationComponent implements OnInit, OnDestroy {
  @Input({ required: true }) actionId!: number;
  @ViewChild('avaliationTable') avaliationTable!: Table;

  private destroy$ = new Subject<void>();

  avaliations: AvaliationByModule[] = [];
  avaliationForms: { [moduleId: number]: FormGroup } = {};
  loading = false;
  saving = false;
  activeIndex = -1;
  openAccordions: Set<number> = new Set();

  ICONS = ICONS;

  constructor(
    private moduleAvaliationService: ModuleAvaliationsService,
    private actionEnrollmentService: ActionEnrollmentService,
    private formBuilder: FormBuilder,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.loadData();
    this.setupSubscriptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSubscriptions(): void {
    this.moduleAvaliationService.updated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadData();
      });

    // Listen to enrollment changes to refresh module evaluation data
    this.actionEnrollmentService.createdSource$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        console.log('Module evaluations: Enrollment created, reloading data');
        this.loadData();
      });

    this.actionEnrollmentService.updatedSource$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        console.log('Module evaluations: Enrollment updated, reloading data');
        this.loadData();
      });

    this.actionEnrollmentService.deletedSource$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        console.log('Module evaluations: Enrollment deleted, reloading data');
        this.loadData();
      });
  }

  private loadData(): void {
    this.loading = true;

    this.moduleAvaliationService
      .getByActionId(this.actionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (avaliations: AvaliationByModule[]) => {
          this.avaliations = avaliations;
          this.createForms();
          this.loading = false;
          console.log(this.avaliations);
        },
        error: (error) => {
          console.error('Error loading module avaliations:', error);
          this.sharedService.showError(error.error.detail);
          this.loading = false;
        },
      });
  }

  private createForms(): void {
    this.avaliationForms = {};

    this.avaliations.forEach((avaliation) => {
      const studentControls = avaliation.gradings.map((grading) =>
        this.formBuilder.group({
          studentPersonId: [grading.studentPersonId],
          studentName: [grading.studentName],
          grade: [
            grading.grade,
            [Validators.required, Validators.min(0), Validators.max(5)],
          ],
          evaluated: [grading.evaluated],
          moduleAvaliationId: [grading.moduleAvaliationId],
        })
      );

      this.avaliationForms[avaliation.moduleId] = this.formBuilder.group({
        moduleId: [avaliation.moduleId],
        students: this.formBuilder.array(studentControls),
      });
    });
  }

  getStudentsFormArray(moduleId: number): FormArray {
    return this.avaliationForms[moduleId]?.get('students') as FormArray;
  }

  saveModuleAvaliations(moduleId: number): void {
    const form = this.avaliationForms[moduleId];
    if (!form || form.invalid) {
      form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    this.saving = true;
    const formValue = form.value;
    const updateRequests = formValue.students
      .filter((student: any) => student.moduleAvaliationId > 0)
      .map((student: any) => {
        const updateData: UpdateModuleAvaliation = {
          id: student.moduleAvaliationId,
          grade: student.grade,
        };
        return this.moduleAvaliationService.updateModuleAvaliation(
          student.moduleAvaliationId,
          updateData
        );
      });

    if (updateRequests.length === 0) {
      this.saving = false;
      this.sharedService.showError('Não há avaliações para guardar');
      return;
    }

    forkJoin(updateRequests)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.sharedService.showSuccess('Avaliações guardadas com sucesso');
          this.saving = false;
          this.loadDataWithStatePreservation();
        },
        error: (error) => {
          console.error('Error saving module avaliations:', error);
          this.sharedService.showError(error.error.detail);
          this.saving = false;
        },
      });
  }

  getGradeSeverity(grade: number): 'success' | 'warn' | 'danger' | 'secondary' {
    if (grade >= 3) {
      return 'success';
    } else if (grade < 3 && grade != 0) {
      return 'danger';
    } else {
      return 'warn';
    }
  }

  getGradeStatus(grade: number): 'Aprovado' | 'Reprovado' | 'Pendente' {
    if (grade >= 3) {
      return 'Aprovado';
    } else if (grade < 3 && grade != 0) {
      return 'Reprovado';
    } else {
      return 'Pendente';
    }
  }

  hasUnsavedChanges(moduleId: number): boolean {
    const form = this.avaliationForms[moduleId];
    return form ? form.dirty : false;
  }

  resetForm(moduleId: number): void {
    const form = this.avaliationForms[moduleId];
    if (form) {
      form.reset();
      this.loadDataWithStatePreservation();
    }
  }

  loadDataWithStatePreservation(): void {
    this.preserveAccordionState();
    this.loadData();
  }

  private preserveAccordionState(): void {
    this.openAccordions.clear();
    const openElements = document.querySelectorAll('.accordion-collapse.show');
    openElements.forEach((element) => {
      const moduleId = element.id.match(/module-(\d+)-content/);
      if (moduleId) {
        this.openAccordions.add(parseInt(moduleId[1]));
      }
    });
  }

  isModuleAvaliationsComplete(moduleId: number): boolean {
    const module = this.avaliations.find((m) => m.moduleId === moduleId);
    if (!module || module.gradings.length === 0) return false;

    const form = this.avaliationForms[moduleId];
    if (!form) return false;

    const studentsArray = this.getStudentsFormArray(moduleId);

    for (let i = 0; i < studentsArray.length; i++) {
      const studentControl = studentsArray.at(i);
      const evaluated = studentControl.get('evaluated')?.value;
      if (!evaluated) {
        return false;
      }
    }
    return true;
  }
}
