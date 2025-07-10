import { Component, OnInit } from '@angular/core';
import {
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  map,
  Observable,
  startWith,
} from 'rxjs';
import { Course } from '../../../core/models/course';
import { ICONS } from '../../../core/objects/icons';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CoursesService } from '../../../core/services/courses.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { CommonModule } from '@angular/common';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { RouterLink } from '@angular/router';
import { TruncatePipe } from '../../../shared/pipes/truncate.pipe';
import { StatusEnum } from '../../../core/objects/status';
import { UpdateCoursesComponent } from '../update-courses/update-courses.component';
import { DeleteCoursesComponent } from '../delete-courses/delete-courses.component';
import { CreateCoursesComponent } from '../create-courses/create-courses.component';
import { ChangeStatusCoursesComponent } from '../change-status-courses/change-status-courses.component';

@Component({
  selector: 'app-index-courses',
  imports: [
    ReactiveFormsModule,
    IconComponent,
    CommonModule,
    SpinnerComponent,
    RouterLink,
    TruncatePipe,
  ],
  templateUrl: './index-courses.component.html',
  styleUrl: './index-courses.component.css',
})
export class IndexCoursesComponent implements OnInit {
  courses$!: Observable<Course[]>;
  loading$!: Observable<boolean>;
  filteredCourses$!: Observable<Course[]>;
  columns = ['#', 'Título', 'Nível Min. Hab.', 'Total Horas', 'Status'];
  ICONS = ICONS;
  STATUS = StatusEnum;
  searchControl = new FormControl('');

  constructor(
    private coursesService: CoursesService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {
    this.courses$ = this.coursesService.courses$;
    this.loading$ = this.coursesService.loading$;
  }
  ngOnInit(): void {
    this.filteredCourses$ = combineLatest([
      this.courses$,
      this.searchControl.valueChanges.pipe(
        startWith(''),
        debounceTime(100),
        distinctUntilChanged()
      ),
    ]).pipe(
      map(([courses, searchTerm]) => {
        if (!searchTerm) return courses;
        const term = searchTerm.toLowerCase();
        return courses.filter((course) =>
          course.title.toLowerCase().includes(term)
        );
      })
    );
    this.updateBreadcumbs();
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

  private updateBreadcumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/courses',
        displayName: 'Cursos',
        className: 'inactive',
      },
    ]);
  }
}
