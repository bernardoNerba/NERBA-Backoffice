import { Component, Input } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { StudentsService } from '../../../core/services/students.service';

@Component({
  selector: 'app-delete-students',
  imports: [],
  templateUrl: './delete-students.component.html',
})
export class DeleteStudentsComponent {
  @Input({ required: true }) id!: number;
  @Input({ required: true }) fullName!: string;
  deleting = false;

  constructor(
    public bsModalRef: BsModalRef,
    private studentsService: StudentsService,
    private sharedService: SharedService
  ) {}

  confirmDelete(): void {
    this.deleting = true;

    this.studentsService.delete(this.id).subscribe({
      next: (value) => {
        this.bsModalRef.hide();
        this.sharedService.showSuccess(value.message);
      },
      error: (error) => {
        this.sharedService.handleErrorResponse(error);
        this.deleting = false;
      },
    });
  }
}
