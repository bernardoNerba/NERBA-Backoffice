import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, Subject, BehaviorSubject, combineLatest } from 'rxjs';
import { takeUntil, map } from 'rxjs/operators';
import { Notification } from '../../../core/models/notification';
import { NotificationService } from '../../../core/services/notification.service';
import { SharedService } from '../../../core/services/shared.service';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { AvatarModule } from 'primeng/avatar';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { DropdownModule } from 'primeng/dropdown';
import { FormsModule } from '@angular/forms';
import { ICONS } from '../../../core/objects/icons';
import { SelectItem } from 'primeng/api';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { SelectButtonModule } from 'primeng/selectbutton';
import { CustomModalService } from '../../../core/services/custom-modal.service';
import { DeleteNotificationsComponent } from '../delete-notifications/delete-notifications.component';

@Component({
  selector: 'app-index-notifications',
  templateUrl: './index-notifications.component.html',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    TagModule,
    TooltipModule,
    AvatarModule,
    ChipModule,
    DividerModule,
    MessageModule,
    DropdownModule,
    PaginatorModule,
    SelectButtonModule,
  ],
})
export class IndexNotificationsComponent implements OnInit, OnDestroy {
  loading$: Observable<boolean>;
  viewData$: Observable<{ notifications: Notification[]; totalRecords: number; pagination: PaginatorState }>;
  sortValue$: Observable<string>;

  private allNotifications$: Observable<Notification[]>;
  private destroy$ = new Subject<void>();
  ICONS = ICONS;

  // View state
  layout: 'list' | 'grid' = 'list';
  isRefreshing = false;

  // State management streams
  private sort$ = new BehaviorSubject<{ field: keyof Notification; order: number }>({ field: 'createdAt', order: -1 });
  private pagination$ = new BehaviorSubject<PaginatorState>({ first: 0, rows: 10, page: 0, pageCount: 0 });

  // Sort options for the dropdown
  sortOptions: SelectItem[] = [
    { label: 'Mais recentes', value: '!createdAt' }, // '!' indicates descending
    { label: 'Mais antigas', value: 'createdAt' }, // No prefix indicates ascending
    { label: 'Não lidas primeiro', value: '!status' }, // Assuming 'Não Lida' status should sort higher
    { label: 'Lidas primeiro', value: 'status' },
  ];

  layoutOptions = [
    { icon: 'pi pi-bars', value: 'list', tooltip: 'Ver em lista' },
    { icon: 'pi pi-th-large', value: 'grid', tooltip: 'Ver em grelha' },
  ];

  constructor(
    private notificationService: NotificationService,
    private sharedService: SharedService,
    private authService: AuthService,
    private router: Router,
    private modalService: CustomModalService
  ) {
    this.loading$ = this.notificationService.loading$;

    this.sortValue$ = this.sort$.pipe(
      map(sort => (sort.order === -1 ? '!' : '') + sort.field)
    );

    // 1. Get the raw data and normalize it
    this.allNotifications$ = this.notificationService.notifications$.pipe(
      map(notifications =>
        notifications.map(n => ({
          ...n,
          // Standardize the type string to avoid duplicate cases in helpers
          type: n.type.replace(' Em ', ' em '),
        }))
      )
    );

    // 2. Combine all state streams to produce the final view data
    this.viewData$ = combineLatest([this.allNotifications$, this.sort$, this.pagination$]).pipe(
      map(([notifications, sort, pagination]) => {
        // Sort the data
        const sortedData = [...notifications].sort((a, b) => {
          const order = sort.order;

          // Use a type-safe switch to handle sorting for different fields
          switch (sort.field) {
            case 'createdAt':
              // Assuming createdAt can be converted to a Date
              return (new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()) * order;
            case 'status':
              return a.status.localeCompare(b.status) * order;
            default:
              return 0;
          }
        });

        // Paginate the sorted data
        const paginatedNotifications = sortedData.slice(pagination.first ?? 0, (pagination.first ?? 0) + (pagination.rows ?? 10));

        return {
          notifications: paginatedNotifications,
          totalRecords: notifications.length,
          pagination: pagination,
        };
      })
    );
  }

