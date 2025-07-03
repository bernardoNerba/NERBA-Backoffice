import { Component, OnInit } from '@angular/core';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Person } from '../../../core/models/person';
import {
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  map,
  Observable,
  startWith,
} from 'rxjs';
import { PeopleService } from '../../../core/services/people.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { CreatePeopleComponent } from '../create-people/create-people.component';
import { UpdatePeopleComponent } from '../update-people/update-people.component';
import { DeletePeopleComponent } from '../delete-people/delete-people.component';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { ICONS } from '../../../core/objects/icons';

@Component({
  selector: 'app-index-people',
  imports: [
    CommonModule,
    SpinnerComponent,
    RouterLink,
    ReactiveFormsModule,
    IconComponent,
  ],
  templateUrl: './index-people.component.html',
  styleUrl: './index-people.component.css',
})
export class IndexPeopleComponent implements OnInit {
  people$!: Observable<Person[]>;
  loading$!: Observable<boolean>;
  columns = ['#', 'Nome', 'NIF', 'Idade', 'Habilitações', 'Email', 'Tel.'];
  filteredPeople$!: Observable<Person[]>;
  ICONS = ICONS;

  searchControl = new FormControl('');

  constructor(
    private peopleService: PeopleService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {
    this.people$ = this.peopleService.people$;
    this.loading$ = this.peopleService.loading$;

    this.updateBreadcrumbs();
  }

  ngOnInit(): void {
    this.filteredPeople$ = combineLatest([
      this.people$,
      this.searchControl.valueChanges.pipe(
        startWith(''),
        debounceTime(100),
        distinctUntilChanged()
      ),
      // add other streams to query
    ]).pipe(
      map(([people, searchTerm]) => {
        if (!people) return [];

        const term = (searchTerm || '').toLowerCase();
        // make them a term

        return people.filter((person) => {
          const matchesSearch =
            person.fullName?.toLowerCase().includes(term) ||
            person.nif?.includes(term);

          // check if matches
          return matchesSearch;
        });
      }),
      startWith([])
    );
  }

  onAddPersonModal() {
    this.modalService.show(CreatePeopleComponent, {
      initialState: {},
      class: 'modal-lg',
    });
  }

  onUpdatePersonModal(person: Person) {
    const initialState = {
      id: person.id,
    };
    this.modalService.show(UpdatePeopleComponent, {
      initialState: initialState,
      class: 'modal-lg',
    });
  }

  onDeletePersonModal(id: number, fullName: string) {
    const initialState = {
      id: id,
      fullName: fullName,
    };
    this.modalService.show(DeletePeopleComponent, { initialState });
  }

  private updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/people',
        displayName: 'Pessoas',
        className: 'inactive',
      },
    ]);
  }
}
