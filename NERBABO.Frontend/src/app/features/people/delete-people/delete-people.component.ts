import { Component, Input } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { PeopleService } from '../../../core/services/people.service';
import { SharedService } from '../../../core/services/shared.service';

@Component({
  selector: 'app-delete-people',
  imports: [],
  templateUrl: './delete-people.component.html',
  styleUrl: './delete-people.component.css',
})
export class DeletePeopleComponent {
  @Input({ required: true }) id!: number;
  @Input({ required: true }) fullName!: string;
  deleting = false;

  constructor(
    public bsModalRef: BsModalRef,
    private peopleService: PeopleService,
    private sharedService: SharedService
  ) {}

  eliminarPessoa(): void {
    this.deleting = true;

    this.peopleService.deletePerson(this.id).subscribe({
      next: (value) => {
        console.log(value);
        this.peopleService.triggerFetchPeople();
        this.peopleService.notifyPersonDelete(this.id);
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
