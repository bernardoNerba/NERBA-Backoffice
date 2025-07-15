import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import {
  BehaviorSubject,
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  map,
  Observable,
  startWith,
} from 'rxjs';
import { UserInfo } from '../../../core/models/userInfo';
import { AccService } from '../../../core/services/acc.service';
import { SharedService } from '../../../core/services/shared.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { CreateAccComponent } from '../create-acc/create-acc.component';
import { CommonModule } from '@angular/common';
import { environment } from '../../../../environments/environment.development';
import { ICONS } from '../../../core/objects/icons';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { AccsTableComponent } from '../../../shared/components/tables/accs-table/accs-table.component';

@Component({
  selector: 'app-index-acc',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AccsTableComponent,
    IconComponent,
  ],
  templateUrl: './index-acc.component.html',
  styleUrls: ['./index-acc.component.css'],
})
export class IndexAccComponent implements OnInit {
  roles = `${environment.roles}`.split(','); // roles set on the environment
  users$!: Observable<UserInfo[] | null>;
  filteredUsers$!: Observable<UserInfo[]>;
  searchControl = new FormControl('');
  selectedRoles$ = new BehaviorSubject<string[]>([]);
  loading$!: Observable<boolean>;
  ICONS = ICONS;

  constructor(
    private accService: AccService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {
    this.users$ = this.accService.users$;
    this.loading$ = this.accService.loading$;
  }

  ngOnInit(): void {
    this.filteredUsers$ = combineLatest([
      this.users$,
      this.searchControl.valueChanges.pipe(
        startWith(''),
        debounceTime(300),
        distinctUntilChanged()
      ),
      this.selectedRoles$,
    ]).pipe(
      map(([users, searchTerm, selectedRoles]) => {
        if (!users) return [];
        const term = (searchTerm || '').toLowerCase();
        const selectedRolesArray = selectedRoles.map((r) => r.toLowerCase());
        return users.filter((user) => {
          const matchesSearch =
            user.fullName?.toLowerCase().includes(term) ||
            user.userName?.toLowerCase().includes(term) ||
            user.email?.toLowerCase().includes(term);
          const userRoles = (
            Array.isArray(user.roles) ? user.roles : [user.roles]
          ).map((role) => role.toLowerCase());
          const matchesRole =
            selectedRolesArray.length === 0 ||
            selectedRolesArray.every((selectedRole) =>
              userRoles.includes(selectedRole)
            );
          return matchesSearch && matchesRole;
        });
      }),
      startWith([])
    );
    this.updateBreadcrumbs();
  }

  onCreateUserModal(): void {
    this.modalService.show(CreateAccComponent, {
      class: 'modal-md',
      initialState: {},
    });
  }

  private updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        displayName: 'Dashboard',
        url: '/dashboard',
        className: '',
      },
      {
        displayName: 'Contas',
        url: '/accs',
        className: 'inactive',
      },
    ]);
  }
}
