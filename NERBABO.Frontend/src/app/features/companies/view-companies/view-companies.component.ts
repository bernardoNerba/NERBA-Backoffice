import { Component, Input, OnInit } from '@angular/core';
import { catchError, Observable, of, tap } from 'rxjs';
import { Company } from '../../../core/models/company';
import { CompaniesService } from '../../../core/services/companies.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ICONS } from '../../../core/objects/icons';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { Student } from '../../../core/models/student';

@Component({
  selector: 'app-view-companies',
  imports: [CommonModule, IconComponent],
  templateUrl: './view-companies.component.html',
  styleUrl: './view-companies.component.css',
})
export class ViewCompaniesComponent implements OnInit {
  @Input({ required: true }) id!: number;
  selectedId!: number;
  company$?: Observable<Company | null>;
  students$!: Observable<Student[] | []>;
  name?: string;
  ICONS = ICONS;
  hasStudents: boolean = false;

  constructor(
    private companiesService: CompaniesService,
    private sharedService: SharedService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const companyId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(companyId ?? '');

    if (isNaN(this.id)) {
      this.router.navigate(['/companies']);
      return;
    }

    this.initializeCompany();
    this.initializeStudentsFromCompany();
  }

  private initializeCompany() {
    this.company$ = this.companiesService.getCompanyById(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/companies']);
          this.sharedService.showWarning('Informação não foi encontrada.');
        }
        return of(null);
      }),
      tap((company) => {
        if (company) {
          this.id = company.id;
          this.name = company.name;
          this.updateBreadcrumbs(this.id, this.name);
        }
      })
    );
  }

  private initializeStudentsFromCompany() {
    this.students$ = this.companiesService.getStudentsByCompanyId(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        }
        this.hasStudents = false;
        return of([]);
      }),
      tap((students) => {
        if (students.length != 0) {
          this.hasStudents = true;
        }
      })
    );
  }

  private updateBreadcrumbs(id: number, name: string): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/companies',
        displayName: 'Empresas',
        className: '',
      },
      {
        url: `/companies/${id}`,
        displayName: name.substring(0, 21) + '...' || 'Detalhes',
        className: 'inactive',
      },
    ]);
  }
}
