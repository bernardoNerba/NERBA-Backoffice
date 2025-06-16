import { Component } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ConfigService } from '../../../../core/services/config.service';
import { SharedService } from '../../../../core/services/shared.service';
import { OkResponse } from '../../../../core/models/okResponse';

@Component({
  selector: 'app-delete-taxes',
  imports: [],
  templateUrl: './delete-taxes.component.html',
  styleUrl: './delete-taxes.component.css',
})
export class DeleteTaxesComponent {
  id!: number;
  name!: string;
  deleting = false;
  constructor(
    private confService: ConfigService,
    private sharedService: SharedService,
    public bsModalRef: BsModalRef
  ) {}

  onSubmit() {
    this.deleting = true;

    this.confService.deleteIvaTax(this.id).subscribe({
      next: (value: OkResponse) => {
        this.sharedService.showSuccess(value.message);
        this.confService.triggerFetchConfigs();
        this.deleting = false;
        this.bsModalRef.hide();
      },
      error: (error) => {
        this.sharedService.handleErrorResponse(error);
        this.deleting = false;
      },
    });
  }
}
