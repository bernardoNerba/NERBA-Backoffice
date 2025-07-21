import { Component, OnInit } from '@angular/core';
import { ActionsTableComponent } from '../../../shared/components/tables/actions-table/actions-table.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { ICONS } from '../../../core/objects/icons';
import { Observable } from 'rxjs';
import { Action } from '../../../core/models/action';
import { ActionsService } from '../../../core/services/actions.service';
import { IIndex } from '../../../core/interfaces/IIndex';
import { BsModalService } from 'ngx-bootstrap/modal';
import { UpsertActionsComponent } from '../upsert-actions/upsert-actions.component';
import { SharedService } from '../../../core/services/shared.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-index-actions',
  imports: [ActionsTableComponent, IconComponent, CommonModule],
  templateUrl: './index-actions.component.html',
})
export class IndexActionsComponent implements IIndex, OnInit {
  actions$!: Observable<Action[]>;
  loading$: any;
  ICONS = ICONS;

  constructor(
    private actionsService: ActionsService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {
    this.actions$ = this.actionsService.actions$;
    this.loading$ = this.actionsService.loading$;
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
  }

  onCreateModal(): void {
    this.modalService.show(UpsertActionsComponent, {
      class: 'modal-lg',
      initialState: {
        id: 0,
        courseId: null,
        courseTitle: null,
      },
    });
  }

  updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/actions',
        displayName: 'Ações Formativas',
        className: 'inactive',
      },
    ]);
  }
}
