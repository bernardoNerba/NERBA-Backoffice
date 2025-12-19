import { Component, Input } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { NotificationService } from '../../../core/services/notification.service';
import { SharedService } from '../../../core/services/shared.service';

@Component({
  selector: 'app-delete-notifications',
  imports: [],
  templateUrl: './delete-notifications.component.html',
})
export class DeleteNotificationsComponent {
  @Input({ required: true }) id!: number;
  @Input({ required: true }) title!: string;
  deleting = false;

  constructor(
    public bsModalRef: BsModalRef,
    private notificationService: NotificationService,
    private sharedService: SharedService
  ) {}

  confirmDelete(): void {
    this.deleting = true;

    this.notificationService.deleteNotification(this.id).subscribe({
      next: (value) => {
        this.bsModalRef.hide();
        this.sharedService.showSuccess('Notificação eliminada com sucesso.');
      },
      error: (error) => {
        this.sharedService.handleErrorResponse(error);
        this.deleting = false;
      },
    });
  }
}
