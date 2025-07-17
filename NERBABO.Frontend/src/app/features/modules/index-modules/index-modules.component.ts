import { Component, OnInit } from '@angular/core';
import { ModulesService } from '../../../core/services/modules.service';
import { Observable } from 'rxjs';
import { Module } from '../../../core/models/module';
import { ICONS } from '../../../core/objects/icons';
import { ReactiveFormsModule } from '@angular/forms';
import { BsModalService } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { CreateModulesComponent } from '../create-modules/create-modules.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { CommonModule } from '@angular/common';
import { ModulesTableComponent } from '../../../shared/components/tables/modules-table/modules-table.component';
import { IIndex } from '../../../core/interfaces/iindex';

@Component({
  selector: 'app-index-modules',
  imports: [
    IconComponent,
    ReactiveFormsModule,
    CommonModule,
    ModulesTableComponent,
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
    private sharedService: SharedService
  ) {
    this.modules$ = this.moduleService.modules$;
    this.loading$ = this.moduleService.loading$;
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
  }

  onCreateModal(): void {
    this.modalService.show(CreateModulesComponent, {
      initialState: {},
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
}
