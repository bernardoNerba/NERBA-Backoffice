import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, of, Subscription } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { PeopleService } from '../../../core/services/people.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Person } from '../../../core/models/person';
import { SharedService } from '../../../core/services/shared.service';
import { UpdatePeopleComponent } from '../update-people/update-people.component';
import { DeletePeopleComponent } from '../delete-people/delete-people.component';

@Component({
  selector: 'app-detail-person',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './view-people.component.html',
  styleUrl: './view-people.component.css',
})
export class ViewPeopleComponent implements OnInit, OnDestroy {
  person$?: Observable<Person | null>;
  selectedId!: number;
  fullName!: string;
  id!: number;

  private subscriptions: Subscription = new Subscription();

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

    this.initializePerson();
    this.updateSourceSubcription();
    this.deleteSourceSubscription();
  }

  updatePersonModal() {
    const initialState = {
      id: this.id,
    };
    this.bsModalService.show(UpdatePeopleComponent, {
      initialState: initialState,
      class: 'modal-lg',
    });
  }

  deletePersonModal() {
    const initialState = {
      id: this.id,
      fullName: this.fullName,
    };
    this.bsModalService.show(DeletePeopleComponent, { initialState });
  }

  private updateBreadcrumbs(id: number, fullName: string): void {
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
        displayName: fullName || 'Detalhes',
        url: `/people/${id}`,
        className: 'inactive',
      },
    ]);
  }

  private initializePerson() {
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
          this.updateBreadcrumbs(person.id, person.fullName);
        }
      })
    );
  }

  private updateSourceSubcription() {
    this.subscriptions.add(
      this.peopleService.updatedSource$.subscribe((updatedId: number) => {
        if (this.id === updatedId) {
          this.initializePerson();
        }
      })
    );
  }

  private deleteSourceSubscription() {
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
