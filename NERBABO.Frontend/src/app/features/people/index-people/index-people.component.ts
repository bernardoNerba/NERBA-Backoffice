import { Component, OnInit } from '@angular/core';
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
import { CommonModule } from '@angular/common';
import { ICONS } from '../../../core/objects/icons';
import { PeopleTableComponent } from '../../../shared/components/tables/people-table/people-table.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';

@Component({
  selector: 'app-index-people',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PeopleTableComponent,
    IconComponent,
  ],
  templateUrl: './index-people.component.html',
  styleUrls: ['./index-people.component.css'],
})
export class IndexPeopleComponent implements OnInit {
  people$!: Observable<Person[]>;
  loading$!: Observable<boolean>;
  filteredPeople$!: Observable<Person[]>;
  searchControl = new FormControl('');
  ICONS = ICONS;

  constructor(
    private peopleService: PeopleService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {
    this.people$ = this.peopleService.people$;
    this.loading$ = this.peopleService.loading$;
  }

  ngOnInit(): void {
    this.filteredPeople$ = combineLatest([
      this.people$,
      this.searchControl.valueChanges.pipe(
        startWith(''),
        debounceTime(300),
        distinctUntilChanged()
      ),
    ]).pipe(
      map(([people, searchTerm]) => {
        if (!people) return [];
        const term = (searchTerm || '').toLowerCase();
        return people.filter(
          (person) =>
            person.fullName?.toLowerCase().includes(term) ||
            person.nif?.toLowerCase().includes(term) ||
            person.email?.toLowerCase().includes(term) ||
            person.phoneNumber?.toLowerCase().includes(term) ||
            person.habilitation?.toLowerCase().includes(term)
        );
      }),
      startWith([])
    );
    this.updateBreadcrumbs();
  }

  onAddPersonModal() {
    this.modalService.show(CreatePeopleComponent, {
      initialState: {},
      class: 'modal-lg',
    });
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
