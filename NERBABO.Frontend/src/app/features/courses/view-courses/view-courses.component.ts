import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Course, CourseKpi } from '../../../core/models/course';
import { catchError, Observable, of, Subscription, tap } from 'rxjs';
import { ICONS } from '../../../core/objects/icons';
import { Frame } from '../../../core/models/frame';
import { CoursesService } from '../../../core/services/courses.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FrameService } from '../../../core/services/frame.service';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { CommonModule } from '@angular/common';
import { MessageModule } from 'primeng/message';
import { BsModalService } from 'ngx-bootstrap/modal';
import { DeleteCoursesComponent } from '../delete-courses/delete-courses.component';
import { ChangeStatusCoursesComponent } from '../change-status-courses/change-status-courses.component';
import { AssignModuleCoursesComponent } from '../assign-module-courses/assign-module-courses.component';
import { ActionsService } from '../../../core/services/actions.service';
import { Action } from '../../../core/models/action';
import { StatusEnum } from '../../../core/objects/status';
import { Module } from '../../../core/models/module';
import { ModulesTableComponent } from '../../../shared/components/tables/modules-table/modules-table.component';
import { ActionsTableComponent } from '../../../shared/components/tables/actions-table/actions-table.component';
import { MenuItem } from 'primeng/api';
import { IView } from '../../../core/interfaces/IView';
import { UpsertCoursesComponent } from '../upsert-courses/upsert-courses.component';
import { UpsertActionsComponent } from '../../actions/upsert-actions/upsert-actions.component';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { MessageComponent } from '../../../shared/components/message/message.component';
import { KpiRowComponent } from '../../../shared/components/kpi-row/kpi-row.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-view-courses',
  imports: [
    IconComponent,
    CommonModule,
    MessageModule,
    ModulesTableComponent,
    ActionsTableComponent,
    TitleComponent,
    MessageComponent,
    KpiRowComponent,
  ],
  templateUrl: './view-courses.component.html',
})
export class ViewCoursesComponent implements IView, OnInit, OnDestroy {
  @Input({ required: true }) id!: number;
  course$?: Observable<Course | null>;
  actions$?: Observable<Action[]>;
  frame$?: Observable<Frame>;
  modules!: Module[];
  title!: string;
  course!: Course;
  kpis?: CourseKpi;
  ICONS = ICONS;
  STATUS = StatusEnum;
  menuItems: MenuItem[] | undefined;

  subscriptions: Subscription = new Subscription();

  // buttons for alert
  addModuleAction = {
    label: 'Adicionar Módulo',
    icon: 'pi pi-plus',
    callback: () => this.onAssignModuleModal(),
    severity: 'secondary' as const,
  };

  constructor(
    private coursesService: CoursesService,
    private frameService: FrameService,
    private sharedService: SharedService,
    private route: ActivatedRoute,
    private router: Router,
    private modalService: BsModalService,
    private actionsService: ActionsService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const courseId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(courseId ?? '');
    if (isNaN(this.id)) {
      this.router.navigate(['/courses']);
      return;
    }

    this.initializeEntity();
    this.initializeActions();
    this.initializeKpis();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
    this.assignModuleSourceSubscription();
    this.changeStatusSourceSubscription();
    this.modifiedActionSourceSubscription();
  }

  onUpdateModal() {
    this.modalService.show(UpsertCoursesComponent, {
      class: 'modal-lg',
      initialState: {
        id: this.id,
      },
    });
  }
  onDeleteModal() {
    this.modalService.show(DeleteCoursesComponent, {
      class: 'modal-md',
      initialState: {
        id: this.id,
        title: this.title,
      },
    });
  }

  onChangeStatusModal() {
    this.modalService.show(ChangeStatusCoursesComponent, {
      class: 'modal-md',
      initialState: {
        course: this.course,
      },
    });
  }

  onAssignModuleModal() {
    this.modalService.show(AssignModuleCoursesComponent, {
      class: 'modal-md',
      initialState: {
        course: this.course,
      },
    });
  }

  onCreateActionModal() {
    this.modalService.show(UpsertActionsComponent, {
      class: 'modal-lg',
      initialState: {
        id: 0,
        courseId: this.id,
        courseTitle: this.title,
      },
    });
  }

  initializeEntity() {
    this.course$ = this.coursesService.getSingleCourse(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/courses']);
          this.sharedService.showWarning('Informação não encontrada.');
        }
        return of(null);
      }),
      tap((course) => {
        if (course) {
          this.id = course.id;
          this.title = course.title;
          this.course = course;

          this.initializeFrame(course.frameId);

          this.updateBreadcrumbs();
          this.populateMenu();
        }
      })
    );
  }

  private initializeActions() {
    this.actions$ = this.actionsService.getActionsByCourseId(this.id);
  }

  private initializeKpis() {
    this.coursesService.getKpis(this.id).subscribe({
      next: (kpis) => {
        this.kpis = kpis;
      },
      error: (error) => {
        console.error('Error loading course KPIs:', error);
      },
    });
  }

  private initializeFrame(frameId: number) {
    this.frame$ = this.frameService.getSingle(frameId);
  }

  populateMenu() {
    this.menuItems = [
      {
        label: 'Opções',
        items: [
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
            label: 'Atribuir Módulo',
            icon: 'pi pi-plus-circle',
            command: () => this.onAssignModuleModal(),
          },
          {
            label: 'Criar Ação Formação',
            icon: 'pi pi-plus-circle',
            command: () => this.onCreateActionModal(),
          },
        ],
      },
    ];
  }

  updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/courses',
        displayName: 'Cursos',
        className: '',
      },
      {
        url: `/courses/${this.id}`,
        displayName:
          this.title.length > 21
            ? this.title.substring(0, 21) + '...'
            : this.title || 'Detalhes',
        className: 'inactive',
      },
    ]);
  }

  updateSourceSubscription() {
    this.subscriptions.add(
      this.coursesService.updatedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeEntity();
        }
      })
    );
  }

  deleteSourceSubscription() {
    this.subscriptions.add(
      this.coursesService.deletedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.router.navigateByUrl('/courses');
        }
      })
    );
  }

  private assignModuleSourceSubscription() {
    this.subscriptions.add(
      this.coursesService.assignModuleSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeEntity();
        }
      })
    );
  }

  private changeStatusSourceSubscription() {
    this.subscriptions.add(
      this.coursesService.changeStatusSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeEntity();
        }
      })
    );
  }

  private modifiedActionSourceSubscription() {
    this.subscriptions.add(
      this.coursesService.modifiedActionSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeEntity();
          this.initializeActions();
          this.initializeKpis();
        }
      })
    );
  }

  showMenu(): boolean {
    return (
      this.authService.userRoles.includes('Admin') ||
      this.authService.userRoles.includes('FM')
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
