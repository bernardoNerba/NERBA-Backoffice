import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { Person } from '../../../../core/models/person';
import { MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Router } from '@angular/router';
import { PeopleService } from '../../../../core/services/people.service';
import { DeletePeopleComponent } from '../../../../features/people/delete-people/delete-people.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { InputTextModule } from 'primeng/inputtext';
import { BadgeModule } from 'primeng/badge';
import { Tag } from 'primeng/tag';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';
import { UpsertPeopleComponent } from '../../../../features/people/upsert-people/upsert-people.component';

@Component({
  selector: 'app-people-table',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    Button,
    Menu,
    CommonModule,
    FormsModule,
    TruncatePipe,
    SpinnerComponent,
    InputTextModule,
    BadgeModule,
    Tag,
    IconAnchorComponent,
  ],
  templateUrl: './people-table.component.html',
})
export class PeopleTableComponent implements OnInit {
  @Input({ required: true }) people!: Person[];
  @Input({ required: true }) loading!: boolean;
  @ViewChild('dt') dt!: Table;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedPerson: Person | undefined;
  first = 0;
  rows = 10;

  private subscriptions = new Subscription();

  constructor(
    private modalService: BsModalService,
    private router: Router,
    private peopleService: PeopleService
  ) {}

  ngOnInit(): void {
    this.menuItems = [
      {
        label: 'Opções',
        items: [
          {
            label: 'Editar',
            icon: 'pi pi-pencil',
            command: () => this.onUpdatePersonModal(this.selectedPerson!),
          },
          {
            label: 'Eliminar',
            icon: 'pi pi-exclamation-triangle',
            command: () => this.onDeletePersonModal(this.selectedPerson!),
          },
          {
            label: 'Detalhes',
            icon: 'pi pi-exclamation-circle',
            command: () =>
              this.router.navigateByUrl(`/people/${this.selectedPerson!.id}`),
          },
        ],
      },
    ];

    // Subscribe to person updates
    this.subscriptions.add(
      this.peopleService.updatedSource$.subscribe((personId) => {
        this.refreshPerson(personId, 'update');
      })
    );

    // Subscribe to person deletions
    this.subscriptions.add(
      this.peopleService.deletedSource$.subscribe((personId) => {
        this.refreshPerson(personId, 'delete');
      })
    );
  }

  refreshPerson(id: number, action: 'update' | 'delete'): void {
    if (id === 0) {
      // If id is 0, it indicates a full refresh is needed (e.g., after create)
      // Parent component should handle full refresh of people
      return;
    }

    // Check if the person exists in the current people list
    const index = this.people.findIndex((person) => person.id === id);
    if (index === -1) return; // Person not in this list, no action needed

    if (action === 'delete') {
      // Remove the person from the list
      this.people = this.people.filter((person) => person.id !== id);
    } else if (action === 'update') {
      // Fetch the updated person
      this.peopleService.getSinglePerson(id).subscribe({
        next: (updatedPerson) => {
          this.people[index] = updatedPerson;
          this.people = [...this.people]; // Trigger change detection
        },
        error: (error) => {
          console.error('Failed to refresh person: ', error);
        },
      });
    }
  }

  onUpdatePersonModal(person: Person): void {
    this.modalService.show(UpsertPeopleComponent, {
      class: 'modal-lg',
      initialState: {
        id: person.id,
      },
    });
  }

  onDeletePersonModal(person: Person): void {
    this.modalService.show(DeletePeopleComponent, {
      class: 'modal-md',
      initialState: {
        id: person.id,
        fullName: person.fullName,
      },
    });
  }

  next() {
    this.first = this.first + this.rows;
  }

  prev() {
    this.first = this.first - this.rows;
  }

  reset() {
    this.first = 0;
  }

  pageChange(event: any) {
    this.first = event.first;
    this.rows = event.rows;
  }

  isLastPage(): boolean {
    return this.people ? this.first + this.rows >= this.people.length : true;
  }

  isFirstPage(): boolean {
    return this.people ? this.first === 0 : true;
  }

  clearFilters() {
    this.searchValue = '';
    this.dt.reset();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
