import { Component, OnInit } from '@angular/core';
import { IView } from '../../../core/interfaces/IView';
import { TeachersService } from '../../../core/services/teachers.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SharedService } from '../../../core/services/shared.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { MenuItem } from 'primeng/api';
import { catchError, Observable, of, Subscription, tap } from 'rxjs';
import { Teacher } from '../../../core/models/teacher';
import { ICONS } from '../../../core/objects/icons';
import { CommonModule } from '@angular/common';
import { Person } from '../../../core/models/person';
import { PeopleService } from '../../../core/services/people.service';
import { DropdownMenuComponent } from '../../../shared/components/dropdown-menu/dropdown-menu.component';
import { UpsertTeachersComponent } from '../upsert-teachers/upsert-teachers.component';
import { DeleteTeachersComponent } from '../delete-teachers/delete-teachers.component';
import { NavHeaderComponent } from '../../../shared/components/nav-header/nav-header.component';
import { TitleComponent } from '../../../shared/components/title/title.component';

@Component({
  selector: 'app-view-teachers',
  imports: [CommonModule, NavHeaderComponent, TitleComponent],
  templateUrl: './view-teachers.component.html',
})
export class ViewTeachersComponent implements IView, OnInit {
  teacher$?: Observable<Teacher | null>;
  person$?: Observable<Person | null>;
  id!: number; // person id
  teacherId!: number;
  fullName: string = '';
  menuItems: MenuItem[] | undefined;

  isStudent: boolean = false;
  isTeacher: boolean = false;
  isColaborator: boolean = false;

  ICONS = ICONS;

  subscriptions: Subscription = new Subscription();

  constructor(
    private route: ActivatedRoute,
    private teacherService: TeachersService,
    private peopleService: PeopleService,
    private router: Router,
    private sharedService: SharedService,
    private bsModalService: BsModalService
  ) {}

  ngOnInit(): void {
    const personId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(personId ?? '');

    if (isNaN(this.id)) {
      this.router.navigate(['/people']);
      return;
    }

    this.initializePerson();
    this.initializeEntity();
    this.populateMenu();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
  }

  initializeEntity(): void {
    this.teacher$ = this.teacherService.getTeacherByPersonId(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/people', this.id]);
          this.sharedService.showWarning('Informação não foi encontrada.');
        }
        return of(null);
      }),
      tap((teacher) => {
        if (teacher) {
          this.teacherId = teacher.id;
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
    const initialState = {
      id: this.teacherId,
      personId: this.id,
    };
    this.bsModalService.show(UpsertTeachersComponent, {
      initialState: initialState,
      class: 'modal-lg',
    });
  }

  updateSourceSubscription(): void {
    this.subscriptions.add(
      this.teacherService.updatedSource$.subscribe((teacher) => {
        if (this.teacherId === teacher?.id) {
          this.initializeEntity();
          this.initializePerson();
        }
      })
    );
  }

  onDeleteModal(): void {
    this.bsModalService.show(DeleteTeachersComponent, {
      initialState: {
        id: this.teacherId,
        fullName: this.fullName,
      },
    });
  }

  deleteSourceSubscription(): void {
    this.subscriptions.add(
      this.teacherService.deletedSource$.subscribe((deletedId: number) => {
        if (this.teacherId === deletedId) {
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
        displayName: 'Detalhes Formador',
        url: `/people/${this.id}/teacher/`,
        className: 'inactive',
      },
    ]);
  }
}
