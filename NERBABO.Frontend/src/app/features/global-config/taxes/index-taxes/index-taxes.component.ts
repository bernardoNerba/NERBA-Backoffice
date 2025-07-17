import { Component, OnInit, ViewChild } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { DropdownModule } from 'primeng/dropdown';
import { Tax } from '../../../../core/models/tax';
import { MenuItem } from 'primeng/api';
import { Observable, Subscription } from 'rxjs';
import { BsModalService } from 'ngx-bootstrap/modal';
import { ConfigService } from '../../../../core/services/config.service';
import { SharedService } from '../../../../core/services/shared.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TaxType } from '../../../../core/objects/taxType';
import { CreateTaxesComponent } from '../create-taxes/create-taxes.component';
import { UpdateTaxesComponent } from '../update-taxes/update-taxes.component';
import { DeleteTaxesComponent } from '../delete-taxes/delete-taxes.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { IIndex } from '../../../../core/interfaces/iindex';

interface TaxTypeFilter {
  label: string;
  value: string | null;
}

@Component({
  selector: 'app-index-taxes',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    Button,
    Menu,
    DropdownModule,
    CommonModule,
    FormsModule,
    SpinnerComponent,
    InputTextModule,
    TagModule,
  ],
  templateUrl: './index-taxes.component.html',
})
export class IndexTaxesComponent implements IIndex, OnInit {
  @ViewChild('dt') dt!: Table;
  taxes: Tax[] = [];
  filteredTaxes: Tax[] = [];
  loading$!: Observable<boolean>;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedTax: Tax | undefined;
  taxTypeFilter: string | null = null;
  taxTypeOptions: TaxTypeFilter[] = [
    { label: 'Todas', value: null },
    { label: 'IVA', value: TaxType.Iva },
    { label: 'IRS', value: TaxType.Irs },
  ];
  first = 0;
  rows = 10;
  private subscriptions = new Subscription();

  constructor(
    private configService: ConfigService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.loading$ = this.configService.loading$;
    this.subscriptions.add(
      this.configService.taxes$.subscribe((taxes) => {
        this.taxes = taxes;
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
            command: () => this.onUpdateModal(this.selectedTax!),
          },
          {
            label: 'Eliminar',
            icon: 'pi pi-exclamation-triangle',
            command: () =>
              this.onDeleteModal(this.selectedTax!.id, this.selectedTax!.name),
          },
          {
            label: 'Ativar | Desativar',
            icon: 'pi pi-power-off',
            command: () => this.onToggleTaxStatus(this.selectedTax!),
          },
        ],
      },
    ];

    this.updateBreadcrumbs();
  }

  applyFilters(): void {
    let filtered = [...this.taxes];

    // Apply tax type filter
    if (this.taxTypeFilter) {
      filtered = filtered.filter((tax) => tax.type === this.taxTypeFilter);
    }

    // Apply search filter
    if (this.searchValue) {
      const term = this.searchValue.toLowerCase();
      filtered = filtered.filter(
        (tax) =>
          tax.name.toLowerCase().includes(term) ||
          tax.type.toLowerCase().includes(term)
      );
    }

    this.filteredTaxes = filtered;
  }

  onCreateModal() {
    this.modalService.show(CreateTaxesComponent, {
      initialState: {},
      class: 'modal-md',
    });
  }

  onUpdateModal(tax: Tax) {
    this.modalService.show(UpdateTaxesComponent, {
      initialState: { currentTax: tax, type: tax.type as TaxType },
      class: 'modal-md',
    });
  }

  onDeleteModal(id: number, name: string) {
    this.modalService.show(DeleteTaxesComponent, {
      initialState: { id, name },
      class: 'modal-md',
    });
  }

  getTypeColor(type: string): string {
    if (type === 'IVA') return 'info';
    return 'secondary';
  }

  getStatusSeverity(status: boolean) {
    return status ? 'success' : 'danger';
  }

  getStatusIcon(status: boolean) {
    return status ? 'pi pi-check' : 'pi pi-times';
  }

  onToggleTaxStatus(tax: Tax) {
    const updatedTax = { ...tax, isActive: !tax.isActive };
    this.configService.updateIvaTax(updatedTax).subscribe({
      next: (value) => {
        this.configService.triggerFetchConfigs();
        this.sharedService.showSuccess(value.message);
      },
      error: (error) => {
        this.sharedService.handleErrorResponse(error);
      },
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
    return this.filteredTaxes
      ? this.first + this.rows >= this.filteredTaxes.length
      : true;
  }

  isFirstPage(): boolean {
    return this.filteredTaxes ? this.first === 0 : true;
  }

  clearFilters() {
    this.searchValue = '';
    this.taxTypeFilter = null;
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
        url: '/taxes',
        displayName: 'Taxas',
        className: 'inactive',
      },
    ]);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
