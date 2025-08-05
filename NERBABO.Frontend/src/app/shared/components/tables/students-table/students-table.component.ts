import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MultiSelectModule } from 'primeng/multiselect';
import { Student } from '../../../../core/models/student';
import { MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Router } from '@angular/router';
import { StudentsService } from '../../../../core/services/students.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { InputTextModule } from 'primeng/inputtext';
import { BadgeModule } from 'primeng/badge';
import { TagModule } from 'primeng/tag';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';
import { UpsertStudentsComponent } from '../../../../features/students/upsert-students/upsert-students.component';
import { DeleteStudentsComponent } from '../../../../features/students/delete-students/delete-students.component';

@Component({
  selector: 'app-students-table',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    ButtonModule,
    MenuModule,
    MultiSelectModule,
    CommonModule,
    FormsModule,
    TruncatePipe,
    SpinnerComponent,
    InputTextModule,
    BadgeModule,
    TagModule,
    IconAnchorComponent,
  ],
  standalone: true,
  templateUrl: './students-table.component.html',
})
export class StudentsTableComponent implements OnInit {
  @Input({ required: true }) students!: Student[];
  @Input({ required: true }) loading!: boolean;
  @ViewChild('dt') dt!: Table;
  @Input({}) isCompanyPage: boolean = false;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedStudent: Student | undefined;
  first = 0;
  rows = 10;

  private subscriptions = new Subscription();

  constructor(
    private modalService: BsModalService,
    private router: Router,
    private studentsService: StudentsService
  ) {}

  ngOnInit(): void {
    this.menuItems = [
      {
        label: 'Opções',
        items: [
          {
            label: 'Detalhes da Pessoa',
            icon: 'pi pi-exclamation-circle',
            command: () =>
              this.router.navigateByUrl(
                `/people/${this.selectedStudent!.personId}`
              ),
          },
          {
            label: 'Detalhes do Formando',
            icon: 'pi pi-exclamation-circle',
            command: () =>
              this.router.navigateByUrl(
                `/people/${this.selectedStudent!.personId}/student`
              ),
          },
          {
            label: 'Editar',
            icon: 'pi pi-pencil',
            command: () => this.onUpdateModal(this.selectedStudent!),
          },
          {
            label: 'Delete',
            icon: 'pi pi-trash',
            command: () => this.onDeleteModal(this.selectedStudent!),
          },
        ],
      },
    ];

    this.subscriptions.add(
      this.studentsService.updatedSource$.subscribe((studentId: number) => {
        this.refreshStudents(studentId, 'update');
      })
    );

    this.subscriptions.add(
      this.studentsService.deletedSource$.subscribe((studentId: number) => {
        this.refreshStudents(studentId, 'delete');
        console.log('From subscription ', studentId);
      })
    );
  }

  refreshStudents(id: number, action: 'update' | 'delete'): void {
    if (id === 0) {
      // If id is 0, it indicates a full refresh is needed (e.g., after create)
      // Parent component should handle full refresh of people
      return;
    }

    // check if the person exists in the current students list
    const index = this.students.findIndex((student) => student.id === id);
    if (index === -1) return; // Student not in this list, no action needed

    if (action === 'delete') {
      // Remove the person from the list
      this.students = this.students.filter((student) => student.id !== id);
    } else if (action === 'update') {
      this.studentsService.getById(id).subscribe({
        next: (updatedStudent) => {
          this.students[index] = updatedStudent;
          this.students = [...this.students];
        },
        error: (error) => {
          console.log('Failed to refresh students: ', error);
        },
      });
    }
  }

  onUpdateModal(student: Student) {
    this.modalService.show(UpsertStudentsComponent, {
      initialState: {
        id: student.id,
        personId: student.personId,
      },
      class: 'modal-lg',
    });
  }

  onDeleteModal(student: Student) {
    this.modalService.show(DeleteStudentsComponent, {
      initialState: {
        id: student.id,
        fullName: student.studentFullName,
      },
      class: 'modal-lg',
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
    return this.students
      ? this.first + this.rows >= this.students.length
      : true;
  }

  isFirstPage(): boolean {
    return this.students ? this.first === 0 : true;
  }

  clearFilters() {
    this.searchValue = '';
    this.dt.reset();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
