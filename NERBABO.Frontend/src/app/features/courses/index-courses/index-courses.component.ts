import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Course } from '../../../core/models/course';
import { ICONS } from '../../../core/objects/icons';
import { ReactiveFormsModule } from '@angular/forms';
import { CoursesService } from '../../../core/services/courses.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { CommonModule } from '@angular/common';
import { CoursesTableComponent } from '../../../shared/components/tables/courses-table/courses-table.component';
import { IIndex } from '../../../core/interfaces/IIndex';
import { UpsertCoursesComponent } from '../upsert-courses/upsert-courses.component';
import { TitleComponent } from '../../../shared/components/title/title.component';

@Component({
  selector: 'app-index-courses',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    CoursesTableComponent,
    TitleComponent,
  ],
  templateUrl: './index-courses.component.html',
})
export class IndexCoursesComponent implements IIndex, OnInit {
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
    this.updateBreadcrumbs();
  }


  onCreateModal() {
    this.modalService.show(UpsertCoursesComponent, {
      class: 'modal-lg',
      initialState: {
        id: 0,
      },
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
        url: '/courses',
        displayName: 'Cursos',
        className: 'inactive',
      },
    ]);
  }

}
