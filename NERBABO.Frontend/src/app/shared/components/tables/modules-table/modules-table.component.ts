import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { TagModule } from 'primeng/tag';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Module, ModuleTeacher } from '../../../../core/models/module';
import { MenuItem } from 'primeng/api';
import { BsModalService } from 'ngx-bootstrap/modal';
import { DeleteModulesComponent } from '../../../../features/modules/delete-modules/delete-modules.component';
import { ModulesService } from '../../../../core/services/modules.service';
import { Subscription } from 'rxjs';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { UpsertModulesComponent } from '../../../../features/modules/upsert-modules/upsert-modules.component';
import { UpsertModuleTeachingComponent } from '../../../../features/actions/upsert-module-teaching/upsert-module-teaching.component';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-modules-table',
  imports: [
    TableModule,
    CommonModule,
    TruncatePipe,
    TagModule,
    Button,
    Menu,
    IconField,
    InputIcon,
    InputTextModule,
    FormsModule,
    IconAnchorComponent,
    SpinnerComponent,
  ],
  templateUrl: './modules-table.component.html',
})
export class ModulesTableComponent implements OnInit {
  @ViewChild('dt') dt!: Table;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedModule: Module | undefined;
  @Input({ required: true }) modules!: (Module | ModuleTeacher)[];
  @Input({ required: true }) loading!: boolean;
  @Input() showTeacherColumn: boolean = false;
  @Input() actionId?: number;
  @Input() actionTitle?: string;
  first = 0;
  rows = 10;

  private subscriptions = new Subscription();

  constructor(
    private modalService: BsModalService,
    private modulesService: ModulesService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.setupMenuItems();

    // Subscribe to module updates to refresh only the specific module
    this.subscriptions.add(
      this.modulesService.updatedSource$.subscribe((moduleId) => {
        this.refreshModule(moduleId);
      })
    );

    // Subscribe to module toggles
    this.subscriptions.add(
      this.modulesService.toggleSource$.subscribe((moduleId) => {
        this.refreshModule(moduleId);
      })
    );

    // Subscribe to module deletions
    this.subscriptions.add(
      this.modulesService.deletedSource$.subscribe((moduleId) => {
        this.modules = this.modules.filter((module) => module.id !== moduleId);
      })
    );
  }

  columns = [
    { field: 'ufcd', header: 'UFCD' },
    { field: 'hours', header: 'Duração' },
    { field: 'status', header: 'Estado' },
  ];

  private refreshModule(moduleId: number): void {
    if (moduleId === 0) {
      // If moduleId is 0, it indicates a full refresh is needed (e.g., after create)
      this.modulesService.triggerFetch();
      return;
    }

    // If we're in action context (showing teacher column), don't refresh individual modules
    // The parent component (view-actions) should handle the refresh to maintain teacher data
    if (this.actionId && this.showTeacherColumn) {
      // Trigger a general refresh that the parent component can listen to
      this.modulesService.triggerFetch();
      return;
    }

    // Check if the module exists in the current modules list
    this.modulesService.getSingleModule(moduleId).subscribe({
      next: (updatedModule) => {
        const index = this.modules.findIndex(
          (module) => module.id === moduleId
        );
        if (index !== -1) {
          this.modules[index] = updatedModule;
          this.modules = [...this.modules]; // Trigger change detection
        }
      },
      error: (error) => {
        console.error('Failed to refresh module: ', error);

        this.modulesService.triggerFetch();
      },
    });
  }

  onUpdateModal(m: Module): void {
    const initialState = {
      id: m.id,
    };
    this.modalService.show(UpsertModulesComponent, {
      initialState: initialState,
      class: 'modal-md',
    });
  }

  onDeleteModal(id: number, name: string): void {
    const initialState = {
      id: id,
      name: name,
    };
    this.modalService.show(DeleteModulesComponent, {
      initialState: initialState,
      class: 'modal-md',
    });
  }

  onToggleModule(id: number): void {
    this.modulesService.toggleModuleIsActive(id);
  }

  getStatusSeverity(status: boolean) {
    return status ? 'success' : 'danger';
  }

  getStatusIcon(status: boolean) {
    return status ? 'pi pi-check' : 'pi pi-times';
  }

  clearFilters() {
    this.searchValue = '';
    this.dt.reset();
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
    return this.modules ? this.first + this.rows >= this.modules.length : true;
  }

  isFirstPage(): boolean {
    return this.modules ? this.first === 0 : true;
  }

  isModuleTeacher(module: Module | ModuleTeacher): module is ModuleTeacher {
    return 'teacherId' in module;
  }

  private setupMenuItems(): void {
    const baseItems = [
      {
        label: 'Editar',
        icon: 'pi pi-pencil',
        command: () => this.onUpdateModal(this.selectedModule!),
      },
      {
        label: 'Eliminar',
        icon: 'pi pi-exclamation-triangle',
        command: () =>
          this.onDeleteModal(
            this.selectedModule!.id,
            this.selectedModule!.name
          ),
      },
      {
        label: 'Detalhes',
        icon: 'pi pi-exclamation-circle',
        command: () =>
          this.router.navigateByUrl(`/modules/${this.selectedModule!.id}`),
      },
      {
        label: 'Atualizar Estado',
        icon: 'pi pi-refresh',
        command: () => this.onToggleModule(this.selectedModule!.id),
      },
    ];

    // Add action-specific menu items when in action context
    if (this.actionId && this.showTeacherColumn) {
      baseItems.splice(1, 0, {
        label: 'Atualizar Formador',
        icon: 'pi pi-user-edit',
        command: () => this.onUpdateTeacherModal(),
      });
    }

    this.menuItems = [
      {
        label: 'Opções',
        items: baseItems,
      },
    ];
  }

  private onUpdateTeacherModal(): void {
    if (!this.actionId || !this.selectedModule) return;

    const moduleTeacher = this.isModuleTeacher(this.selectedModule)
      ? this.selectedModule
      : null;

    this.modalService.show(UpsertModuleTeachingComponent, {
      class: 'modal-lg',
      initialState: {
        actionId: this.actionId,
        actionTitle: this.actionTitle,
        moduleId: this.selectedModule.id,
        moduleName: this.selectedModule.name,
        teacherId: moduleTeacher?.teacherId || null,
        isUpdate: true,
      },
    });
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
