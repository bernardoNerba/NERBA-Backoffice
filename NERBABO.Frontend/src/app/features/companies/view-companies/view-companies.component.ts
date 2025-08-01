import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { catchError, Observable, of, Subscription, tap } from 'rxjs';
import { Company } from '../../../core/models/company';
import { CompaniesService } from '../../../core/services/companies.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ICONS } from '../../../core/objects/icons';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { Student } from '../../../core/models/student';
import { BsModalService } from 'ngx-bootstrap/modal';
import { DeleteCompaniesComponent } from '../delete-companies/delete-companies.component';
import { MenuItem } from 'primeng/api';
import { UpsertCompaniesComponent } from '../upsert-companies/upsert-companies.component';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { IView } from '../../../core/interfaces/IView';

@Component({
  selector: 'app-view-companies',
  imports: [CommonModule, IconComponent, RouterLink, TitleComponent],
  templateUrl: './view-companies.component.html',
})
export class ViewCompaniesComponent implements IView, OnInit, OnDestroy {
  @Input({ required: true }) id!: number;
  company$?: Observable<Company | null>;
  students$!: Observable<Student[] | []>;
  name?: string;
  ICONS = ICONS;
  hasStudents: boolean = false;
  menuItems: MenuItem[] | undefined;

  subscriptions: Subscription = new Subscription();

  constructor(
    private companiesService: CompaniesService,
    private sharedService: SharedService,
    private route: ActivatedRoute,
    private router: Router,
    private modalService: BsModalService
  ) {}

  ngOnInit(): void {
    const companyId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(companyId ?? '');

    if (isNaN(this.id)) {
      this.router.navigate(['/companies']);
      return;
    }

    this.initializeEntity();
    this.initializeStudentsFromCompany();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
  }

  onUpdateModal() {
    const initialState = {
      id: this.id,
    };
    this.modalService.show(UpsertCompaniesComponent, {
      initialState: initialState,
      class: 'modal-lg',
    });
  }

  onDeleteModal() {
    const initialState = {
      id: this.id,
      name: this.name,
    };
    this.modalService.show(DeleteCompaniesComponent, { initialState });
  }

  initializeEntity() {
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
          this.updateBreadcrumbs();
          this.populateMenu();
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

  populateMenu(): void {
    this.menuItems = [
      {
        label: 'Opções',
        items: [
          {
            label: 'Editar',
            icon: 'pi pi-pencil',
            command: () => this.onUpdateModal(),
          },
          {
            label: 'Eliminar',
            icon: 'pi pi-exclamation-triangle',
            command: () => this.onDeleteModal(),
          },
        ],
      },
    ];
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
        className: '',
      },
      {
        url: `/companies/${this.id}`,
        displayName: this.name?.substring(0, 21) + '...' || 'Detalhes',
        className: 'inactive',
      },
    ]);
  }

  updateSourceSubscription() {
    this.subscriptions.add(
      this.companiesService.updatedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeEntity();
          this.initializeStudentsFromCompany();
        }
      })
    );
  }

  deleteSourceSubscription() {
    this.subscriptions.add(
      this.companiesService.deletedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this, this.router.navigateByUrl('/companies');
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
