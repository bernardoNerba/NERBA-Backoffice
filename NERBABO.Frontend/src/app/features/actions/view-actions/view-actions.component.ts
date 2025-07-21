import { Component, Input, OnInit } from '@angular/core';
import { catchError, Observable, of, Subscription, tap } from 'rxjs';
import { IView } from '../../../core/interfaces/IView';
import { Action } from '../../../core/models/action';
import { MenuItem } from 'primeng/api';
import { ActionsService } from '../../../core/services/actions.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { BsModalService } from 'ngx-bootstrap/modal';
import { UpsertActionsComponent } from '../upsert-actions/upsert-actions.component';
import { DeleteActionsComponent } from '../delete-actions/delete-actions.component';
import { ChangeStatusActionsComponent } from '../change-status-actions/change-status-actions.component';
import { CommonModule } from '@angular/common';
import { DropdownMenuComponent } from '../../../shared/components/dropdown-menu/dropdown-menu.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { Course } from '../../../core/models/course';
import { CoursesService } from '../../../core/services/courses.service';
import { ICONS } from '../../../core/objects/icons';

@Component({
  selector: 'app-view-actions',
  imports: [CommonModule, DropdownMenuComponent, IconComponent],
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

  ICONS = ICONS;

  subscriptions: Subscription = new Subscription();

  constructor(
    private actionsService: ActionsService,
    private sharedService: SharedService,
    private router: Router,
    private modalService: BsModalService,
    private route: ActivatedRoute,
    private courseService: CoursesService
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
          this.populateMenu();
          this.initializeCourse();
        }
      })
    );
  }

  private initializeCourse(): void {
    this.course$ = this.courseService.getSingleCourse(this.courseId);
  }

  populateMenu(): void {
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
            label: 'Ver Curso',
            icon: 'pi pi-exclamation-circle',
            command: () =>
              this.router.navigateByUrl(`/courses/${this.courseId}`),
          },
          {
            label: 'Adicionar Formando',
            icon: 'pi pi-plus',
            command: () => {}, // TODO: Implement add student to action
          },
          {
            label: 'Adicionar Formador',
            icon: 'pi pi-plus',
            command: () => {}, // TODO: Implement add teacher to action
          },
          {
            label: 'Concluir Ação',
            icon: 'pi pi-plus',
            command: () => {}, // TODO: Implement complete action only available for coordenator
          },
        ],
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
}
