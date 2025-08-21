import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { catchError, Observable, of, Subscription, tap } from 'rxjs';
import { Frame } from '../../../core/models/frame';
import { Course } from '../../../core/models/course';
import { FrameService } from '../../../core/services/frame.service';
import { CoursesService } from '../../../core/services/courses.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ICONS } from '../../../core/objects/icons';
import { BsModalService } from 'ngx-bootstrap/modal';
import { DeleteFramesComponent } from '../delete-frames/delete-frames.component';
import { MenuItem } from 'primeng/api';
import { UpsertFramesComponent } from '../upsert-frames/upsert-frames.component';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { CoursesTableComponent } from '../../../shared/components/tables/courses-table/courses-table.component';
import { IView } from '../../../core/interfaces/IView';
import { Button } from 'primeng/button';

@Component({
  selector: 'app-view-frames',
  imports: [CommonModule, TitleComponent, CoursesTableComponent, Button],
  templateUrl: './view-frames.component.html',
})
export class ViewFramesComponent implements IView, OnInit, OnDestroy {
  @Input({ required: true }) id!: number;
  frame$?: Observable<Frame | null>;
  courses$!: Observable<Course[] | []>;
  program?: string;
  ICONS = ICONS;
  hasCourses: boolean = false;
  menuItems: MenuItem[] | undefined;

  subscriptions: Subscription = new Subscription();

  constructor(
    private frameService: FrameService,
    private coursesService: CoursesService,
    private sharedService: SharedService,
    private route: ActivatedRoute,
    private router: Router,
    private modalService: BsModalService
  ) {}

  ngOnInit(): void {
    const frameId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(frameId ?? '');

    if (isNaN(this.id)) {
      this.router.navigate(['/frames']);
      return;
    }

    this.initializeEntity();
    this.initializeCoursesFromFrame();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
  }

  onUpdateModal() {
    const initialState = {
      id: this.id,
    };
    this.modalService.show(UpsertFramesComponent, {
      initialState: initialState,
      class: 'modal-lg',
    });
  }

  onDeleteModal() {
    const initialState = {
      id: this.id,
      program: this.program,
    };
    this.modalService.show(DeleteFramesComponent, { initialState });
  }

  initializeEntity() {
    this.frame$ = this.frameService.getSingle(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/frames']);
          this.sharedService.showWarning('Informação não foi encontrada.');
        }
        return of(null);
      }),
      tap((frame) => {
        if (frame) {
          this.id = frame.id;
          this.program = frame.program;
          this.updateBreadcrumbs();
          this.populateMenu();
        }
      })
    );
  }

  private initializeCoursesFromFrame() {
    this.courses$ = this.coursesService.getCoursesByFrameId(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        }
        this.hasCourses = false;
        return of([]);
      }),
      tap((courses) => {
        if (courses.length != 0) {
          this.hasCourses = true;
        }
      })
    );
  }

  populateMenu(): void {
    this.menuItems = [
      {
        label: 'Opções',
        items: [
          {
            label: 'Editar',
            icon: 'pi pi-pencil',
            command: () => this.onUpdateModal(),
          },
          {
            label: 'Eliminar',
            icon: 'pi pi-exclamation-triangle',
            command: () => this.onDeleteModal(),
          },
        ],
      },
    ];
  }

  updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/frames',
        displayName: 'Enquadramentos',
        className: '',
      },
      {
        url: `/frames/${this.id}`,
        displayName: this.program?.substring(0, 21) + '...' || 'Detalhes',
        className: 'inactive',
      },
    ]);
  }

  updateSourceSubscription() {
    this.subscriptions.add(
      this.frameService.updatedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeEntity();
          this.initializeCoursesFromFrame();
        }
      })
    );
  }

  deleteSourceSubscription() {
    this.subscriptions.add(
      this.frameService.deletedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.router.navigateByUrl('/frames');
        }
      })
    );
  }


  downloadLogo(frame: Frame, logoType: 'program' | 'financement'): void {
    this.frameService.downloadLogo(frame.id, logoType).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${frame.program}-${logoType}-logo`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        this.sharedService.showSuccess('Logo descarregado com sucesso.');
      },
      error: (error) => {
        console.error('Failed to download logo:', error);
        this.sharedService.showError('Erro ao descarregar o logo.');
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
