import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { CompaniesService } from '../../../core/services/companies.service';
import { SharedService } from '../../../core/services/shared.service';

@Component({
  selector: 'app-delete-companies',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './delete-companies.component.html',
})
export class DeleteCompaniesComponent {
  id!: number;
  name!: string;
  deleting = false;

  constructor(
    public bsModalRef: BsModalRef,
    private companiesService: CompaniesService,
    private sharedService: SharedService
  ) {}

  confirmDelete(): void {
    this.deleting = true;
    this.companiesService.deleteCompany(this.id).subscribe({
      next: (value) => {
        this.companiesService.triggerFetch();
        this.bsModalRef.hide();
        this.sharedService.showSuccess(value.message);
        this.companiesService.notifyDelete(this.id);
      },
      error: (error) => {
        this.sharedService.handleErrorResponse(error);
        this.deleting = false;
      },
    });
  }
}
