import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { Observable, map } from 'rxjs';
import { CommonModule } from '@angular/common';
import { BsModalService } from 'ngx-bootstrap/modal';
import { ConfigService } from '../../../../core/services/config.service';
import { Tax } from '../../../../core/models/tax';
import { CreateTaxesComponent } from '../create-taxes/create-taxes.component';
import { DeleteTaxesComponent } from '../delete-taxes/delete-taxes.component';
import { UpdateTaxesComponent } from '../update-taxes/update-taxes.component';
import { TaxType } from '../../../../core/objects/taxType';
import { SharedService } from '../../../../core/services/shared.service';

@Component({
  selector: 'app-index-taxes',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './index-taxes.component.html',
  styleUrl: './index-taxes.component.css',
})
export class IndexTaxesComponent implements OnInit {
  taxes$!: Observable<Array<Tax>>;
  ivaTaxes$!: Observable<Array<Tax>>;
  irsTaxes$!: Observable<Array<Tax>>;
  loading$!: Observable<boolean>;

  constructor(
    private confService: ConfigService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.loadCofiguration();
  }

  private loadCofiguration() {
    this.taxes$ = this.confService.taxes$;
    this.loading$ = this.confService.loading$;

    // Filter taxes by type
    this.ivaTaxes$ = this.taxes$.pipe(
      map((taxes) =>
        taxes.filter((tax) => tax.type === TaxType.Iva || tax.type === 'IVA')
      )
    );

    this.irsTaxes$ = this.taxes$.pipe(
      map((taxes) =>
        taxes.filter((tax) => tax.type === TaxType.Irs || tax.type === 'IRS')
      )
    );
  }

  onAddTaxModal() {
    this.modalService.show(CreateTaxesComponent, {
      initialState: {},
      class: 'modal-md',
    });
  }

  onDeleteIvaModal(id: number, name: string) {
    this.modalService.show(DeleteTaxesComponent, {
      initialState: { id: id, name: name, type: TaxType.Iva },
      class: 'modal-md',
    });
  }

  onDeleteIrsModal(id: number, name: string) {
    this.modalService.show(DeleteTaxesComponent, {
      initialState: { id: id, name: name, type: TaxType.Irs },
      class: 'modal-md',
    });
  }

  onUpdateIvaModal(instance: Tax) {
    this.modalService.show(UpdateTaxesComponent, {
      initialState: { currentTax: instance, type: TaxType.Iva },
      class: 'modal-md',
    });
  }

  onUpdateIrsModal(instance: Tax) {
    this.modalService.show(UpdateTaxesComponent, {
      initialState: { currentTax: instance, type: TaxType.Irs },
      class: 'modal-md',
    });
  }

  onToggleTaxStatus(tax: Tax) {
    tax.isActive = !tax.isActive;
    this.confService.updateIvaTax(tax).subscribe({
      next: (value) => {
        this.confService.triggerFetchConfigs();
        this.sharedService.showSuccess(value.message);
      },
      error: (error) => {
        this.sharedService.handleErrorResponse(error);
      },
    });
  }
}
