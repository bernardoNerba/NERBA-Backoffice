import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CompaniesService } from '../../../core/services/companies.service';
import { SharedService } from '../../../core/services/shared.service';
import { Company } from '../../../core/models/company';
import { Observable } from 'rxjs';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { ICONS } from '../../../core/objects/icons';
import { BsModalService } from 'ngx-bootstrap/modal';
import { CreateCompaniesComponent } from '../create-companies/create-companies.component';
import { UpdateCompaniesComponent } from '../update-companies/update-companies.component';
import { DeleteCompaniesComponent } from '../delete-companies/delete-companies.component';
import { TruncatePipe } from '../../../shared/pipes/truncate.pipe';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-index-companies',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SpinnerComponent,
    IconComponent,
    TruncatePipe,
    RouterLink,
  ],
  templateUrl: './index-companies.component.html',
  styleUrl: './index-companies.component.css',
})
export class IndexCompaniesComponent implements OnInit {
  companies$!: Observable<Company[]>;
  loading$!: Observable<boolean>;
  filteredCompanies$!: Observable<Company[]>;
  searchControl = new FormControl('');
  ICONS = ICONS;
  columns = [
    '#',
    'Designação',
    'Tel.',
    'Email',
    'Setor de Atividade',
    'Tamanho',
  ];

  constructor(
    private readonly companiesService: CompaniesService,
    private readonly sharedService: SharedService,
    private readonly modalService: BsModalService
  ) {
    this.companies$ = this.companiesService.companies$;
    this.loading$ = this.companiesService.loading$;
  }
  ngOnInit(): void {
    this.filteredCompanies$ = this.companies$;
    this.updateBreadcrumbs();
  }

  onAddCompanyModal() {
    this.modalService.show(CreateCompaniesComponent, {
      initialState: {},
      class: 'modal-lg',
    });
  }

  onUpdateCompanyModal(companyId: number) {
    const initialState = {
      id: companyId,
    };
    this.modalService.show(UpdateCompaniesComponent, {
      initialState: initialState,
      class: 'modal-lg',
    });
  }

  onDeleteCompanyModal(id: number, name: string) {
    const initialState = {
      id: id,
      name: name,
    };
    this.modalService.show(DeleteCompaniesComponent, { initialState });
  }

  private updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/companies',
        displayName: 'Empresas',
        className: 'inactive',
      },
    ]);
  }
}
