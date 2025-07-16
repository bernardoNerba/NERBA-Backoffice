import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Person } from '../../../core/models/person';
import { Observable } from 'rxjs';
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
})
export class IndexPeopleComponent implements OnInit {
  people$!: Observable<Person[]>;
  loading$!: Observable<boolean>;
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
