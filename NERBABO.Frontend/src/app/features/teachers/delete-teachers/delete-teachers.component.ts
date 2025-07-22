import { Component, Input } from '@angular/core';
import { TeachersService } from '../../../core/services/teachers.service';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';

@Component({
  selector: 'app-delete-teachers',
  imports: [],
  templateUrl: './delete-teachers.component.html',
})
export class DeleteTeachersComponent {
  @Input({ required: true }) id!: number;
  @Input({ required: true }) fullName!: string;
  deleting = false;

  constructor(
    public bsModalRef: BsModalRef,
    private teacherService: TeachersService,
    private sharedService: SharedService
  ) {}

  confirmDelete(): void {
    this.deleting = true;

    this.teacherService.delete(this.id).subscribe({
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
