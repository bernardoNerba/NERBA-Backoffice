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
import { TeachersService } from '../../../../core/services/teachers.service';
import { StudentsService } from '../../../../core/services/students.service';
import { AccService } from '../../../../core/services/acc.service';
import { DeletePeopleComponent } from '../../../../features/people/delete-people/delete-people.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { InputTextModule } from 'primeng/inputtext';
import { BadgeModule } from 'primeng/badge';
import { Tag } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';
import { UpsertPeopleComponent } from '../../../../features/people/upsert-people/upsert-people.component';
import { UpsertTeachersComponent } from '../../../../features/teachers/upsert-teachers/upsert-teachers.component';
import { UpsertStudentsComponent } from '../../../../features/students/upsert-students/upsert-students.component';
import { UpsertAccComponent } from '../../../../features/acc/upsert-acc/upsert-acc.component';

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
    TooltipModule,
    IconAnchorComponent,
    RouterLink,
  ],
  templateUrl: './people-table.component.html'
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
    private peopleService: PeopleService,
    private teachersService: TeachersService,
    private studentsService: StudentsService,
    private accService: AccService
  ) {}

  ngOnInit(): void {
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

    // Subscribe to profile updates to refresh person data
    this.subscriptions.add(
      this.teachersService.updatedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
      })
    );

    this.subscriptions.add(
      this.studentsService.updatedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
      })
    );

    this.subscriptions.add(
      this.accService.updatedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
      })
    );

    // Subscribe to profile deletions
    this.subscriptions.add(
      this.teachersService.deletedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
      })
    );

    this.subscriptions.add(
      this.studentsService.deletedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
      })
    );

    this.subscriptions.add(
      this.accService.deletedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
      })
    );
  }

  populateMenu(person: Person): void {
    const items: MenuItem[] = [
      {
        label: 'Editar',
        icon: 'pi pi-pencil',
        command: () => this.onUpdatePersonModal(person),
      },
      {
        label: 'Eliminar',
        icon: 'pi pi-exclamation-triangle',
        command: () => this.onDeletePersonModal(person),
      },
      {
        label: 'Detalhes',
        icon: 'pi pi-exclamation-circle',
        command: () => this.router.navigateByUrl(`/people/${person.id}`),
      },
    ];

    // Add "Adicionar como" options if not already assigned
    if (!person.isTeacher) {
      items.push({
        label: 'Adicionar como Formador',
        icon: 'pi pi-user-plus',
        command: () => this.onCreateTeacher(person),
      });
    } else {
      items.push({
        label: 'Ver dados Formador',
        icon: 'pi pi-user',
        command: () => this.router.navigateByUrl(`/people/${person.id}/teacher`),
      });
    }

    if (!person.isStudent) {
      items.push({
        label: 'Adicionar como Formando',
        icon: 'pi pi-graduation-cap',
        command: () => this.onCreateStudent(person),
      });
    } else {
      items.push({
        label: 'Ver dados Formando',
        icon: 'pi pi-book',
        command: () => this.router.navigateByUrl(`/people/${person.id}/student`),
      });
    }

    if (!person.isColaborator) {
      items.push({
        label: 'Adicionar como Colaborador',
        icon: 'pi pi-briefcase',
        command: () => this.onCreateColaborator(person),
      });
    } else {
      items.push({
        label: 'Ver dados Colaborador',
        icon: 'pi pi-building',
        command: () => this.router.navigateByUrl(`/people/${person.id}/acc`),
      });
    }

    this.menuItems = [
      {
        label: 'Opções',
        items,
      },
    ];
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

  onCreateTeacher(person: Person): void {
    this.modalService.show(UpsertTeachersComponent, {
      initialState: {
        id: 0,
        personId: person.id,
      },
      class: 'modal-lg',
    });
  }

  onCreateStudent(person: Person): void {
    this.modalService.show(UpsertStudentsComponent, {
      initialState: {
        id: 0,
        personId: person.id,
      },
      class: 'modal-lg',
    });
  }

  onCreateColaborator(person: Person): void {
    this.modalService.show(UpsertAccComponent, {
      initialState: {
        id: 0,
        personId: person.id,
      },
      class: 'modal-md',
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
