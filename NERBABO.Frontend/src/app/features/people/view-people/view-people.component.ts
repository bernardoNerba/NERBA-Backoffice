import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, of, Subscription } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { PeopleService } from '../../../core/services/people.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Person } from '../../../core/models/person';
import { SharedService } from '../../../core/services/shared.service';
import { DeletePeopleComponent } from '../delete-people/delete-people.component';
import { PersonRelationship } from '../../../core/models/personRelationships';
import { ICONS } from '../../../core/objects/icons';
import { MenuItem } from 'primeng/api';
import { DropdownMenuComponent } from '../../../shared/components/dropdown-menu/dropdown-menu.component';
import { IView } from '../../../core/interfaces/IView';
import { UpsertPeopleComponent } from '../upsert-people/upsert-people.component';

@Component({
  selector: 'app-detail-person',
  standalone: true,
  imports: [CommonModule, DropdownMenuComponent],
  templateUrl: './view-people.component.html',
})
export class ViewPeopleComponent implements IView, OnInit, OnDestroy {
  person$?: Observable<Person | null>;
  relationships$?: Observable<PersonRelationship | null>;
  selectedId!: number;
  fullName!: string;
  id!: number;
  ICONS = ICONS;
  menuItems: MenuItem[] | undefined;

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
        displayName: this.fullName || 'Detalhes',
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
          this.fullName = person.fullName;
          this.id = person.id;
          this.updateBreadcrumbs();
          this.populateMenu();
        }
      })
    );

    this.relationships$ = this.peopleService.getPersonRelationships(this.id);
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
          {
            // TODO: Implement create colaborator from person
            label: 'Criar como Colaborador',
            icon: 'pi pi-plus',
            command: () => {},
          },
          {
            // TODO: Implement create student from person
            label: 'Criar como Formando',
            icon: 'pi pi-plus',
            command: () => {},
          },
          {
            // TODO: Implement create teacher from person
            label: 'Criar como Formador',
            icon: 'pi pi-plus',
            command: () => {},
          },
        ],
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
