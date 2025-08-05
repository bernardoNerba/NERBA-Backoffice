import { Component, Input } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { AccService } from '../../../core/services/acc.service';
import { SharedService } from '../../../core/services/shared.service';
@Component({
  selector: 'app-block-acc',
  imports: [],
  templateUrl: './block-acc.component.html',
})
export class BlockAccComponent {
  @Input() id!: string;
  @Input() active!: boolean;
  @Input() fullName!: string;

  constructor(
    private accountService: AccService,
    public bsModalRef: BsModalRef,
    private sharedService: SharedService
  ) {}

  blockUnblockUser() {
    this.accountService.blockUser(this.id).subscribe({
      next: (response) => {
        this.sharedService.showSuccess(response.title);
        this.bsModalRef.hide();
      },
      error: (err) => {
        this.sharedService.handleErrorResponse(err);
      },
    });
  }
}
