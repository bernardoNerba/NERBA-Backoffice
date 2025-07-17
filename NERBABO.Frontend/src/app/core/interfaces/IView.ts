export interface IView {
  menuItems: any;
  subscriptions: any;

  initializeEntity(): void;
  populateMenu(): void;

  onUpdateModal(): void;
  updateSourceSubscription(): void;
  onDeleteModal(): void;
  deleteSourceSubscription(): void;

  updateBreadcrumbs(): void;
}
