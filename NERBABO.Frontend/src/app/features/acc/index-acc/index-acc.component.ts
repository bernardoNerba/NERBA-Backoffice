import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { UserInfo } from '../../../core/models/userInfo';
import { AccService } from '../../../core/services/acc.service';
import { SharedService } from '../../../core/services/shared.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { CommonModule } from '@angular/common';
import { environment } from '../../../../environments/environment.development';
import { ICONS } from '../../../core/objects/icons';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { AccsTableComponent } from '../../../shared/components/tables/accs-table/accs-table.component';
import { IIndex } from '../../../core/interfaces/IIndex';
import { UpsertAccComponent } from '../upsert-acc/upsert-acc.component';
import { TitleComponent } from '../../../shared/components/title/title.component';

@Component({
  selector: 'app-index-acc',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AccsTableComponent,
    IconComponent,
    TitleComponent,
  ],
  templateUrl: './index-acc.component.html',
})
export class IndexAccComponent implements IIndex, OnInit {
  roles = `${environment.roles}`.split(','); // roles set on the environment
  users$!: Observable<UserInfo[] | null>;
  loading$!: Observable<boolean>;
  ICONS = ICONS;

  constructor(
    private accService: AccService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {
    this.users$ = this.accService.users$;
    this.loading$ = this.accService.loading$;
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
  }

  onCreateModal(): void {
    this.modalService.show(UpsertAccComponent, {
      class: 'modal-md',
      initialState: { id: 0, personId: null },
    });
  }

  updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        displayName: 'Dashboard',
        url: '/dashboard',
        className: '',
      },
      {
        displayName: 'Contas',
        url: '/accs',
        className: 'inactive',
      },
    ]);
  }
}
