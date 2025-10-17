import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActionsTableComponent } from '../../../shared/components/tables/actions-table/actions-table.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { ICONS } from '../../../core/objects/icons';
import { Observable, Subscription, forkJoin } from 'rxjs';
import { Action, ActionKpi } from '../../../core/models/action';
import { ActionsService } from '../../../core/services/actions.service';
import { IIndex } from '../../../core/interfaces/IIndex';
import { BsModalService } from 'ngx-bootstrap/modal';
import { UpsertActionsComponent } from '../upsert-actions/upsert-actions.component';
import { SharedService } from '../../../core/services/shared.service';
import { CommonModule } from '@angular/common';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { KpiRowComponent } from '../../../shared/components/kpi-row/kpi-row.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-index-actions',
  imports: [
    ActionsTableComponent,
    CommonModule,
    TitleComponent,
    KpiRowComponent,
  ],
  templateUrl: './index-actions.component.html',
  styleUrl: './index-actions.component.css',
})
export class IndexActionsComponent implements IIndex, OnInit, OnDestroy {
  actions$!: Observable<Action[]>;
  loading$: any;
  ICONS = ICONS;

  // KPI properties
  aggregatedKpis: ActionKpi = {
    totalStudents: 0,
    totalApproved: 0,
    totalVolumeHours: 0,
    totalVolumeDays: 0,
  };

  private subscriptions: Subscription = new Subscription();
  private allActionsKpis: Map<number, ActionKpi> = new Map();

  constructor(
    private actionsService: ActionsService,
    private modalService: BsModalService,
    private sharedService: SharedService,
    private authService: AuthService
  ) {
    this.actions$ = this.actionsService.actions$;
    this.loading$ = this.actionsService.loading$;
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
    this.initializeKpis();
  }

  onCreateModal(): void {
    this.modalService.show(UpsertActionsComponent, {
      class: 'modal-lg',
      initialState: {
        id: 0,
        courseId: null,
        courseTitle: null,
      },
    });
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
    ]);
  }

  private initializeKpis(): void {
    // Subscribe to actions and load KPIs for each action
    this.subscriptions.add(
      this.actions$.subscribe((actions) => {
        if (actions && actions.length > 0) {
          this.loadAllKpis(actions);
        } else {
          this.resetKpis();
        }
      })
    );
  }

  private loadAllKpis(actions: Action[]): void {
    const kpiRequests = actions.map((action) =>
      this.actionsService.getKpis(action.id)
    );

    this.subscriptions.add(
      forkJoin(kpiRequests).subscribe({
        next: (allKpis) => {
          // Store KPIs by action ID
          actions.forEach((action, index) => {
            this.allActionsKpis.set(action.id, allKpis[index]);
          });
          // Calculate initial aggregated KPIs (all actions)
          this.calculateAggregatedKpis(actions);
        },
        error: (error) => {
          console.error('Error loading KPIs:', error);
          this.resetKpis();
        },
      })
    );
  }

  private calculateAggregatedKpis(filteredActions: Action[]): void {
    this.aggregatedKpis = {
      totalStudents: 0,
      totalApproved: 0,
      totalVolumeHours: 0,
      totalVolumeDays: 0,
    };

    filteredActions.forEach((action) => {
      const kpi = this.allActionsKpis.get(action.id);
      if (kpi) {
        this.aggregatedKpis.totalStudents += kpi.totalStudents;
        this.aggregatedKpis.totalApproved += kpi.totalApproved;
        this.aggregatedKpis.totalVolumeHours += kpi.totalVolumeHours;
        this.aggregatedKpis.totalVolumeDays += kpi.totalVolumeDays;
      }
    });
  }

  private resetKpis(): void {
    this.aggregatedKpis = {
      totalStudents: 0,
      totalApproved: 0,
      totalVolumeHours: 0,
      totalVolumeDays: 0,
    };
  }

  // Method to be called by the table component when filters change
  onTableFilter(filteredActions: Action[]): void {
    this.calculateAggregatedKpis(filteredActions);
  }

  canPerformActions(): boolean {
    return (
      this.authService.userRoles.includes('Admin') ||
      this.authService.userRoles.includes('FM')
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
