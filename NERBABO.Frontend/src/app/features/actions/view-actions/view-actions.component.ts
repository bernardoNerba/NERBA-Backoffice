import { Component, Input, OnInit } from '@angular/core';
import { catchError, Observable, of, Subscription, tap } from 'rxjs';
import { IView } from '../../../core/interfaces/IView';
import { Action, ActionKpi } from '../../../core/models/action';
import { MenuItem } from 'primeng/api';
import { ActionsService } from '../../../core/services/actions.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { BsModalService } from 'ngx-bootstrap/modal';
import { UpsertActionsComponent } from '../upsert-actions/upsert-actions.component';
import { DeleteActionsComponent } from '../delete-actions/delete-actions.component';
import { ChangeStatusActionsComponent } from '../change-status-actions/change-status-actions.component';
import { UpsertModuleTeachingComponent } from '../upsert-module-teaching/upsert-module-teaching.component';
import { UpsertActionEnrollmentComponent } from '../../enrollments/upsert-action-enrollment/upsert-action-enrollment.component';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { Course } from '../../../core/models/course';
import { CoursesService } from '../../../core/services/courses.service';
import { ICONS } from '../../../core/objects/icons';
import { ModulesTableComponent } from '../../../shared/components/tables/modules-table/modules-table.component';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { IconAnchorComponent } from '../../../shared/components/anchors/icon-anchor.component';
import { ModulesService } from '../../../core/services/modules.service';
import { ModuleTeachingService } from '../../../core/services/module-teaching.service';
import { Module, ModuleTeacher } from '../../../core/models/module';
import { MessageComponent } from '../../../shared/components/message/message.component';
import { SessionsSchedulerComponent } from '../../../shared/components/sessions-scheduler/sessions-scheduler.component';
import { MessageService } from 'primeng/api';
import { PdfActionsComponent } from '../../../shared/components/pdf-actions/pdf-actions.component';
import { ActionEnrollmentTableComponent } from '../../../shared/components/tables/action-enrollment-table/action-enrollment-table.component';
import { ActionEnrollmentService } from '../../../core/services/action-enrollment.service';
import { ActionEnrollment } from '../../../core/models/actionEnrollment';
import { KpiRowComponent } from '../../../shared/components/kpi-row/kpi-row.component';
import { SessionAttendanceComponent } from '../../../shared/components/session-attendance/session-attendance.component';
import { ModuleAvaliationComponent } from '../../../shared/components/module-avaliation/module-avaliation.component';
import { ProcessMtPaymentsComponent } from '../../../shared/components/process-mt-payments/process-mt-payments.component';

@Component({
  selector: 'app-view-actions',
  imports: [
    CommonModule,
    IconComponent,
    ModulesTableComponent,
    TitleComponent,
    IconAnchorComponent,
    MessageComponent,
    SessionsSchedulerComponent,
    PdfActionsComponent,
    ActionEnrollmentTableComponent,
    KpiRowComponent,
    SessionAttendanceComponent,
    ModuleAvaliationComponent,
    ProcessMtPaymentsComponent,
  ],
  providers: [MessageService],
  templateUrl: './view-actions.component.html',
})
export class ViewActionsComponent implements IView, OnInit {
  @Input({ required: true }) id!: number;
  title!: string;
  courseId!: number;
  action$?: Observable<Action | null>;
  course$?: Observable<Course | null>;
  menuItems: MenuItem[] | undefined;
  action?: Action;
  modulesWithoutTeacher: Module[] = [];
  modulesWithTeacher: ModuleTeacher[] = [];
  actionEnrollments: ActionEnrollment[] = [];
  enrollmentsLoading: boolean = false;
  kpis?: ActionKpi;

  ICONS = ICONS;

  subscriptions: Subscription = new Subscription();

  constructor(
    private actionsService: ActionsService,
    private sharedService: SharedService,
    private router: Router,
    private modalService: BsModalService,
    private route: ActivatedRoute,
    private courseService: CoursesService,
    private modulesService: ModulesService,
    private moduleTeachingService: ModuleTeachingService,
    private actionEnrollmentService: ActionEnrollmentService
  ) {}