  ngOnInit(): void {
    this.loadNotifications();
    this.updateBreadcrumbs();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Loads all notifications from the service.
   */
  loadNotifications(): void {
    this.notificationService.getAllNotifications().pipe(takeUntil(this.destroy$)).subscribe();
  }

  /**
   * Handles sorting changes from the dropdown and updates the sort stream.
   * @param event The dropdown change event.
   */
  onSortChange(event: any) {
    const value = event.value;
    const order = value.startsWith('!') ? -1 : 1;
    const field = (value.startsWith('!') ? value.substring(1) : value) as keyof Notification;
    this.sort$.next({ field, order });
  }

  /**
   * Handles page changes from the paginator and updates the pagination stream.
   */
  onPageChange(event: PaginatorState) {
    this.pagination$.next(event);
  }

  // --- Notification Actions ---

  onMarkAsRead(notification: Notification): void {
    if (notification.status === 'Não Lida') {
      this.notificationService.markAsRead(notification.id).pipe(takeUntil(this.destroy$)).subscribe();
    }
  }

  onMarkAllAsRead(): void {
    this.notificationService.markAllAsRead().pipe(takeUntil(this.destroy$)).subscribe();
  }

  onDeleteNotification(notification: Notification): void {
    const initialState = {
      id: notification.id,
      title: notification.title,
    };

    this.modalService.show(DeleteNotificationsComponent, { initialState });
  }

  onNavigateToRelatedEntity(notification: Notification): void {
    if (notification.actionUrl) {
      this.router.navigate([notification.actionUrl]);
    }
  }

  /**
   * Manually triggers notification generation system-wide.
   * This scans all people in the system and updates notifications for missing documents.
   */
  onRefreshNotifications(): void {
    this.isRefreshing = true;
    this.notificationService.generateNotifications().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => {
        this.isRefreshing = false;
        this.sharedService.showSuccess(
          'Sistema de notificações atualizado com sucesso. A lista será recarregada automaticamente.'
        );
        // Reload notifications after generation
        this.notificationService.refreshNotifications();
      },
      error: () => {
        this.isRefreshing = false;
        this.sharedService.showError(
          'Ocorreu um erro ao atualizar o sistema de notificações. Tente novamente.'
        );
      }
    });
  }

  // --- Display Helpers ---

  /**
   * Maps notification status to a PrimeNG Tag severity.
   */
  private readonly STATUS_SEVERITY_MAP: Record<string, 'success' | 'info' | 'warning' | 'danger'> = {
    Lida: 'success',
    'Não Lida': 'warning',
    Arquivada: 'info',
  };
  getStatusSeverity(status: string): 'success' | 'info' | 'warning' | 'danger' {
    return this.STATUS_SEVERITY_MAP[status] ?? 'info';
  }

  /**
   * Maps notification type to a PrimeNG Tag severity.
   */
  private readonly TYPE_SEVERITY_MAP: Record<string, 'success' | 'info' | 'warning' | 'danger'> = {
    'Documento em Falta': 'danger',
    'Informação Incompleta': 'warning',
    'Alerta Geral': 'info',
  };
  getTypeSeverity(type: string): 'success' | 'info' | 'warning' | 'danger' {
    return this.TYPE_SEVERITY_MAP[type] ?? 'info';
  }

  /**
   * Maps notification type to a PrimeIcons icon class.
   */
  private readonly TYPE_ICON_MAP: Record<string, string> = {
    'Documento em Falta': 'pi pi-file-excel',
    'Informação Incompleta': 'pi pi-info-circle',
    'Alerta Geral': 'pi pi-bell',
  };
  getTypeIcon(type: string): string {
    return this.TYPE_ICON_MAP[type] ?? 'pi pi-bell';
  }

  /**
   * Maps notification type to a custom CSS class for Avatar background/text color.
   */
  private readonly AVATAR_CLASS_MAP: Record<string, string> = {
    'Documento em Falta': 'avatar-error',
    'Informação Incompleta': 'avatar-warning',
    'Alerta Geral': 'avatar-info',
  };
  getAvatarClass(type: string): string {
    return this.AVATAR_CLASS_MAP[type] ?? 'avatar-info';
  }

  /**
   * Returns a Bootstrap border color class based on the notification type's severity.
   */
  private readonly TYPE_BORDER_CLASS_MAP: Record<string, string> = {
    'Documento em Falta': 'border-start-danger',
    'Informação Incompleta': 'border-start-warning',
    'Alerta Geral': 'border-start-info',
  };
  getTypeBorderClass(type: string): string {
    return this.TYPE_BORDER_CLASS_MAP[type] ?? 'border-start-secondary';
  }

  updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/notifications',
        displayName: 'Notificações',
        className: 'inactive',
      },
    ]);
  }

  /**
   * Checks if the current user is an admin
   */
  get isAdmin(): boolean {
    return this.authService.isUserAdmin;
  }
}