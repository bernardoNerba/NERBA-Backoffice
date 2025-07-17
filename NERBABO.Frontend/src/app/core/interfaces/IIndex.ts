import { Observable } from 'rxjs';

export interface IIndex {
  loading$: Observable<boolean>;

  onCreateModal(): void;

  updateBreadcrumbs(): void;
}
