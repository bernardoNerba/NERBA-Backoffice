import { Component, Input } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { ActionsService } from '../../../core/services/actions.service';

@Component({
  selector: 'app-delete-actions',
  imports: [],
  templateUrl: './delete-actions.component.html',
})
export class DeleteActionsComponent {
  @Input({ required: true }) id!: number;
  @Input({ required: true }) title!: string;
  @Input({ required: true }) courseId!: number;
  deleting: boolean = false;

  constructor(
    public bsModalRef: BsModalRef,
    private actionsService: ActionsService,
    private sharedService: SharedService
  ) {}

  confirmDelete(): void {
    this.deleting = true;

    this.actionsService.deleteAction(this.id, this.courseId).subscribe({
      next: (value) => {
        this.actionsService.triggerFetchActions();
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
