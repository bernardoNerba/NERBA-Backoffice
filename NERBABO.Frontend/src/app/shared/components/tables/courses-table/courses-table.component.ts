import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Course } from '../../../../core/models/course';
import { TableModule } from 'primeng/table';
import { CommonModule } from '@angular/common';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { StatusEnum } from '../../../../core/objects/status';
import { TagModule } from 'primeng/tag';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { Table } from 'primeng/table';
import { BsModalService } from 'ngx-bootstrap/modal';
import { CreateCoursesComponent } from '../../../../features/courses/create-courses/create-courses.component';
import { UpdateCoursesComponent } from '../../../../features/courses/update-courses/update-courses.component';
import { DeleteCoursesComponent } from '../../../../features/courses/delete-courses/delete-courses.component';
import { ChangeStatusCoursesComponent } from '../../../../features/courses/change-status-courses/change-status-courses.component';
import { AssignModuleCoursesComponent } from '../../../../features/courses/assign-module-courses/assign-module-courses.component';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-courses-table',
  imports: [
    TableModule,
    CommonModule,
    TruncatePipe,
    TagModule,
    Button,
    Menu,
    IconField,
    InputIcon,
    InputTextModule,
    FormsModule,
    RouterLink,
  ],
  templateUrl: './courses-table.component.html',
  styleUrl: './courses-table.component.css',
})
export class CoursesTableComponent implements OnInit {
  @Input({ required: true }) courses!: Course[];
  @Input({ required: true }) loading!: boolean;
  @ViewChild('dt') dt!: Table;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedCourse: Course | undefined;

  constructor(private modalService: BsModalService, private router: Router) {}

  ngOnInit(): void {
    this.menuItems = [
      {
        label: 'Opções',
        items: [
          {
            label: 'Editar',
            icon: 'pi pi-pencil',
            command: () => this.onUpdateCourseModal(this.selectedCourse!),
          },
          {
            label: 'Eliminar',
            icon: 'pi pi-exclamation-triangle',
            command: () =>
              this.onDeleteCourseModal(
                this.selectedCourse!.id,
                this.selectedCourse!.title
              ),
          },
          {
            label: 'Detalhes',
            icon: 'pi pi-exclamation-circle',
            command: () =>
              this.router.navigateByUrl(`/courses/${this.selectedCourse!.id}`),
          },
          {
            label: 'Atualizar Estado',
            icon: 'pi pi-refresh',
            command: () => this.onChangeStatusModal(this.selectedCourse!),
          },
          {
            label: 'Atribuir Módulo',
            icon: 'pi pi-plus-circle',
            command: () => this.onAssignModuleModal(this.selectedCourse!),
          },
        ],
      },
    ];
  }

  clearFilters() {
    this.searchValue = '';
    this.dt.reset();
  }

  onAddCourseModal() {
    this.modalService.show(CreateCoursesComponent, {
      class: 'modal-lg',
      initialState: {},
    });
  }

  onUpdateCourseModal(course: Course) {
    this.modalService.show(UpdateCoursesComponent, {
      class: 'modal-lg',
      initialState: {
        id: course.id,
        course: course,
      },
    });
  }

  onDeleteCourseModal(id: number, title: string) {
    this.modalService.show(DeleteCoursesComponent, {
      class: 'modal-lg',
      initialState: {
        id: id,
        title: title,
      },
    });
  }

  onChangeStatusModal(course: Course) {
    this.modalService.show(ChangeStatusCoursesComponent, {
      class: 'modal-md',
      initialState: {
        course: course,
      },
    });
  }

  onAssignModuleModal(course: Course) {
    this.modalService.show(AssignModuleCoursesComponent, {
      class: 'modal-md',
      initialState: {
        course: course,
      },
    });
  }

  columns = [
    { field: 'title', header: 'Título' },
    { field: 'minHabilitationLevel', header: 'Nível Min. Hab.' },
    { field: 'totalDuration', header: 'Duração' },
    { field: 'status', header: 'Estado' },
  ];

  getStatusSeverity(status: string) {
    switch (status) {
      case StatusEnum.Cancelled:
        return 'secondary';
      case StatusEnum.Completed:
        return 'success';
      case StatusEnum.InProgress:
        return 'warn';
      case StatusEnum.NotStarted:
        return 'info';
    }
    return null;
  }

  getStatusIcon(status: string) {
    switch (status) {
      case StatusEnum.Cancelled:
        return 'pi pi-times';
      case StatusEnum.Completed:
        return 'pi pi-check';
      case StatusEnum.InProgress:
        return 'pi pi-exclamation-triangle';
      case StatusEnum.NotStarted:
        return 'pi pi-info-circle';
    }
    return null;
  }
}
