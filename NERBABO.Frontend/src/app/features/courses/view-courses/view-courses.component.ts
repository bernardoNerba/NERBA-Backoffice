import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Course } from '../../../core/models/course';
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
import { UpdateCoursesComponent } from '../update-courses/update-courses.component';
import { DeleteCoursesComponent } from '../delete-courses/delete-courses.component';
import { ChangeStatusCoursesComponent } from '../change-status-courses/change-status-courses.component';
import { AssignModuleCoursesComponent } from '../assign-module-courses/assign-module-courses.component';
import { ActionsService } from '../../../core/services/actions.service';
import { Action } from '../../../core/models/action';
import { StatusEnum } from '../../../core/objects/status';
import { CreateActionsComponent } from '../../actions/create-actions/create-actions.component';
import { Module } from '../../../core/models/module';
import { ModulesTableComponent } from '../../../shared/components/tables/modules-table/modules-table.component';
import { ActionsTableComponent } from '../../../shared/components/tables/actions-table/actions-table.component';
import { MenuItem } from 'primeng/api';
import { DropdownMenuComponent } from '../../../shared/components/dropdown-menu/dropdown-menu.component';
import { IView } from '../../../core/interfaces/IView';

@Component({
  selector: 'app-view-courses',
  imports: [
    IconComponent,
    CommonModule,
    MessageModule,
    ModulesTableComponent,
    ActionsTableComponent,
    DropdownMenuComponent,
  ],
  templateUrl: './view-courses.component.html',
})
export class ViewCoursesComponent implements IView, OnInit, OnDestroy {
  @Input({ required: true }) id!: number;
  course$?: Observable<Course | null>;
  actions$?: Observable<Action[]>;
  frame!: Frame;
  modules!: Module[];
  title!: string;
  frameId!: number;
  course!: Course;
  ICONS = ICONS;
  STATUS = StatusEnum;
  menuItems: MenuItem[] | undefined;

  subscriptions: Subscription = new Subscription();

  constructor(
    private coursesService: CoursesService,
    private frameService: FrameService,
    private sharedService: SharedService,
    private route: ActivatedRoute,
    private router: Router,
    private modalService: BsModalService,
    private actionsService: ActionsService
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
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
    this.assignModuleSourceSubscription();
    this.changeStatusSourceSubscription();
  }

  onUpdateModal() {
    this.modalService.show(UpdateCoursesComponent, {
      class: 'modal-lg',
      initialState: {
        id: this.id,
        course: this.course,
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
    this.modalService.show(CreateActionsComponent, {
      class: 'modal-lg',
      initialState: {
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

          this.updateBreadcrumbs();

          this.frameService.getSingle(course.frameId).subscribe({
            next: (frame: Frame) => {
              this.frame = frame;
            },
            error: (error: any) => {
              this.sharedService.showError(error.detail);
            },
          });

          this.populateMenu();
        }
      })
    );
  }

  private initializeActions() {
    this.actions$ = this.actionsService.getActionsByCourseId(this.id);
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

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
