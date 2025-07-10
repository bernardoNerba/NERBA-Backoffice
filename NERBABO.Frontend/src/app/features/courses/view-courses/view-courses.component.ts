import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Course } from '../../../core/models/course';
import { catchError, Observable, of, Subscription, tap } from 'rxjs';
import { ICONS } from '../../../core/objects/icons';
import { Frame } from '../../../core/models/frame';
import { CoursesService } from '../../../core/services/courses.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FrameService } from '../../../core/services/frame.service';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { CommonModule } from '@angular/common';
import { MessageModule } from 'primeng/message';
import { TruncatePipe } from '../../../shared/pipes/truncate.pipe';
import { BsModalService } from 'ngx-bootstrap/modal';
import { UpdateCoursesComponent } from '../update-courses/update-courses.component';
import { DeleteCoursesComponent } from '../delete-courses/delete-courses.component';
import { ChangeStatusCoursesComponent } from '../change-status-courses/change-status-courses.component';
import { AssignModuleCoursesComponent } from '../assign-module-courses/assign-module-courses.component';

@Component({
  selector: 'app-view-courses',
  imports: [
    IconComponent,
    CommonModule,
    MessageModule,
    TruncatePipe,
    RouterLink,
  ],
  templateUrl: './view-courses.component.html',
  styleUrl: './view-courses.component.css',
})
export class ViewCoursesComponent implements OnInit, OnDestroy {
  @Input({ required: true }) id!: number;
  course$?: Observable<Course | null>;
  frame!: Frame;
  title?: string;
  frameId!: number;
  ICONS = ICONS;

  private subscriptions: Subscription = new Subscription();

  constructor(
    private coursesService: CoursesService,
    private frameService: FrameService,
    private sharedService: SharedService,
    private route: ActivatedRoute,
    private router: Router,
    private modalService: BsModalService
  ) {}

  ngOnInit(): void {
    const courseId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(courseId ?? '');
    if (isNaN(this.id)) {
      this.router.navigate(['/courses']);
      return;
    }

    this.initializeCourse();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
    this.assignModuleSourceSubscription();
    this.changeStatusSourceSubscription();
  }

  onUpdateCourseModal(course: Course) {
    this.modalService.show(UpdateCoursesComponent, {
      class: 'modal-lg',
      initialState: {
        id: course.id,
        course: course,
      },
    });
  }
  onDeleteCourseModal(id: number, title: string) {
    this.modalService.show(DeleteCoursesComponent, {
      class: 'modal-lg',
      initialState: {
        id: id,
        title: title,
      },
    });
  }

  onChangeStatusModal(course: Course) {
    this.modalService.show(ChangeStatusCoursesComponent, {
      class: 'modal-md',
      initialState: {
        course: course,
      },
    });
  }

  onAssignModuleModal(course: Course) {
    this.modalService.show(AssignModuleCoursesComponent, {
      class: 'modal-md',
      initialState: {
        course: course,
      },
    });
  }

  private initializeCourse() {
    this.course$ = this.coursesService.getSingleCourse(this.id).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/courses']);
          this.sharedService.showWarning('Informação não encontrada.');
        }
        return of(null);
      }),
      tap((course) => {
        if (course) {
          this.id = course.id;
          this.title = course.title;

          this.updateBreadcrumbs(this.id, this.title);

          this.frameService.getSingle(course.frameId).subscribe({
            next: (frame: Frame) => {
              console.log(frame);
              this.frame = frame;
            },
            error: (error: any) => {
              this.sharedService.showError('Sem enquadramento associado.');
            },
          });
        }
      })
    );
  }

  private updateBreadcrumbs(id: number, title: string): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/courses',
        displayName: 'Cursos',
        className: '',
      },
      {
        url: `/courses/${id}`,
        displayName:
          title.length > 21
            ? title.substring(0, 21) + '...'
            : title || 'Detalhes',
        className: 'inactive',
      },
    ]);
  }

  private updateSourceSubscription() {
    this.subscriptions.add(
      this.coursesService.updatedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeCourse();
        }
      })
    );
  }

  private deleteSourceSubscription() {
    this.subscriptions.add(
      this.coursesService.deletedSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.router.navigateByUrl('/courses');
        }
      })
    );
  }

  private assignModuleSourceSubscription() {
    this.subscriptions.add(
      this.coursesService.assignModuleSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeCourse();
        }
      })
    );
  }

  private changeStatusSourceSubscription() {
    this.subscriptions.add(
      this.coursesService.changeStatusSource$.subscribe((id: number) => {
        if (this.id === id) {
          this.initializeCourse();
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
