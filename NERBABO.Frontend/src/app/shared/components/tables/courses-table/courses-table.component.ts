import { Component, Input, OnInit, ViewChild, OnDestroy } from '@angular/core';
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
import { Subscription } from 'rxjs';
import { CoursesService } from '../../../../core/services/courses.service';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';
import { ProgressBarModule } from 'primeng/progressbar';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { CreateActionsComponent } from '../../../../features/actions/create-actions/create-actions.component';

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
    IconAnchorComponent,
    ProgressBarModule,
    SpinnerComponent,
  ],
  templateUrl: './courses-table.component.html',
})
export class CoursesTableComponent implements OnInit, OnDestroy {
  @Input({ required: true }) courses!: Course[];
  @Input({ required: true }) loading!: boolean;
  @ViewChild('dt') dt!: Table;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedCourse: Course | undefined;
  first = 0;
  rows = 10;
  private subscriptions: Subscription = new Subscription();

  constructor(
    private modalService: BsModalService,
    private router: Router,
    private coursesService: CoursesService
  ) {}

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
          {
            label: 'Criar Ação Formação',
            icon: 'pi pi-plus-circle',
            command: () =>
              this.onCreateActionModal(
                this.selectedCourse!.id,
                this.selectedCourse!.title
              ),
          },
        ],
      },
    ];

    // Subscribe to course change notifications
    this.subscriptions.add(
      this.coursesService.updatedSource$.subscribe((courseId) => {
        this.refreshCourse(courseId);
      })
    );
    this.subscriptions.add(
      this.coursesService.deletedSource$.subscribe((courseId) => {
        this.removeCourse(courseId);
      })
    );
    this.subscriptions.add(
      this.coursesService.changeStatusSource$.subscribe((courseId) => {
        this.refreshCourse(courseId);
      })
    );
    this.subscriptions.add(
      this.coursesService.assignModuleSource$.subscribe((courseId) => {
        this.refreshCourse(courseId);
      })
    );
    this.subscriptions.add(
      this.coursesService.courses$.subscribe((courses) => {
        this.courses = courses; // Update the courses input
      })
    );
  }

  // Calculate progress percentage for a course
  getProgressPercentage(course: Course): number {
    if (course.totalDuration === 0) return 0;
    return Math.round(
      ((course.totalDuration - course.remainingDuration) /
        course.totalDuration) *
        100
    );
  }

  // Determine progress bar class based on percentage
  getProgressBarClass(course: Course): string {
    return this.getProgressPercentage(course) >= 100 ? '' : 'black';
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe(); // Clean up subscriptions
  }

  // Refresh a single course by fetching its updated data
  private refreshCourse(courseId: number): void {
    if (courseId === 0) {
      // Handle course creation (refetch all courses)
      this.coursesService.triggerFetchCourses();
      return;
    }
    this.coursesService.getSingleCourse(courseId).subscribe({
      next: (updatedCourse) => {
        const index = this.courses.findIndex(
          (course) => course.id === courseId
        );
        if (index !== -1) {
          this.courses[index] = updatedCourse; // Update the specific course
          this.courses = [...this.courses]; // Trigger change detection
        }
      },
      error: (error) => {
        console.error('Failed to refresh course:', error);
        // Optionally refetch all courses if single fetch fails
        this.coursesService.triggerFetchCourses();
      },
    });
  }

  // Remove a course from the local array
  private removeCourse(courseId: number): void {
    this.courses = this.courses.filter((course) => course.id !== courseId);
    // Trigger change detection
    this.courses = [...this.courses];
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

  onCreateActionModal(id: number, title: string) {
    this.modalService.show(CreateActionsComponent, {
      class: 'modal-lg',
      initialState: {
        courseId: id,
        courseTitle: title,
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
    return this.courses ? this.first + this.rows >= this.courses.length : true;
  }

  isFirstPage(): boolean {
    return this.courses ? this.first === 0 : true;
  }
}
