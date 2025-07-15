import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { MultiSelect, MultiSelectModule } from 'primeng/multiselect';
import { UserInfo } from '../../../../core/models/userInfo';
import { MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Router, RouterLink } from '@angular/router';
import { AccService } from '../../../../core/services/acc.service';
import { UpdateAccComponent } from '../../../../features/acc/update-acc/update-acc.component';
import { BlockAccComponent } from '../../../../features/acc/block-acc/block-acc.component';
import { AssignRoleAccComponent } from '../../../../features/acc/assign-role-acc/assign-role-acc.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { InputTextModule } from 'primeng/inputtext';
import { IconComponent } from '../../icon/icon.component';
import { ICONS } from '../../../../core/objects/icons';
import { Tag } from 'primeng/tag';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';

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
    RouterLink,
    TruncatePipe,
    SpinnerComponent,
    InputTextModule,
    IconComponent,
    Tag,
    IconAnchorComponent,
  ],
  templateUrl: './accs-table.component.html',
  styleUrls: ['./accs-table.component.css'],
  standalone: true,
})
export class AccsTableComponent implements OnInit {
  @Input({ required: true }) users!: UserInfo[];
  @Input({ required: true }) loading!: boolean;
  @Input({ required: true }) roles!: string[];
  @Input() selectedRoles: string[] = [];
  @ViewChild('dt') dt!: Table;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedUser: UserInfo | undefined;
  first = 0;
  rows = 10;
  ICONS: any = ICONS;

  private subscriptions = new Subscription();

  constructor(
    private modalService: BsModalService,
    private router: Router,
    private accService: AccService
  ) {}

  ngOnInit(): void {
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

    // Subscribe to user updates
    this.subscriptions.add(
      this.accService.updatedSource$.subscribe((userId) => {
        this.refreshUser(userId, 'update');
      })
    );

    // Subscribe to user deletions (block/unblock treated as updates)
    this.subscriptions.add(
      this.accService.deletedSource$.subscribe((userId) => {
        this.refreshUser(userId, 'update'); // Treat block/unblock as update
      })
    );
  }

  refreshUser(id: string, action: 'update'): void {
    if (id === '0') {
      // If id is '0', it indicates a full refresh is needed (e.g., after create)
      // Parent component should handle full refresh of users
      return;
    }

    // Check if the user exists in the current users list
    const index = this.users.findIndex((user) => user.id === id);
    if (index === -1) return; // User not in this list, no action needed

    // Fetch the updated user
    this.accService.getUserById(id).subscribe({
      next: (updatedUser) => {
        this.users[index] = updatedUser;
        this.users = [...this.users]; // Trigger change detection
      },
      error: (error) => {
        console.error('Failed to refresh user: ', error);
      },
    });
  }

  onUpdateUserModal(id: string): void {
    this.modalService.show(UpdateAccComponent, {
      class: 'modal-md',
      initialState: { id },
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

  private blockOrUnblock(isActive: boolean): string {
    return isActive ? 'Bloquear' : 'Desbloquear';
  }
  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
