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
import { Module } from '../../../../core/models/module';
import { MenuItem } from 'primeng/api';
import { BsModalService } from 'ngx-bootstrap/modal';
import { DeleteModulesComponent } from '../../../../features/modules/delete-modules/delete-modules.component';
import { ModulesService } from '../../../../core/services/modules.service';
import { Subscription } from 'rxjs';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { UpsertModulesComponent } from '../../../../features/modules/upsert-modules/upsert-modules.component';

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
  @Input({ required: true }) modules!: Module[];
  @Input({ required: true }) loading!: boolean;
  first = 0;
  rows = 10;

  private subscriptions = new Subscription();

  constructor(
    private modalService: BsModalService,
    private modulesService: ModulesService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.menuItems = [
      {
        label: 'Opções',
        items: [
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
        ],
      },
    ];

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
      // Parent component should handle full refresh of course.modules
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

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
