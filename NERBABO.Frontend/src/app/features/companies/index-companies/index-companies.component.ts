import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CompaniesService } from '../../../core/services/companies.service';
import { SharedService } from '../../../core/services/shared.service';
import { Company } from '../../../core/models/company';
import { Observable, combineLatest, map } from 'rxjs';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { BsModalService } from 'ngx-bootstrap/modal';
import { CreateCompaniesComponent } from '../create-companies/create-companies.component';
import { ICONS } from '../../../core/objects/icons';
import { debounceTime, distinctUntilChanged, startWith } from 'rxjs/operators';
import { CompaniesTableComponent } from '../../../shared/components/tables/companies-table/companies-table.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';

@Component({
  selector: 'app-index-companies',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CompaniesTableComponent,
    IconComponent,
  ],
  templateUrl: './index-companies.component.html',
  styleUrls: ['./index-companies.component.css'],
})
export class IndexCompaniesComponent implements OnInit {
  companies$!: Observable<Company[]>;
  loading$!: Observable<boolean>;
  filteredCompanies$!: Observable<Company[]>;
  searchControl = new FormControl('');
  ICONS = ICONS;

  constructor(
    private readonly companiesService: CompaniesService,
    private readonly sharedService: SharedService,
    private readonly modalService: BsModalService
  ) {
    this.companies$ = this.companiesService.companies$;
    this.loading$ = this.companiesService.loading$;
  }

  ngOnInit(): void {
    this.filteredCompanies$ = combineLatest([
      this.companies$,
      this.searchControl.valueChanges.pipe(
        startWith(''),
        debounceTime(300),
        distinctUntilChanged()
      ),
    ]).pipe(
      map(([companies, search]) => {
        if (!search) return companies;
        const lowerSearch = search.toLowerCase();
        return companies.filter(
          (company) =>
            company.name?.toLowerCase().includes(lowerSearch) ||
            company.phoneNumber?.toLowerCase().includes(lowerSearch) ||
            company.email?.toLowerCase().includes(lowerSearch) ||
            company.ativitySector?.toLowerCase().includes(lowerSearch) ||
            company.size?.toLowerCase().includes(lowerSearch)
        );
      })
    );
    this.updateBreadcrumbs();
  }

  onAddCompanyModal() {
    this.modalService.show(CreateCompaniesComponent, {
      initialState: {},
      class: 'modal-lg',
    });
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
