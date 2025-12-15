import {
  Component,
  Input,
  OnInit,
  OnChanges,
  ViewChild,
  Output,
  EventEmitter,
} from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { Action } from '../../../../core/models/action';
import { MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';
import { CustomModalService } from '../../../../core/services/custom-modal.service';
import { Router, RouterLink } from '@angular/router';
import { ActionsService } from '../../../../core/services/actions.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { InputTextModule } from 'primeng/inputtext';
import { FormatDateRangePipe } from '../../../pipes/format-date-range.pipe';
import { StatusEnum } from '../../../../core/objects/status';
import { ICONS } from '../../../../core/objects/icons';
import { TagModule } from 'primeng/tag';
import { UpsertActionsComponent } from '../../../../features/actions/upsert-actions/upsert-actions.component';
import { DeleteActionsComponent } from '../../../../features/actions/delete-actions/delete-actions.component';
import { DatePicker } from 'primeng/datepicker';
import { ChangeStatusActionsComponent } from '../../../../features/actions/change-status-actions/change-status-actions.component';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-actions-table',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    Button,
    Menu,
    CommonModule,
    FormsModule,
    TruncatePipe,
    SpinnerComponent,
    InputTextModule,
    FormatDateRangePipe,
    TagModule,
    DatePicker,
    IconAnchorComponent,
  ],
  templateUrl: './actions-table.component.html',
})
export class ActionsTableComponent implements OnInit, OnChanges {
  @Input({ required: true }) actions!: Action[];
  @Input({ required: true }) loading!: boolean;
  @Output() tableFilter = new EventEmitter<Action[]>();
  @ViewChild('dt') dt!: Table;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  dateRange: Date[] | null = null;
  selectedAction: Action | undefined;
  first = 0;
  rows = 10;
  STATUS = StatusEnum;
  ICONS = ICONS;
  filteredActions: Action[] = []; // Store filtered actions

  private subscriptions = new Subscription();

  constructor(
    private modalService: CustomModalService,
    private router: Router,
    private actionsService: ActionsService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.menuItems = [
      {
        label: 'Opções',
        items: [
          {
            label: 'Editar',
            icon: 'pi pi-pencil',
            command: () => this.onUpdateActionModal(this.selectedAction!),
          },
          {
            label: 'Eliminar',
            icon: 'pi pi-exclamation-triangle',
            command: () =>
              this.onDeleteActionModal(
                this.selectedAction!.id,
                this.selectedAction!.title,
                this.selectedAction!.courseId
              ),
          },
          {
            label: 'Atualizar Estado',
            icon: 'pi pi-refresh',
            command: () => this.onChangeStatusModal(this.selectedAction!),
          },
          {
            label: 'Detalhes',
            icon: 'pi pi-exclamation-circle',
            command: () =>
              this.router.navigateByUrl(`/actions/${this.selectedAction!.id}`),
          },
        ],
      },
    ];

    // Initialize filteredActions
    this.filteredActions = this.actions;
    this.tableFilter.emit(this.filteredActions);

    // Subscribe to action updates
    this.subscriptions.add(
      this.actionsService.updatedSource$.subscribe((actionId) => {
        this.refreshAction(actionId);
      })
    );

    // Subscribe to action deletions
    this.subscriptions.add(
      this.actionsService.deletedSource$.subscribe((actionId) => {
        this.actions = this.actions.filter((action) => action.id !== actionId);
      })
    );
  }

  // Update filtered actions when inputs change
  ngOnChanges(): void {
    if (this.actions) {
      this.applyFilters();
    }
  }

  // Apply search and date range filters
  applyFilters(): void {
    let filtered = [...this.actions];

    // Apply date range filter
    if (
      this.dateRange &&
      this.dateRange.length === 2 &&
      this.dateRange[0] &&
      this.dateRange[1]
    ) {
      const [start, end] = this.dateRange;
      filtered = filtered.filter((action) => {
        const actionStart = new Date(action.startDate);
        const actionEnd = new Date(action.endDate);
        const filterStart = new Date(start);
        const filterEnd = new Date(end);
        return actionStart <= filterEnd && actionEnd >= filterStart;
      });
    }

    // Apply search filter
    if (this.searchValue) {
      const term = this.searchValue.toLowerCase();
      filtered = filtered.filter(
        (action) =>
          action.title?.toLowerCase().includes(term) ||
          action.courseTitle?.toLowerCase().includes(term) ||
          action.locality?.toLowerCase().includes(term) ||
          action.status?.toLowerCase().includes(term) ||
          action.regiment?.toLowerCase().includes(term) ||
          action.actionNumber?.toString().includes(term) ||
          action.startDate?.toString().includes(term)
      );
    }

    this.filteredActions = filtered;
    this.tableFilter.emit(this.filteredActions);
  }

