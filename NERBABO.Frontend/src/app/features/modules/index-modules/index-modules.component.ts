import { Component, OnInit } from '@angular/core';
import { ModulesService } from '../../../core/services/modules.service';
import { Observable } from 'rxjs';
import { Module } from '../../../core/models/module';
import { ICONS } from '../../../core/objects/icons';
import { ReactiveFormsModule } from '@angular/forms';
import { BsModalService } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { CommonModule } from '@angular/common';
import { ModulesTableComponent } from '../../../shared/components/tables/modules-table/modules-table.component';
import { IIndex } from '../../../core/interfaces/IIndex';
import { UpsertModulesComponent } from '../upsert-modules/upsert-modules.component';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-index-modules',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    ModulesTableComponent,
    TitleComponent,
  ],
  templateUrl: './index-modules.component.html',
})
export class IndexModulesComponent implements IIndex, OnInit {
  modules$!: Observable<Module[]>;
  loading$!: Observable<boolean>;
  ICONS = ICONS;

  constructor(
    private moduleService: ModulesService,
    private modalService: BsModalService,
    private sharedService: SharedService,
    private authService: AuthService
  ) {
    this.modules$ = this.moduleService.modules$;
    this.loading$ = this.moduleService.loading$;
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
  }

  onCreateModal(): void {
    this.modalService.show(UpsertModulesComponent, {
      initialState: { id: 0 },
      class: 'modal-md',
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
        url: '/modules',
        displayName: 'Módulos',
        className: 'inactive',
      },
    ]);
  }

  canPerformAction(): boolean {
    return (
      this.authService.userRoles.includes('Admin') ||
      this.authService.userRoles.includes('FM')
    );
  }
}
