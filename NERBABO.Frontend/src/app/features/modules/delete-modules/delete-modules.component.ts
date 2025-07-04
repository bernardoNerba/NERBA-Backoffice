import { Component } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ModulesService } from '../../../core/services/modules.service';
import { SharedService } from '../../../core/services/shared.service';

@Component({
  selector: 'app-delete-modules',
  imports: [],
  templateUrl: './delete-modules.component.html',
  styleUrl: './delete-modules.component.css',
})
export class DeleteModulesComponent {
  id!: number;
  name!: string;
  deleting: boolean = false;

  constructor(
    public bsModalRef: BsModalRef,
    private moduleService: ModulesService,
    private sharedService: SharedService
  ) {}

  confirmDelete(): void {
    this.deleting = true;
    this.moduleService.deleteModule(this.id).subscribe({
      next: (value) => {
        this.moduleService.triggerFetch();
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
