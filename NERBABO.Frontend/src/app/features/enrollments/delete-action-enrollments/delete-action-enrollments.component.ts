import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ActionEnrollmentService } from '../../../core/services/action-enrollment.service';
import { SharedService } from '../../../core/services/shared.service';

@Component({
  selector: 'app-delete-action-enrollments',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './delete-action-enrollments.component.html',
})
export class DeleteActionEnrollmentsComponent {
  id!: number;
  name!: string;
  deleting = false;

  constructor(
    public bsModalRef: BsModalRef,
    private enrollmentService: ActionEnrollmentService,
    private sharedService: SharedService
  ) {}

  confirmDelete(): void {
    this.deleting = true;
    this.enrollmentService.delete(this.id).subscribe({
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
