import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { CalendarModule } from 'primeng/calendar';
import { Action } from '../../../../core/models/action';
import { MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Router, RouterLink } from '@angular/router';
import { ActionsService } from '../../../../core/services/actions.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { InputTextModule } from 'primeng/inputtext';
import { IconComponent } from '../../icon/icon.component';
import { FormatDateRangePipe } from '../../../pipes/format-date-range.pipe';
import { STATUS, StatusEnum } from '../../../../core/objects/status';
import { ICONS } from '../../../../core/objects/icons';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-actions-table',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    Button,
    Menu,
    CalendarModule,
    CommonModule,
    FormsModule,
    RouterLink,
    TruncatePipe,
    SpinnerComponent,
    InputTextModule,
    FormatDateRangePipe,
    TagModule,
  ],
  templateUrl: './actions-table.component.html',
  styleUrls: ['./actions-table.component.css'],
  standalone: true,
})
export class ActionsTableComponent implements OnInit {
  @Input({ required: true }) actions!: Action[];
  @Input({ required: true }) loading!: boolean;
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
    private modalService: BsModalService,
    private router: Router,
    private actionsService: ActionsService
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
            command: () => this.onDeleteActionModal(this.selectedAction!),
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

    // Subscribe to action updates
    this.subscriptions.add(
      this.actionsService.updatedSource$.subscribe((actionId) => {
        this.refreshAction(actionId, 'update');
      })
    );

    // Subscribe to action deletions
    this.subscriptions.add(
      this.actionsService.deletedSource$.subscribe((actionId) => {
        this.refreshAction(actionId, 'delete');
      })
    );
  }

  // Update filtered actions when inputs change
  ngOnChanges(): void {
    this.applyFilters();
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
          action.status?.toLowerCase().includes(term) ||
          action.regiment?.toLowerCase().includes(term)
      );
    }

    this.filteredActions = filtered;
  }

  // Handle date range change
  filterByDateRange(): void {
    this.applyFilters();
  }

  refreshAction(id: number, action: 'update' | 'delete'): void {
    if (id === 0) {
      // Parent component handles full refresh
      return;
    }

    const index = this.actions.findIndex((action) => action.id === id);
    if (index === -1) return;

    if (action === 'delete') {
      this.actions = this.actions.filter((action) => action.id !== id);
      this.applyFilters(); // Reapply filters after deletion
    } else if (action === 'update') {
      this.actionsService.getActionById(id).subscribe({
        next: (updatedAction) => {
          this.actions[index] = updatedAction;
          this.actions = [...this.actions];
          this.applyFilters(); // Reapply filters after update
        },
        error: (error) => {
          console.error('Failed to refresh action: ', error);
        },
      });
    }
  }

  onUpdateActionModal(action: Action): void {
    // Implement as needed
  }

  onDeleteActionModal(action: Action): void {
    // Implement as needed
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
    this.applyFilters();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
