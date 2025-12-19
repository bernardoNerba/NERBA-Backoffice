import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { MultiSelectModule } from 'primeng/multiselect';
import { UserInfo } from '../../../../core/models/userInfo';
import { FilterService, MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';
import { CustomModalService } from '../../../../core/services/custom-modal.service';
import { Router } from '@angular/router';
import { AccService } from '../../../../core/services/acc.service';
import { BlockAccComponent } from '../../../../features/acc/block-acc/block-acc.component';
import { AssignRoleAccComponent } from '../../../../features/acc/assign-role-acc/assign-role-acc.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { InputTextModule } from 'primeng/inputtext';
import { ICONS } from '../../../../core/objects/icons';
import { Tag } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';
import { UpsertAccComponent } from '../../../../features/acc/upsert-acc/upsert-acc.component';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-accs-table',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    Button,
    Menu,
    MultiSelectModule,
    CommonModule,
    FormsModule,
    TruncatePipe,
    SpinnerComponent,
    InputTextModule,
    Tag,
    TooltipModule,
    IconAnchorComponent,
  ],
  templateUrl: './accs-table.component.html',
  standalone: true,
})
export class AccsTableComponent implements OnInit {
  @Input({ required: true }) users!: UserInfo[];
  @Input({ required: true }) loading!: boolean;
  @Input({ required: true }) roles!: string[];
  selectedRoles: string[] = [];
  @ViewChild('dt') dt!: Table;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedUser: UserInfo | undefined;
  first = 0;
  rows = 10;
  ICONS: any = ICONS;

  private subscriptions = new Subscription();

  constructor(
    private modalService: CustomModalService,
    private router: Router,
    private accService: AccService,
    private filterService: FilterService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Register custom filter for roles
    this.filterService.register(
      'rolesFilter',
      (value: string[], selectedRoles: string[]) => {
        if (!selectedRoles || selectedRoles.length === 0) {
          return true; // No roles selected, show all users
        }
        // Check if all selected roles are present in the user's roles
        return selectedRoles.every((role) => value.includes(role));
      }
    );

    this.menuItems = [
      {
        label: 'Opções',
        items: [
          {
            label: 'Editar',
            icon: 'pi pi-pencil',
            command: () => this.onUpdateUserModal(this.selectedUser!.id),
          },
          {
            label: 'Bloquear / Desbloquear',
            icon: 'pi pi-exclamation-triangle',
            command: () =>
              this.onBlockUserModal(
                this.selectedUser!.id,
                this.selectedUser!.isActive
              ),
          },
          {
            label: 'Detalhes',
            icon: 'pi pi-exclamation-circle',
            command: () =>
              this.router.navigateByUrl(
                `/people/${this.selectedUser!.personId}`
              ),
          },
          {
            label: 'Atribuir Papel',
            icon: 'pi pi-clipboard',
            command: () => this.onAssignRoleModal(this.selectedUser!.id),
          },
        ],
      },
    ];

    this.subscriptions.add(
      this.accService.updatedSource$.subscribe((userId) => {
        this.refreshUser(userId, 'update');
      })
    );

    this.subscriptions.add(
      this.accService.deletedSource$.subscribe((userId) => {
        this.refreshUser(userId, 'update');
      })
    );
  }

  // Apply the role filter
  applyRoleFilter() {
    this.dt.filter(this.selectedRoles, 'roles', 'rolesFilter');
  }

  refreshUser(id: string, action: 'update'): void {
    if (id === '0') {
      return;
    }

    const index = this.users.findIndex((user) => user.id === id);
    if (index === -1) return;

    this.accService.getUserById(id).subscribe({
      next: (updatedUser) => {
        this.users[index] = updatedUser;
        this.users = [...this.users];
      },
      error: (error) => {
        console.error('Failed to refresh user: ', error);
      },
    });
  }

  onUpdateUserModal(id: string): void {
    this.modalService.show(UpsertAccComponent, {
      class: 'modal-md',
      initialState: { id: id },
    });
  }

  onBlockUserModal(id: string, isActive: boolean): void {
    const fullName = this.accService.getuserById(id)?.fullName;
    this.modalService.show(BlockAccComponent, {
      class: 'modal-md',
      initialState: { id, active: isActive, fullName },
    });
  }

  onAssignRoleModal(id: string): void {
    const user = this.accService.getuserById(id);
    this.modalService.show(AssignRoleAccComponent, {
      class: 'modal-md',
      initialState: { user },
    });
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
    return this.users ? this.first + this.rows >= this.users.length : true;
  }

  isFirstPage(): boolean {
    return this.users ? this.first === 0 : true;
  }

  clearFilters() {
    this.searchValue = '';
    this.selectedRoles = [];
    this.dt.reset();
  }

  getStatusSeverity(status: boolean) {
    return status ? 'success' : 'danger';
  }

  getStatusIcon(status: boolean) {
    return status ? 'pi pi-check' : 'pi pi-times';
  }

  getRoleClass(role: string): string {
    const roleMap: { [key: string]: string } = {
      admin: 'admin',
      cq: 'cq',
      user: 'user',
      fm: 'fm',
    };
    return roleMap[role.toLowerCase()] || 'default';
  }

  getRoleIcon(role: string): string {
    const iconMap: { [key: string]: string } = {
      admin: 'pi pi-shield',
      cq: 'pi pi-user-edit',
      user: 'pi pi-user',
      fm: 'pi pi-graduation-cap',
    };
    return iconMap[role.toLowerCase()] || 'bi bi-person-circle';
  }

  getRoleTooltip(role: string): string {
    const tooltipMap: { [key: string]: string } = {
      admin: 'Administrador',
      cq: 'Centro Qualifica',
      user: 'Utilizador',
      fm: 'Formação Modular',
    };
    return tooltipMap[role.toLowerCase()] || role;
  }

  canUseDropdownMenu(): boolean {
    return this.authService.userRoles.includes('Admin');
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
