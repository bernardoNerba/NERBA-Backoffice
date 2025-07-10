import { Component, OnInit } from '@angular/core';
import { ModulesService } from '../../../core/services/modules.service';
import {
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  map,
  Observable,
  startWith,
} from 'rxjs';
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
import { ActiveBadgeComponent } from '../../../shared/components/badges/active-badge/active-badge.component';

@Component({
  selector: 'app-index-modules',
  imports: [
    IconComponent,
    ReactiveFormsModule,
    CommonModule,
    SpinnerComponent,
    RouterLink,
    ActiveBadgeComponent,
  ],
  templateUrl: './index-modules.component.html',
  styleUrl: './index-modules.component.css',
})
export class IndexModulesComponent implements OnInit {
  modules$!: Observable<Module[]>;
  loading$!: Observable<boolean>;
  columns = ['#', 'UFCD', 'Total Horas', 'Status'];
  ICONS = ICONS;
  filteredModules$!: Observable<Module[] | null>;
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
    this.filteredModules$ = combineLatest([
      this.modules$,
      this.searchControl.valueChanges.pipe(
        startWith(''), // Emit an initial value to trigger the stream
        debounceTime(300), // Wait for a pause in typing before emitting
        distinctUntilChanged() // Only emit if the value has changed
      ),
    ]).pipe(
      map(([modules, searchTerm]) => {
        // If the modules array is null or undefined, return null
        if (!modules) {
          return null;
        }

        // Normalize the search term
        const term = (searchTerm || '').toLowerCase().trim();

        // If the search term is empty, return the original full list
        if (!term) {
          return modules;
        }

        // Filter the modules based on the search term
        return modules.filter((module) =>
          module.name.toLowerCase().includes(term)
        );
      })
    );
  }

  onAddModal(): void {
    this.modalService.show(CreateModulesComponent, {
      initialState: {},
      class: 'modal-md',
    });
  }

  onUpdateModal(m: Module): void {
    const initialState = {
      id: m.id,
    };
    this.modalService.show(UpdateModulesComponent, {
      initialState: initialState,
      class: 'modal-md',
    });
  }

  onDeleteModal(id: number, name: string): void {
    const initialState = {
      id: id,
      name: name,
    };
    this.modalService.show(DeleteModulesComponent, {
      initialState: initialState,
      class: 'modal-md',
    });
  }

  onToggleModule(id: number): void {
    this.moduleService.toggleModuleIsActive(id);
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
