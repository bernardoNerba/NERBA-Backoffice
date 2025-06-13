import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { BsModalService } from 'ngx-bootstrap/modal';
import { ConfigService } from '../../../../core/services/config.service';
import { Tax } from '../../../../core/models/tax';
import { CreateTaxesComponent } from '../create-taxes/create-taxes.component';
import { DeleteTaxesComponent } from '../delete-taxes/delete-taxes.component';
import { UpdateTaxesComponent } from '../update-taxes/update-taxes.component';

@Component({
  selector: 'app-index-taxes',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './index-taxes.component.html',
  styleUrl: './index-taxes.component.css',
})
export class IndexTaxesComponent implements OnInit {
  taxes$!: Observable<Array<Tax>>;
  loading$!: Observable<boolean>;

  constructor(
    private confService: ConfigService,
    private modalService: BsModalService
  ) {}

  ngOnInit(): void {
    this.loadCofiguration();
  }

  private loadCofiguration() {
    this.taxes$ = this.confService.taxes$;
    this.loading$ = this.confService.loading$;
  }

  onAddIvaModal() {
    this.modalService.show(CreateTaxesComponent, {
      initialState: {},
      class: 'modal-md',
    });
  }

  onDeleteIvaModal(id: number, name: string) {
    this.modalService.show(DeleteTaxesComponent, {
      initialState: { id: id, name: name },
      class: 'modal-md',
    });
  }

  onUpdateIvaModal(instance: Tax) {
    this.modalService.show(UpdateTaxesComponent, {
      initialState: { currentTaxIva: instance },
      class: 'modal-md',
    });
  }
}
