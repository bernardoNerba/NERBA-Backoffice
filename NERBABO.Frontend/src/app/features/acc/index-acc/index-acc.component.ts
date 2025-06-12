import { Component, OnInit } from '@angular/core';
import {
  BehaviorSubject,
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  map,
  Observable,
  startWith,
} from 'rxjs';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { environment } from '../../../../environments/environment.development';
import { BsModalService } from 'ngx-bootstrap/modal';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserInfo } from '../../../core/models/userInfo';
import { AccService } from '../../../core/services/acc.service';
import { SharedService } from '../../../core/services/shared.service';
import { UpdateAccComponent } from '../update-acc/update-acc.component';
import { BlockAccComponent } from '../block-acc/block-acc.component';
import { AssignRoleAccComponent } from '../assign-role-acc/assign-role-acc.component';
import { CreateAccComponent } from '../create-acc/create-acc.component';

@Component({
  selector: 'app-index-acc',
  imports: [
    CommonModule,
    BadgeComponent,
    ReactiveFormsModule,
    SpinnerComponent,
    RouterLink,
  ],
  templateUrl: './index-acc.component.html',
  styleUrl: './index-acc.component.css',
})
export class IndexAccComponent implements OnInit {
  roles = `${environment.roles}`.split(','); // roles set on the environment
  columns = ['#', 'Nome Completo', 'Username', 'Email', 'Papeis']; // List of column names
  searchControl = new FormControl(''); // search bar form reference
  users$!: Observable<UserInfo[] | null>;
  filteredUsers$!: Observable<UserInfo[] | null>;
  selectedRoles$ = new BehaviorSubject<Set<string>>(new Set());
  loading$!: Observable<boolean>;

  constructor(
    private accService: AccService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {
    this.users$ = this.accService.users$;
    this.loading$ = this.accService.loading$;

    this.updateBreadcrumbs();
  }

  ngOnInit(): void {
    this.filteredUsers$ = combineLatest([
      // combines the latest values of users, search input and selected roles
      this.users$,
      this.searchControl.valueChanges.pipe(
        startWith(''),
        debounceTime(100),
        distinctUntilChanged() // only emit if the value is different from the last one
      ),
      this.selectedRoles$,
    ]).pipe(
      map(([users, searchTerm, selectedRoles]) => {
        if (!users) return null;

        const term = (searchTerm || '').toLowerCase();
        const selectedRolesArray = Array.from(selectedRoles).map((r) =>
          r.toLowerCase()
        );

        return users.filter((user) => {
          // Filter by search term
          const matchesSearch = user.fullName?.toLowerCase().includes(term);

          // Filter by roles
          const userRoles = (
            Array.isArray(user.roles) ? user.roles : [user.roles]
          ).map((role) => role.toLowerCase());

          const matchesRole =
            selectedRoles.size === 0 ||
            selectedRolesArray.every((selectedRole) =>
              userRoles.includes(selectedRole)
            );

          return matchesSearch && matchesRole;
        });
      })
    );
  }

  onCreateUserModal(): void {
    this.modalService.show(CreateAccComponent, {
      class: 'modal-md',
      initialState: {},
    });
  }

  onUpdateUserModal(id: string) {
    const initialState = { id };
    this.modalService.show(UpdateAccComponent, { initialState });
  }

  onBlockUserModal(id: string, active: boolean) {
    const fullName = this.accService.getuserById(id)?.fullName;
    const initialState = { id, active, fullName };
    this.modalService.show(BlockAccComponent, { initialState });
  }

  onAssignRoleModal(id: string) {
    const user = this.accService.getuserById(id);
    const initialState = { user };
    this.modalService.show(AssignRoleAccComponent, { initialState });
  }

  onRoleClicked(role: string) {
    const current = this.selectedRoles$.value;
    const updated = new Set(current);

    if (updated.has(role)) {
      updated.delete(role);
    } else {
      updated.add(role);
    }

    this.selectedRoles$.next(updated);
  }

  private updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        displayName: 'Dashboard',
        url: '/dashboard',
        className: '',
      },
      {
        displayName: 'Utilizadores',
        url: '/users',
        className: 'inactive',
      },
    ]);
  }
}
