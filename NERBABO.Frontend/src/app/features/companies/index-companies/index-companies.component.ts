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

@Component({
  selector: 'app-index-companies',
  imports: [CommonModule, ReactiveFormsModule, SpinnerComponent, IconComponent],
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
    private readonly sharedService: SharedService
  ) {
    this.companies$ = this.companiesService.comapnies$;
    this.loading$ = this.companiesService.loading$;
  }
  ngOnInit(): void {
    this.filteredCompanies$ = this.companies$;
  }
}
