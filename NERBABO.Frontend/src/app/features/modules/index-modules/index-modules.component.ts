import { Component, OnInit } from '@angular/core';
import { ModulesService } from '../../../core/services/modules.service';
import { Observable } from 'rxjs';
import { Module } from '../../../core/models/module';
import { ICONS } from '../../../core/objects/icons';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { BsModalService } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { CreateModulesComponent } from '../create-modules/create-modules.component';
import { UpdateModulesComponent } from '../update-modules/update-modules.component';
import { DeleteModulesComponent } from '../delete-modules/delete-modules.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { CommonModule } from '@angular/common';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-index-modules',
  imports: [
    IconComponent,
    ReactiveFormsModule,
    CommonModule,
    SpinnerComponent,
    RouterLink,
  ],
  templateUrl: './index-modules.component.html',
  styleUrl: './index-modules.component.css',
})
export class IndexModulesComponent implements OnInit {
  modules$!: Observable<Module[]>;
  loading$!: Observable<boolean>;
  columns = ['#', 'UFCD', 'Total Horas', 'Status'];
  ICONS = ICONS;
  filteredModules$!: Observable<Module[]>;
  searchControl = new FormControl('');

  constructor(
    private moduleService: ModulesService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {
    this.modules$ = this.moduleService.modules$;
    this.loading$ = this.moduleService.loading$;
    this.updateBreadcrumbs();
  }
  ngOnInit(): void {
    this.filteredModules$ = this.modules$;
  }

  onAddModal() {
    this.modalService.show(CreateModulesComponent, {
      initialState: {},
      class: 'modal-lg',
    });
  }

  onUpdateModal(m: Module) {
    const initialState = {
      id: m.id,
    };
    this.modalService.show(UpdateModulesComponent, {
      initialState: initialState,
      class: 'modal-lg',
    });
  }

  onDeleteModal(id: number, name: string) {
    const initialState = {
      id: id,
      name: name,
    };
    this.modalService.show(DeleteModulesComponent, {
      initialState: initialState,
      class: 'modal-lg',
    });
  }

  private updateBreadcrumbs(): void {
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
