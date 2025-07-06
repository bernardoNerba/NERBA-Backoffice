import { Component, Input, OnInit } from '@angular/core';
import { catchError, Observable, of, tap } from 'rxjs';
import { Module } from '../../../core/models/module';
import { ICONS } from '../../../core/objects/icons';
import { ModulesService } from '../../../core/services/modules.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { Course } from '../../../core/models/course';
import { TruncatePipe } from '../../../shared/pipes/truncate.pipe';

@Component({
  selector: 'app-view-modules',
  imports: [CommonModule, IconComponent, RouterLink, TruncatePipe],
  templateUrl: './view-modules.component.html',
  styleUrl: './view-modules.component.css',
})
export class ViewModulesComponent implements OnInit {
  @Input({ required: true }) id!: number;
  module$?: Observable<Module | null>;
  courses$?: Observable<Course[] | null>;
  name?: string;
  ICONS = ICONS;
  hasActions: boolean = false;

  constructor(
    private modulesService: ModulesService,
    private sharedService: SharedService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const moduleId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(moduleId ?? '');

    if (isNaN(this.id)) {
      this.router.navigate(['/modules']);
      return;
    }

    this.initializeModule();
  }

  private initializeModule() {
    this.module$ = this.modulesService.getSingleModule(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/companies']);
          this.sharedService.showWarning('Informação não foi encontrada.');
        }
        return of(null);
      }),
      tap((module) => {
        if (module) {
          this.id = module.id;
          this.name = module.name;
          this.updateBreadcrumbs(this.id, this.name);
        }
      })
    );
    this.courses$ = this.modulesService.getCoursesByModule(this.id);
  }

  private updateBreadcrumbs(id: number, name: string): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/modules',
        displayName: 'Módulos',
        className: '',
      },
      {
        url: `/modules/${id}`,
        displayName: name.substring(0, 21) + '...' || 'Detalhes',
        className: 'inactive',
      },
    ]);
  }
}
