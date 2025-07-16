import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { Company } from '../../../../core/models/company';
import { MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Router } from '@angular/router';
import { CompaniesService } from '../../../../core/services/companies.service';
import { UpdateCompaniesComponent } from '../../../../features/companies/update-companies/update-companies.component';
import { DeleteCompaniesComponent } from '../../../../features/companies/delete-companies/delete-companies.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';

@Component({
  selector: 'app-companies-table',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    Button,
    Menu,
    CommonModule,
    FormsModule,
    TruncatePipe,
    SpinnerComponent,
    InputTextModule,
    TooltipModule,
    IconAnchorComponent,
  ],
  templateUrl: './companies-table.component.html',
  standalone: true,
})
export class CompaniesTableComponent implements OnInit {
  @Input({ required: true }) companies!: Company[];
  @Input({ required: true }) loading!: boolean;
  @ViewChild('dt') dt!: Table;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedCompany: Company | undefined;
  first = 0;
  rows = 10;

  private subscriptions = new Subscription();

  constructor(
    private modalService: BsModalService,
    private router: Router,
    private companiesService: CompaniesService
  ) {}

  ngOnInit(): void {
    this.menuItems = [
      {
        label: 'Opções',
        items: [
          {
            label: 'Editar',
            icon: 'pi pi-pencil',
            command: () => this.onUpdateCompanyModal(this.selectedCompany!),
          },
          {
            label: 'Eliminar',
            icon: 'pi pi-exclamation-triangle',
            command: () => this.onDeleteCompanyModal(this.selectedCompany!),
          },
          {
            label: 'Detalhes',
            icon: 'pi pi-exclamation-circle',
            command: () =>
              this.router.navigateByUrl(
                `/companies/${this.selectedCompany!.id}`
              ),
          },
        ],
      },
    ];

    // Subscribe to company updates
    this.subscriptions.add(
      this.companiesService.updatedSource$.subscribe((companyId) => {
        this.refreshCompany(companyId, 'update');
      })
    );

    // Subscribe to company deletions
    this.subscriptions.add(
      this.companiesService.deletedSource$.subscribe((companyId) => {
        this.refreshCompany(companyId, 'delete');
      })
    );
  }

  refreshCompany(id: number, action: 'update' | 'delete'): void {
    if (id === 0) {
      // If id is 0, it indicates a full refresh is needed (e.g., after create)
      // Parent component should handle full refresh of companies
      return;
    }

    // Check if the company exists in the current companies list
    const index = this.companies.findIndex((company) => company.id === id);
    if (index === -1) return; // Company not in this list, no action needed

    if (action === 'delete') {
      // Remove the company from the list
      this.companies = this.companies.filter((company) => company.id !== id);
    } else if (action === 'update') {
      // Fetch the updated company
      this.companiesService.getCompanyById(id).subscribe({
        next: (updatedCompany) => {
          this.companies[index] = updatedCompany;
          this.companies = [...this.companies]; // Trigger change detection
        },
        error: (error) => {
          console.error('Failed to refresh company: ', error);
        },
      });
    }
  }

  onUpdateCompanyModal(company: Company): void {
    this.modalService.show(UpdateCompaniesComponent, {
      class: 'modal-lg',
      initialState: {
        id: company.id,
      },
    });
  }

  onDeleteCompanyModal(company: Company): void {
    this.modalService.show(DeleteCompaniesComponent, {
      class: 'modal-md',
      initialState: {
        id: company.id,
        name: company.name,
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
    return this.companies
      ? this.first + this.rows >= this.companies.length
      : true;
  }

  isFirstPage(): boolean {
    return this.companies ? this.first === 0 : true;
  }

  clearFilters() {
    this.searchValue = '';
    this.dt.reset();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
