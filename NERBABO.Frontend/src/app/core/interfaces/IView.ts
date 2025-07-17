import { MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';

export interface IView {
  menuItems: MenuItem[] | undefined;
  subscriptions: Subscription;

  initializeEntity(): void;
  populateMenu(): void;

  onUpdateModal(): void;
  updateSourceSubscription(): void;
  onDeleteModal(): void;
  deleteSourceSubscription(): void;

  updateBreadcrumbs(): void;
}
