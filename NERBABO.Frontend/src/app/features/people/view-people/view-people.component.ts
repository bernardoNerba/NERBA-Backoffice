import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { Observable, of, Subscription } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { PeopleService } from '../../../core/services/people.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Person } from '../../../core/models/person';
import { SharedService } from '../../../core/services/shared.service';
import { DeletePeopleComponent } from '../delete-people/delete-people.component';
import { ICONS } from '../../../core/objects/icons';
import { MenuItem } from 'primeng/api';
import { DropdownMenuComponent } from '../../../shared/components/dropdown-menu/dropdown-menu.component';
import { IView } from '../../../core/interfaces/IView';
import { UpsertPeopleComponent } from '../upsert-people/upsert-people.component';
import { UpsertTeachersComponent } from '../../teachers/upsert-teachers/upsert-teachers.component';
import { UpsertAccComponent } from '../../acc/upsert-acc/upsert-acc.component';
import { UpsertStudentsComponent } from '../../students/upsert-students/upsert-students.component';
import { NavHeaderComponent } from '../../../shared/components/nav-header/nav-header.component';

@Component({
  selector: 'app-detail-person',
  standalone: true,
  imports: [
    CommonModule,
    DropdownMenuComponent,
    RouterModule,
    NavHeaderComponent,
  ],
  templateUrl: './view-people.component.html',
})
export class ViewPeopleComponent implements IView, OnInit, OnDestroy {
  person$?: Observable<Person | null>;
  selectedId!: number;
  fullName!: string;
  id!: number;
  ICONS = ICONS;
  menuItems: MenuItem[] | undefined;

  isStudent: boolean = false;
  isTeacher: boolean = false;
  isColaborator: boolean = false;

  subscriptions: Subscription = new Subscription();

  constructor(
    private route: ActivatedRoute,
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

    this.initializeEntity();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
  }

  onUpdateModal() {
    const initialState = {
      id: this.id,
    };
    this.bsModalService.show(UpsertPeopleComponent, {
      initialState: initialState,
      class: 'modal-lg',
    });
  }

  onDeleteModal() {
    const initialState = {
      id: this.id,
      fullName: this.fullName,
    };
    this.bsModalService.show(DeletePeopleComponent, { initialState });
  }

  onCreateColaborator() {
    this.bsModalService.show(UpsertAccComponent, {
      initialState: {
        id: 0,
        personId: this.id,
      },
      class: 'modal-md',
    });
  }

  onCreateStudent() {
    this.bsModalService.show(UpsertStudentsComponent, {
      initialState: {
        id: 0,
        personId: this.id,
      },
      class: 'modal-lg',
    });
  }

  onCreateTeacher() {
    this.bsModalService.show(UpsertTeachersComponent, {
      initialState: {
        id: 0,
        personId: this.id,
      },
      class: 'modal-lg',
    });
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
            : this.fullName || 'Detalhes',
        url: `/people/${this.id}`,
        className: 'inactive',
      },
    ]);
  }

  initializeEntity() {
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
          console.log(person);
          this.fullName = person.fullName;
          this.id = person.id;
          this.isColaborator = person.isColaborator ?? false;
          this.isStudent = person.isStudent ?? false;
          this.isTeacher = person.isTeacher ?? false;
          this.updateBreadcrumbs();
          this.populateMenu();
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

    // Conditionally add "Criar como ..." options based on person properties
    if (!this.isColaborator) {
      items.push({
        label: 'Criar como Colaborador',
        icon: 'pi pi-plus',
        command: () => this.onCreateColaborator(),
      });
    }

    if (!this.isStudent) {
      items.push({
        label: 'Criar como Formando',
        icon: 'pi pi-plus',
        command: () => this.onCreateStudent(),
      });
    }

    if (!this.isTeacher) {
      items.push({
        label: 'Criar como Formador',
        icon: 'pi pi-plus',
        command: () => this.onCreateTeacher(),
      });
    }

    this.menuItems = [
      {
        label: 'Opções',
        items,
      },
    ];
  }

  updateSourceSubscription() {
    this.subscriptions.add(
      this.peopleService.updatedSource$.subscribe((updatedId: number) => {
        if (this.id === updatedId) {
          this.initializeEntity();
        }
      })
    );
  }

  deleteSourceSubscription() {
    this.subscriptions.add(
      this.peopleService.deletedSource$.subscribe((deletedId: number) => {
        if (this.id === deletedId) {
          this.router.navigateByUrl('/people');
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