  // Handle date range change
  filterByDateRange(): void {
    this.applyFilters();
  }

  // Sort table
  onSort(event: any) {
    this.filteredActions.sort((a, b) => {
      const valueA = this.getProperty(a, event.field);
      const valueB = this.getProperty(b, event.field);
      let result = 0;

      if (valueA != null && valueB != null) {
        if (typeof valueA === 'string' && typeof valueB === 'string') {
          result = valueA.localeCompare(valueB);
        } else {
          result = valueA < valueB ? -1 : valueA > valueB ? 1 : 0;
        }
      } else if (valueA != null) {
        result = -1;
      } else if (valueB != null) {
        result = 1;
      }

      return result * event.order;
    });
  }

  private getProperty(obj: any, path: string): any {
    return path.split('.').reduce((o, key) => (o && o[key]) || null, obj);
  }

  refreshAction(id: number): void {
    if (id === 0) {
      // if id is 0, needs a full refresh
      this.actionsService.triggerFetchActions();
      return;
    }

    // Check if the current action in the current actions list
    this.actionsService.getActionById(id).subscribe({
      next: (updatedAction) => {
        const index = this.actions.findIndex((action) => action.id == id);
        if (index !== -1) {
          this.actions[index] = updatedAction;
          this.actions = [...this.actions];
          this.applyFilters(); // Reapply filters after update
        }
      },
      error: (error) => {
        console.error('Failed to refresh action: ', error);
        this.actionsService.triggerFetchActions();
      },
    });
  }

  onUpdateActionModal(action: Action): void {
    this.modalService.show(UpsertActionsComponent, {
      class: 'modal-lg',
      initialState: {
        id: action.id,
        courseId: action.courseId,
        courseTitle: action.courseTitle,
      },
    });
  }

  onDeleteActionModal(id: number, title: string, courseId: number): void {
    this.modalService.show(DeleteActionsComponent, {
      class: 'modal-md',
      initialState: {
        id: id,
        title: title,
        courseId: courseId,
      },
    });
  }

  onChangeStatusModal(action: Action) {
    this.modalService.show(ChangeStatusActionsComponent, {
      class: 'modal-md',
      initialState: {
        action: action,
      },
    });
  }

  getStatusSeverity(status: string) {
    switch (status) {
      case StatusEnum.Cancelled:
        return 'secondary';
      case StatusEnum.Completed:
        return 'success';
      case StatusEnum.InProgress:
        return 'warn';
      case StatusEnum.NotStarted:
        return 'info';
      default:
        return null;
    }
  }

  getStatusIcon(status: string) {
    switch (status) {
      case StatusEnum.Cancelled:
        return 'pi pi-times';
      case StatusEnum.Completed:
        return 'pi pi-check';
      case StatusEnum.InProgress:
        return 'pi pi-exclamation-triangle';
      case StatusEnum.NotStarted:
        return 'pi pi-info-circle';
      default:
        return null;
    }
  }

  next() {
    this.first = this.first + this.rows;
  }

  prev() {
    this.first = this.first - this.rows;
  }

  reset() {
    this.first = 0;
  }

  pageChange(event: any) {
    this.first = event.first;
    this.rows = event.rows;
  }

  isLastPage(): boolean {
    return this.filteredActions
      ? this.first + this.rows >= this.filteredActions.length
      : true;
  }

  isFirstPage(): boolean {
    return this.filteredActions ? this.first === 0 : true;
  }

  clearFilters() {
    this.searchValue = '';
    this.dateRange = null;
    this.dt.reset();
    this.applyFilters();
  }

  canUseDropdownMenu(): boolean {
    return (
      this.authService.userRoles.includes('Admin') ||
      this.authService.userRoles.includes('FM')
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
