import { Component, OnDestroy, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { Person } from '../../../core/models/person';
import { Observable, Subscription } from 'rxjs';
import { PeopleService } from '../../../core/services/people.service';
import { CustomModalService } from '../../../core/services/custom-modal.service';
import { SharedService } from '../../../core/services/shared.service';
import { UpsertPeopleComponent } from '../upsert-people/upsert-people.component';
import { ImportPeopleComponent } from '../import-people/import-people.component';
import { CommonModule } from '@angular/common';
import { ICONS } from '../../../core/objects/icons';
import { PeopleTableComponent } from '../../../shared/components/tables/people-table/people-table.component';
import { IIndex } from '../../../core/interfaces/IIndex';
import { TitleComponent } from '../../../shared/components/title/title.component';

@Component({
  selector: 'app-index-people',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PeopleTableComponent,
    TitleComponent,
  ],
  templateUrl: './index-people.component.html',
})
export class IndexPeopleComponent implements IIndex, OnInit, OnDestroy {
  people$!: Observable<Person[]>;
  loading$!: Observable<boolean>;
  ICONS = ICONS;

  private subscriptions: Subscription = new Subscription();

  constructor(
    private peopleService: PeopleService,
    private modalService: CustomModalService,
    private sharedService: SharedService
  ) {
    this.people$ = this.peopleService.people$;
    this.loading$ = this.peopleService.loading$;
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
  }

  onCreateModal(): void {
    this.modalService.show(UpsertPeopleComponent, {
      initialState: {
        id: 0,
      },
      class: 'modal-lg',
    });
  }

  onImportModal(): void {
    this.modalService.show(ImportPeopleComponent, {
      class: 'modal-lg',
    });
  }

  updateBreadcrumbs(): void {
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

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
