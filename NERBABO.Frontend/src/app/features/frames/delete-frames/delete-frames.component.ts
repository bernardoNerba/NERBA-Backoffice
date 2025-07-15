import { Component } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { FrameService } from '../../../core/services/frame.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedService } from '../../../core/services/shared.service';
@Component({
  selector: 'app-delete-frames',
  templateUrl: './delete-frames.component.html',
  styleUrl: './delete-frames.component.css',
  imports: [CommonModule, ReactiveFormsModule],
})
export class DeleteFramesComponent {
  id!: number;
  name!: string;
  deleting = false;

  constructor(
    public bsModalRef: BsModalRef,
    private frameService: FrameService,
    private sharedService: SharedService
  ) {}

  confirmDelete(): void {
    this.deleting = true;
    this.frameService.delete(this.id).subscribe({
      next: (value) => {
        this.frameService.triggerFetchFrames();
        this.bsModalRef.hide();
        this.sharedService.showSuccess(value.message);
        this.frameService.notifyFrameDelete(this.id);
      },
      error: (error) => {
        this.sharedService.handleErrorResponse(error);
        this.deleting = false;
      },
    });
  }
}
