import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CompaniesService } from '../../../core/services/companies.service';
import { SharedService } from '../../../core/services/shared.service';
import { Company } from '../../../core/models/company';
import { Observable } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { CustomModalService } from '../../../core/services/custom-modal.service';
import { ICONS } from '../../../core/objects/icons';
import { CompaniesTableComponent } from '../../../shared/components/tables/companies-table/companies-table.component';
import { IIndex } from '../../../core/interfaces/IIndex';
import { UpsertCompaniesComponent } from '../upsert-companies/upsert-companies.component';
import { TitleComponent } from '../../../shared/components/title/title.component';

@Component({
  selector: 'app-index-companies',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CompaniesTableComponent,
    TitleComponent,
  ],
  templateUrl: './index-companies.component.html',
})
export class IndexCompaniesComponent implements IIndex, OnInit {
  companies$!: Observable<Company[]>;
  loading$!: Observable<boolean>;
  ICONS = ICONS;

  constructor(
    private readonly companiesService: CompaniesService,
    private readonly sharedService: SharedService,
    private readonly modalService: CustomModalService
  ) {
    this.companies$ = this.companiesService.companies$;
    this.loading$ = this.companiesService.loading$;
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
  }

  onCreateModal(): void {
    this.modalService.show(UpsertCompaniesComponent, {
      initialState: { id: 0 },
      class: 'modal-lg',
    });
  }

  updateBreadcrumbs(): void {
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
