import { Component, Input, OnInit } from '@angular/core';
import { Course } from '../../../core/models/course';
import { catchError, Observable, of, tap } from 'rxjs';
import { ICONS } from '../../../core/objects/icons';
import { Frame } from '../../../core/models/frame';
import { CoursesService } from '../../../core/services/courses.service';
import { SharedService } from '../../../core/services/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FrameService } from '../../../core/services/frame.service';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { CommonModule } from '@angular/common';
import { MessageModule } from 'primeng/message';

@Component({
  selector: 'app-view-courses',
  imports: [IconComponent, CommonModule, MessageModule],
  templateUrl: './view-courses.component.html',
  styleUrl: './view-courses.component.css',
})
export class ViewCoursesComponent implements OnInit {
  @Input({ required: true }) id!: number;
  course$?: Observable<Course | null>;
  frame!: Frame;
  title?: string;
  frameId!: number;
  ICONS = ICONS;

  constructor(
    private coursesService: CoursesService,
    private frameService: FrameService,
    private sharedService: SharedService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const courseId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(courseId ?? '');
    if (isNaN(this.id)) {
      this.router.navigate(['/courses']);
      return;
    }

    this.initializeCourse();
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
}
