import { Component, OnInit, ViewChild } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { Observable, Subscription } from 'rxjs';
import { CustomModalService } from '../../../../core/services/custom-modal.service';
import { ConfigService } from '../../../../core/services/config.service';
import { SharedService } from '../../../../core/services/shared.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { IIndex } from '../../../../core/interfaces/IIndex';
import { UpsertModuleCategoriesComponent } from '../upsert-module-categories/upsert-module-categories.component';
import { DeleteModuleCategoriesComponent } from '../delete-module-categories/delete-module-categories.component';
import { ModuleCategory } from '../module-category.model';

@Component({
  selector: 'app-index-module-categories',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    Button,
    Menu,
    CommonModule,
    FormsModule,
    SpinnerComponent,
    InputTextModule,
    TagModule,
  ],
  templateUrl: './index-module-categories.component.html',
})
export class IndexModuleCategoriesComponent implements IIndex, OnInit {
  @ViewChild('dt') dt!: Table;
  moduleCategories: ModuleCategory[] = [];
  filteredModuleCategories: ModuleCategory[] = [];
  loading$!: Observable<boolean>;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedModuleCategory: ModuleCategory | undefined;
  first = 0;
  rows = 10;
  private subscriptions = new Subscription();

  constructor(
    private configService: ConfigService,
    private modalService: CustomModalService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.loading$ = this.configService.loading$;
    this.subscriptions.add(
      this.configService.moduleCategories$.subscribe((moduleCategories) => {
        this.moduleCategories = moduleCategories;
        this.applyFilters();
      })
    );
    this.configService.triggerFetchConfigs();

    this.menuItems = [
      {
        label: 'Opções',
        items: [
          {
            label: 'Editar',
            icon: 'pi pi-pencil',
            command: () => this.onUpdateModal(this.selectedModuleCategory!),
          },
          {
            label: 'Eliminar',
            icon: 'pi pi-exclamation-triangle',
            command: () =>
              this.onDeleteModal(this.selectedModuleCategory!.id, this.selectedModuleCategory!.name),
          },
        ],
      },
    ];

    this.updateBreadcrumbs();
  }

  applyFilters(): void {
    let filtered = [...this.moduleCategories];

    if (this.searchValue) {
      const term = this.searchValue.toLowerCase();
      filtered = filtered.filter(
        (category) =>
          category.name.toLowerCase().includes(term) ||
          category.shortenName.toLowerCase().includes(term)
      );
    }

    this.filteredModuleCategories = filtered;
  }

  onCreateModal() {
    this.modalService.show(UpsertModuleCategoriesComponent, {
      initialState: { id: 0 },
      class: 'modal-md',
    });
  }

  onUpdateModal(category: ModuleCategory) {
    this.modalService.show(UpsertModuleCategoriesComponent, {
      initialState: { id: category.id, currentCategory: category },
      class: 'modal-md',
    });
  }

  onDeleteModal(id: number, name: string) {
    this.modalService.show(DeleteModuleCategoriesComponent, {
      initialState: { id, name },
      class: 'modal-md',
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
    return this.filteredModuleCategories
      ? this.first + this.rows >= this.filteredModuleCategories.length
      : true;
  }

  isFirstPage(): boolean {
    return this.filteredModuleCategories ? this.first === 0 : true;
  }

  clearFilters() {
    this.searchValue = '';
    this.applyFilters();
    this.dt.reset();
  }

  updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/module-categories',
        displayName: 'Categorias de Módulos',
        className: 'inactive',
      },
    ]);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}