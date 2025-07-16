import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Course } from '../../../core/models/course';
import { ICONS } from '../../../core/objects/icons';
import { ReactiveFormsModule } from '@angular/forms';
import { CoursesService } from '../../../core/services/courses.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { CommonModule } from '@angular/common';
import { CreateCoursesComponent } from '../create-courses/create-courses.component';
import { CoursesTableComponent } from '../../../shared/components/tables/courses-table/courses-table.component';

@Component({
  selector: 'app-index-courses',
  imports: [
    ReactiveFormsModule,
    IconComponent,
    CommonModule,
    CoursesTableComponent,
  ],
  templateUrl: './index-courses.component.html',
})
export class IndexCoursesComponent implements OnInit {
  courses$!: Observable<Course[]>;
  loading$!: Observable<boolean>;
  ICONS = ICONS;

  constructor(
    private coursesService: CoursesService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {
    this.courses$ = this.coursesService.courses$;
    this.loading$ = this.coursesService.loading$;
  }
  ngOnInit(): void {
    this.updateBreadcumbs();
  }

  onAddCourseModal() {
    this.modalService.show(CreateCoursesComponent, {
      class: 'modal-lg',
      initialState: {},
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
