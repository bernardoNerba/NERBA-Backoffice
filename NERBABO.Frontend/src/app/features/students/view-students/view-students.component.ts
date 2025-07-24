import { Component, OnInit } from '@angular/core';
import { IView } from '../../../core/interfaces/IView';
import { MenuItem } from 'primeng/api';
import { catchError, Observable, of, Subscription, tap } from 'rxjs';
import { Student } from '../../../core/models/student';
import { Person } from '../../../core/models/person';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StudentsService } from '../../../core/services/students.service';
import { SharedService } from '../../../core/services/shared.service';
import { PeopleService } from '../../../core/services/people.service';
import { CommonModule } from '@angular/common';
import { DropdownMenuComponent } from '../../../shared/components/dropdown-menu/dropdown-menu.component';
import { NavHeaderComponent } from '../../../shared/components/nav-header/nav-header.component';
import { Company } from '../../../core/models/company';
import { CompaniesService } from '../../../core/services/companies.service';
import { TruncatePipe } from '../../../shared/pipes/truncate.pipe';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { UpsertStudentsComponent } from '../upsert-students/upsert-students.component';

@Component({
  selector: 'app-view-students',
  imports: [
    CommonModule,
    DropdownMenuComponent,
    NavHeaderComponent,
    TruncatePipe,
  ],
  templateUrl: './view-students.component.html',
})
export class ViewStudentsComponent implements IView, OnInit {
  student$?: Observable<Student | null>;
  person$?: Observable<Person | null>;
  company$?: Observable<Company | null>;
  id!: number; // person id
  studentId!: number;
  fullName: string = '';

  menuItems: MenuItem[] | undefined;

  subscriptions: Subscription = new Subscription();

  constructor(
    private modalService: BsModalService,
    private route: ActivatedRoute,
    private router: Router,
    private studentsService: StudentsService,
    private peopleService: PeopleService,
    private sharedService: SharedService,
    private companiesService: CompaniesService
  ) {}

  ngOnInit(): void {
    const personId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(personId ?? '');

    if (isNaN(this.id)) {
      this.router.navigate(['/people']);
      return;
    }

    this.initializeEntity();
    this.initializePerson();
    this.populateMenu();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
  }

  initializeEntity(): void {
    this.student$ = this.studentsService.getByPersonId(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/people', this.id]);
          this.sharedService.showWarning('Informação não foi encontrada.');
        }
        return of(null);
      }),
      tap((student) => {
        if (student) {
          this.studentId = student.id;
          this.initializeCompany(student.companyId ?? 0);
        }
      })
    );
  }

  initializePerson(): void {
    this.person$ = this.peopleService.getSinglePerson(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/people']);
          this.sharedService.showWarning('Informação não foi encontrada.');
        }
        return of(null);
      }),
      tap((person) => {
        if (person) {
          this.fullName = person.fullName;
          this.updateBreadcrumbs();
        }
      })
    );
  }

  initializeCompany(companyId: number): void {
    this.company$ = this.companiesService.getCompanyById(companyId);
  }

  populateMenu(): void {
    const items: MenuItem[] = [
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
    ];

    this.menuItems = [{ label: 'Opções', items }];
  }

  onUpdateModal(): void {
    this.modalService.show(UpsertStudentsComponent, {
      initialState: {
        id: this.studentId,
        personId: this.id,
      },
      class: 'modal-lg',
    });
  }

  updateSourceSubscription(): void {
    this.subscriptions.add(
      this.studentsService.updatedSource$.subscribe((updatedId: number) => {
        if (this.studentId === updatedId) {
          this.initializeEntity();
          this.initializePerson();
        }
      })
    );
  }

  onDeleteModal(): void {
    throw new Error('Method not implemented.');
  }

  deleteSourceSubscription(): void {
    this.subscriptions.add(
      this.studentsService.deletedSource$.subscribe((deletedId: number) => {
        if (this.studentId === deletedId) {
          this.router.navigateByUrl('/people/' + this.id);
        }
      })
    );
  }

  updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        displayName: 'Dashboard',
        url: '/dashboard',
        className: '',
      },
      {
        displayName: 'Pessoas',
        url: '/people',
        className: '',
      },
      {
        displayName:
          this.fullName.length > 21
            ? this.fullName.substring(0, 21) + '...'
            : this.fullName || 'Detalhes Pessoa',
        url: `/people/${this.id}`,
        className: '',
      },
      {
        displayName: 'Detalhes Formando',
        url: `/people/${this.id}/student/`,
        className: 'inactive',
      },
    ]);
  }
}
