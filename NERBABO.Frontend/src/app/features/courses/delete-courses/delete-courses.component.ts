import { Component, Input } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { CoursesService } from '../../../core/services/courses.service';
import { SharedService } from '../../../core/services/shared.service';

@Component({
  selector: 'app-delete-courses',
  imports: [],
  templateUrl: './delete-courses.component.html',
})
export class DeleteCoursesComponent {
  @Input({ required: true }) id!: number;
  @Input({ required: true }) title!: string;
  deleting: boolean = false;

  constructor(
    public bsModalRef: BsModalRef,
    private coursesService: CoursesService,
    private sharedService: SharedService
  ) {}

  confirmDelete(): void {
    this.deleting = true;

    this.coursesService.delete(this.id).subscribe({
      next: (value) => {
        this.coursesService.triggerFetchCourses();
        this.bsModalRef.hide();
        this.sharedService.showSuccess(value.message);
        this.coursesService.notifyCourseDelete(this.id);
      },
      error: (error) => {
        this.sharedService.handleErrorResponse(error);
        this.deleting = false;
      },
    });
  }
}
