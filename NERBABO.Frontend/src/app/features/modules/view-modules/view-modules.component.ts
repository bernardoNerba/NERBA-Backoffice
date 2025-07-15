import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { catchError, Observable, of, Subscription, tap } from 'rxjs';
import { Module } from '../../../core/models/module';
import { ICONS } from '../../../core/objects/icons';
import { ModulesService } from '../../../core/services/modules.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { Course } from '../../../core/models/course';
import { Action } from '../../../core/models/action';
import { ActionsService } from '../../../core/services/actions.service';
import { FormatDateRangePipe } from '../../../shared/pipes/format-date-range.pipe';
import { BsModalService } from 'ngx-bootstrap/modal';
import { UpdateModulesComponent } from '../update-modules/update-modules.component';
import { DeleteModulesComponent } from '../delete-modules/delete-modules.component';
import { ActiveBadgeComponent } from '../../../shared/components/badges/active-badge/active-badge.component';
import { STATUS, StatusEnum } from '../../../core/objects/status';
import { CoursesTableComponent } from '../../../shared/components/tables/courses-table/courses-table.component';
import { ActionsTableComponent } from '../../../shared/components/tables/actions-table/actions-table.component';

@Component({
  selector: 'app-view-modules',
  imports: [
    CommonModule,
    IconComponent,
    RouterLink,
    FormatDateRangePipe,
    ActiveBadgeComponent,
    CoursesTableComponent,
    ActionsTableComponent,
  ],
  templateUrl: './view-modules.component.html',
  styleUrl: './view-modules.component.css',
})
export class ViewModulesComponent implements OnInit, OnDestroy {
  @Input({ required: true }) id!: number;
  module$?: Observable<Module | null>;
  courses$?: Observable<Course[] | null>;
  actions$?: Observable<Action[] | null>;
  name?: string;
  ICONS = ICONS;
  STATUS = StatusEnum;
  hasActions: boolean = false;

  private subscriptions: Subscription = new Subscription();

  constructor(
    private modulesService: ModulesService,
    private sharedService: SharedService,
    private actionsService: ActionsService,
    private route: ActivatedRoute,
    private router: Router,
    private modalService: BsModalService
  ) {}

  ngOnInit(): void {
    const moduleId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(moduleId ?? '');

    if (isNaN(this.id)) {
      this.router.navigate(['/modules']);
      return;
    }

    this.initializeModule();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
    this.toggleSourceSubscription();
  }

  onUpdateModal(m: Module) {
    const initialState = {
      id: m.id,
    };
    this.modalService.show(UpdateModulesComponent, {
      initialState: initialState,
      class: 'modal-md',
    });
  }

  onDeleteModal(id: number, name: string) {
    const initialState = {
      id: id,
      name: name,
    };
    this.modalService.show(DeleteModulesComponent, {
      initialState: initialState,
      class: 'modal-md',
    });
  }

  onToggleModule(id: number): void {
    this.modulesService.toggleModuleIsActive(id);
  }

  private initializeModule() {
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
          this.updateBreadcrumbs(this.id, this.name);
        }
      })
    );
    this.courses$ = this.modulesService.getCoursesByModule(this.id);
    this.actions$ = this.actionsService.getActionsByModuleId(this.id);
  }

  private updateSourceSubscription() {
    this.subscriptions.add(
      this.modulesService.updatedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeModule();
        }
      })
    );
  }

  private deleteSourceSubscription() {
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
          this.initializeModule();
        }
      })
    );
  }

  private updateBreadcrumbs(id: number, name: string): void {
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
        url: `/modules/${id}`,
        displayName: name.substring(0, 21) + '...' || 'Detalhes',
        className: 'inactive',
      },
    ]);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
