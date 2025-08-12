import { Component, Input } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { SessionsService } from '../../../core/services/sessions.service';
import { SharedService } from '../../../core/services/shared.service';
import { Session } from '../../../core/objects/sessions';

@Component({
  selector: 'app-delete-sessions',
  imports: [],
  templateUrl: './delete-sessions.component.html',
  styleUrl: './delete-sessions.component.css',
})
export class DeleteSessionsComponent {
  @Input({ required: true }) id!: number;
  @Input({ required: true }) session!: Session;
  deleting: boolean = false;

  constructor(
    public bsModalRef: BsModalRef,
    private sessionsService: SessionsService,
    private sharedService: SharedService
  ) {}

  confirmDelete(): void {
    this.deleting = true;

    this.sessionsService.delete(this.id).subscribe({
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
