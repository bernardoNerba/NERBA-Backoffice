import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { catchError, Observable, of, Subscription, tap } from 'rxjs';
import { Module, RetrievedModule } from '../../../core/models/module';
import { ICONS } from '../../../core/objects/icons';
import { ModulesService } from '../../../core/services/modules.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { Course } from '../../../core/models/course';
import { Action } from '../../../core/models/action';
import { ActionsService } from '../../../core/services/actions.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { DeleteModulesComponent } from '../delete-modules/delete-modules.component';
import { ActiveBadgeComponent } from '../../../shared/components/badges/active-badge/active-badge.component';
import { StatusEnum } from '../../../core/objects/status';
import { CoursesTableComponent } from '../../../shared/components/tables/courses-table/courses-table.component';
import { ActionsTableComponent } from '../../../shared/components/tables/actions-table/actions-table.component';
import { MenuItem } from 'primeng/api';
import { DropdownMenuComponent } from '../../../shared/components/dropdown-menu/dropdown-menu.component';
import { IView } from '../../../core/interfaces/IView';
import { UpsertModulesComponent } from '../upsert-modules/upsert-modules.component';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-view-modules',
  imports: [
    CommonModule,
    IconComponent,
    ActiveBadgeComponent,
    CoursesTableComponent,
    ActionsTableComponent,
    TitleComponent,
  ],
  templateUrl: './view-modules.component.html',
})
export class ViewModulesComponent implements IView, OnInit, OnDestroy {
  @Input({ required: true }) id!: number;
  module$?: Observable<RetrievedModule | null>;
  courses$?: Observable<Course[] | null>;
  actions$?: Observable<Action[] | null>;
  name?: string;
  ICONS = ICONS;
  STATUS = StatusEnum;
  hasActions: boolean = false;
  menuItems: MenuItem[] | undefined;

  subscriptions: Subscription = new Subscription();

  constructor(
    private modulesService: ModulesService,
    private sharedService: SharedService,
    private actionsService: ActionsService,
    private route: ActivatedRoute,
    private router: Router,
    private modalService: BsModalService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const moduleId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(moduleId ?? '');

    if (isNaN(this.id)) {
      this.router.navigate(['/modules']);
      return;
    }

    this.initializeEntity();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
    this.toggleSourceSubscription();
  }

  onUpdateModal(): void {
    const initialState = {
      id: this.id,
    };
    this.modalService.show(UpsertModulesComponent, {
      initialState: initialState,
      class: 'modal-md',
    });
  }

  onDeleteModal(): void {
    const initialState = {
      id: this.id,
      name: this.name,
    };
    this.modalService.show(DeleteModulesComponent, {
      initialState: initialState,
      class: 'modal-md',
    });
  }

  onToggleModule(): void {
    this.modulesService.toggleModuleIsActive(this.id);
  }

  initializeEntity() {
    this.module$ = this.modulesService.getSingleModule(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/modules']);
          this.sharedService.showWarning('Informação não foi encontrada.');
        }
        return of(null);
      }),
      tap((module) => {
        if (module) {
          this.id = module.id;
          this.name = module.name;
          this.updateBreadcrumbs();
          this.populateMenu();
        }
      })
    );
    this.courses$ = this.modulesService.getCoursesByModule(this.id);
    this.actions$ = this.actionsService.getActionsByModuleId(this.id);
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
            command: () => this.onToggleModule(),
          },
        ],
      },
    ];
  }

  updateSourceSubscription() {
    this.subscriptions.add(
      this.modulesService.updatedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeEntity();
        }
      })
    );
  }

  deleteSourceSubscription() {
    this.subscriptions.add(
      this.modulesService.deletedSource$.subscribe((id: number) => {
        if (this.id === id) this.router.navigateByUrl('/modules');
      })
    );
  }

  private toggleSourceSubscription() {
    this.subscriptions.add(
      this.modulesService.toggleSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeEntity();
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
        url: '/modules',
        displayName: 'Módulos',
        className: '',
      },
      {
        url: `/modules/${this.id}`,
        displayName: this.name?.substring(0, 21) + '...' || 'Detalhes',
        className: 'inactive',
      },
    ]);
  }

  canPerformAction(): boolean {
    return (
      this.authService.userRoles.includes('Admin') ||
      this.authService.userRoles.includes('FM')
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
