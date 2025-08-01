import {
  Component,
  Input,
  OnInit,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { MultiSelectModule } from 'primeng/multiselect';
import { Person } from '../../../../core/models/person';
import { MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Router, RouterLink } from '@angular/router';
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

interface ProfileOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-people-table',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    Button,
    Menu,
    MultiSelectModule,
    CommonModule,
    FormsModule,
    TruncatePipe,
    SpinnerComponent,
    InputTextModule,
    BadgeModule,
    Tag,
    IconAnchorComponent,
    RouterLink,
  ],
  templateUrl: './people-table.component.html',
})
export class PeopleTableComponent implements OnInit, AfterViewInit {
  @Input({ required: true }) people!: Person[];
  @Input({ required: true }) loading!: boolean;
  @ViewChild('dt') dt!: Table;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedPerson: Person | undefined;
  selectedProfiles: ProfileOption[] = [];
  first = 0;
  rows = 10;

  // Profile options for the multiselect
  profileOptions: ProfileOption[] = [
    { label: 'Formador', value: 'Teacher' },
    { label: 'Formando', value: 'Student' },
    { label: 'Colaborador', value: 'Colaborator' },
  ];

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

  ngAfterViewInit(): void {
    // No need to register custom filter function, we'll handle filtering manually
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

  // Filtered people based on selected profiles
  get filteredPeople(): Person[] {
    if (this.selectedProfiles.length === 0) {
      return this.people;
    }

    const selectedValues = this.selectedProfiles.map(
      (profile) => profile.value
    );

    return this.people.filter((person) => {
      return selectedValues.every((profileType) => {
        switch (profileType) {
          case 'Teacher':
            return person.isTeacher;
          case 'Student':
            return person.isStudent;
          case 'Colaborator':
            return person.isColaborator;
          default:
            return false;
        }
      });
    });
  }

  // Profile filter function - simplified
  onProfileFilterChange(): void {
    // The filtering is handled by the getter above
    // This method exists to trigger change detection when profiles are selected
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
    return this.filteredPeople
      ? this.first + this.rows >= this.filteredPeople.length
      : true;
  }

  isFirstPage(): boolean {
    return this.filteredPeople ? this.first === 0 : true;
  }

  clearFilters() {
    this.searchValue = '';
    this.selectedProfiles = [];
    this.dt.reset();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