  ngOnInit(): void {
    const actionId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(actionId ?? '');
    if (isNaN(this.id)) {
      this.router.navigate(['/actions']);
      return;
    }
    this.initializeEntity();

    this.updateSourceSubscription();
    this.deleteSourceSubscription();
    this.moduleTeachingCreatedSubscription();
    this.moduleChangesSubscription();
    this.enrollmentChangesSubscription();
  }

  initializeEntity(): void {
    this.action$ = this.actionsService.getActionById(this.id).pipe(
      catchError((error) => {
        console.error(error);
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/actions']);
          this.sharedService.showWarning('Informação não encontrada.');
        }
        return of(null);
      }),
      tap((action) => {
        console.log('Action' + action);
        if (action) {
          this.id = action.id;
          this.title = action.title;
          this.action = action;
          this.courseId = action.courseId;

          this.updateBreadcrumbs();
          this.initializeCourse();
          this.loadModulesWithoutTeacher();
          this.loadModulesWithTeacher();
          this.loadActionEnrollments();
          this.loadKpis();
        }
      })
    );
  }
  getModulesWithoutTeacherNames(): string[] {
    return this.modulesWithoutTeacher.map(
      (module) => `${module.name} - ${module.hours} horas`
    );
  }
  private initializeCourse(): void {
    this.course$ = this.courseService.getSingleCourse(this.courseId);
  }

  loadModulesWithoutTeacher(): void {
    this.modulesService.getModulesWithoutTeacherByAction(this.id).subscribe({
      next: (modules) => {
        this.modulesWithoutTeacher = modules;
        // Repopulate menu after loading modules to conditionally show/hide "Adicionar Formador"
        this.populateMenu();
      },
      error: (error) => {
        console.error('Error loading modules without teacher:', error);
        this.modulesWithoutTeacher = [];
        // Repopulate menu even on error to ensure consistency
        this.populateMenu();
      },
    });
  }

  loadModulesWithTeacher(): void {
    this.modulesService.getModulesWithTeacherByAction(this.id).subscribe({
      next: (modules) => {
        this.modulesWithTeacher = modules;
      },
      error: (error) => {
        console.error('Error loading modules with teacher:', error);
        this.modulesWithTeacher = [];
      },
    });
  }

  loadActionEnrollments(): void {
    this.enrollmentsLoading = true;
    this.actionEnrollmentService.getByActionId(this.id).subscribe({
      next: (enrollments) => {
        this.actionEnrollments = enrollments;
        this.enrollmentsLoading = false;
      },
      error: (error) => {
        console.error('Error loading action enrollments:', error);
        this.actionEnrollments = [];
        this.enrollmentsLoading = false;
      },
    });
  }

  loadKpis(): void {
    console.log('Loading KPIs for action ID:', this.id);
    this.actionsService.getKpis(this.id).subscribe({
      next: (kpi: ActionKpi) => {
        console.log('KPIs loaded successfully:', kpi);
        this.kpis = kpi;
      },
      error: (error: any) => {
        console.error('Error loading KPIs:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.message);
      },
    });
  }

  populateMenu(): void {
    const baseMenuItems = [
      {
        label: 'Editar',
        icon: 'pi pi-pencil',
        command: () => this.onUpdateModal(),
      },
      {
        label: 'Eliminar',
        icon: 'pi pi-exclamation-triangle',
        command: () => this.onDeleteModal(),
      },
      {
        label: 'Atualizar Estado',
        icon: 'pi pi-refresh',
        command: () => this.onChangeStatusModal(),
      },
      {
        label: 'Ver Curso',
        icon: 'pi pi-exclamation-circle',
        command: () => this.router.navigateByUrl(`/courses/${this.courseId}`),
      },
      {
        label: 'Adicionar Formando',
        icon: 'pi pi-plus',
        command: () => this.onAddStudentModal(),
      },
    ];

    // Add "Adicionar Formador" only if there are modules without teachers
    if (this.modulesWithoutTeacher.length > 0) {
      baseMenuItems.push({
        label: 'Adicionar Formador',
        icon: 'pi pi-plus',
        command: () => this.onAddTeacherModal(),
      });
    }

    baseMenuItems.push({
      label: 'Concluir Ação',
      icon: 'pi pi-plus',
      command: () => {}, // TODO: Implement complete action only available for coordenator
    });

    this.menuItems = [
      {
        label: 'Opções',
        items: baseMenuItems,
      },
    ];
  }

  onUpdateModal(): void {
    this.modalService.show(UpsertActionsComponent, {
      class: 'modal-lg',
      initialState: {
        id: this.id,
        courseId: null,
        courseTitle: null,
      },
    });
  }

  updateSourceSubscription(): void {
    this.subscriptions.add(
      this.actionsService.updatedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeEntity();
        }
      })
    );
  }

  onDeleteModal(): void {
    this.modalService.show(DeleteActionsComponent, {
      class: 'modal-md',
      initialState: {
        id: this.id,
        title: this.title,
        courseId: this.courseId,
      },
    });
  }

  deleteSourceSubscription(): void {
    this.subscriptions.add(
      this.actionsService.deletedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.router.navigateByUrl('/actions');
        }
      })
    );
  }

  moduleTeachingCreatedSubscription(): void {
    this.subscriptions.add(
      this.moduleTeachingService.createdSource$.subscribe(() => {
        // Reload both module lists when a new module teaching is created
        this.loadModulesWithoutTeacher();
        this.loadModulesWithTeacher();
      })
    );

    // Also listen for updates to module teachings
    this.subscriptions.add(
      this.moduleTeachingService.updatedSource$.subscribe(() => {
        // Reload both module lists when a module teaching is updated
        this.loadModulesWithoutTeacher();
        this.loadModulesWithTeacher();
      })
    );
  }

  moduleChangesSubscription(): void {
    this.subscriptions.add(
      this.modulesService.toggleSource$.subscribe(() => {
        // Reload modules with teacher data when a module is toggled
        this.loadModulesWithTeacher();
      })
    );

    // Also listen for module updates to reload teacher data
    this.subscriptions.add(
      this.modulesService.updatedSource$.subscribe(() => {
        // Reload modules with teacher data when a module is updated
        this.loadModulesWithTeacher();
      })
    );
  }

  updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/actions',
        displayName: 'Ações Formativas',
        className: 'inactive',
      },
      {
        url: `/actions/${this.id}`,
        displayName:
          this.title.length > 21
            ? this.title.substring(0, 21) + '...'
            : this.title || 'Detalhes',
        className: 'inactive',
      },
    ]);
  }

  onChangeStatusModal(): void {
    this.modalService.show(ChangeStatusActionsComponent, {
      class: 'modal-md',
      initialState: {
        action: this.action,
      },
    });
  }

  onAddTeacherModal(): void {
    this.modalService.show(UpsertModuleTeachingComponent, {
      class: 'modal-lg',
      initialState: {
        actionId: this.id,
        actionTitle: this.title,
      },
    });
  }

  onAddStudentModal(): void {
    const initialState = {
      id: 0, // 0 indicates create mode
      actionId: this.id,
    };

    this.modalService.show(UpsertActionEnrollmentComponent, {
      initialState,
      class: 'modal-lg',
    });
  }

  enrollmentChangesSubscription(): void {
    this.subscriptions.add(
      this.actionEnrollmentService.createdSource$.subscribe(() => {
        this.loadActionEnrollments();
      })
    );

    this.subscriptions.add(
      this.actionEnrollmentService.updatedSource$.subscribe(() => {
        this.loadActionEnrollments();
      })
    );

    this.subscriptions.add(
      this.actionEnrollmentService.deletedSource$.subscribe(() => {
        this.loadActionEnrollments();
      })
    );
  }
}
