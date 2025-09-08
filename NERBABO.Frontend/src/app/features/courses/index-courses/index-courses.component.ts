import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription, forkJoin, combineLatest } from 'rxjs';
import { Course, CourseKpi } from '../../../core/models/course';
import { ICONS } from '../../../core/objects/icons';
import { ReactiveFormsModule } from '@angular/forms';
import { CoursesService } from '../../../core/services/courses.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { CommonModule } from '@angular/common';
import { CoursesTableComponent } from '../../../shared/components/tables/courses-table/courses-table.component';
import { IIndex } from '../../../core/interfaces/IIndex';
import { UpsertCoursesComponent } from '../upsert-courses/upsert-courses.component';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { KpiCardComponent } from '../../../shared/components/kpi-card/kpi-card.component';

@Component({
  selector: 'app-index-courses',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    CoursesTableComponent,
    TitleComponent,
    KpiCardComponent,
  ],
  templateUrl: './index-courses.component.html',
})
export class IndexCoursesComponent implements IIndex, OnInit, OnDestroy {
  courses$!: Observable<Course[]>;
  loading$!: Observable<boolean>;
  ICONS = ICONS;
  
  // KPI properties
  aggregatedKpis: CourseKpi = {
    totalStudents: 0,
    totalApproved: 0,
    totalVolumeHours: 0,
    totalVolumeDays: 0
  };
  
  private subscriptions: Subscription = new Subscription();
  private allCoursesKpis: Map<number, CourseKpi> = new Map();

  constructor(
    private coursesService: CoursesService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {
    this.courses$ = this.coursesService.courses$;
    this.loading$ = this.coursesService.loading$;
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
    this.initializeKpis();
  }

  private initializeKpis(): void {
    // Subscribe to courses and load KPIs for each course
    this.subscriptions.add(
      this.courses$.subscribe(courses => {
        if (courses && courses.length > 0) {
          this.loadAllKpis(courses);
        } else {
          this.resetKpis();
        }
      })
    );
  }

  private loadAllKpis(courses: Course[]): void {
    const kpiRequests = courses.map(course => 
      this.coursesService.getKpis(course.id)
    );

    this.subscriptions.add(
      forkJoin(kpiRequests).subscribe({
        next: (allKpis) => {
          // Store KPIs by course ID
          courses.forEach((course, index) => {
            this.allCoursesKpis.set(course.id, allKpis[index]);
          });
          // Calculate initial aggregated KPIs (all courses)
          this.calculateAggregatedKpis(courses);
        },
        error: (error) => {
          console.error('Error loading KPIs:', error);
          this.resetKpis();
        }
      })
    );
  }

  private calculateAggregatedKpis(filteredCourses: Course[]): void {
    this.aggregatedKpis = {
      totalStudents: 0,
      totalApproved: 0,
      totalVolumeHours: 0,
      totalVolumeDays: 0
    };

    filteredCourses.forEach(course => {
      const kpi = this.allCoursesKpis.get(course.id);
      if (kpi) {
        this.aggregatedKpis.totalStudents += kpi.totalStudents;
        this.aggregatedKpis.totalApproved += kpi.totalApproved;
        this.aggregatedKpis.totalVolumeHours += kpi.totalVolumeHours;
        this.aggregatedKpis.totalVolumeDays += kpi.totalVolumeDays;
      }
    });
  }

  private resetKpis(): void {
    this.aggregatedKpis = {
      totalStudents: 0,
      totalApproved: 0,
      totalVolumeHours: 0,
      totalVolumeDays: 0
    };
  }

  // Method to be called by the table component when filters change
  onTableFilter(filteredCourses: Course[]): void {
    this.calculateAggregatedKpis(filteredCourses);
  }

  onCreateModal() {
    this.modalService.show(UpsertCoursesComponent, {
      class: 'modal-lg',
      initialState: {
        id: 0,
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
        url: '/courses',
        displayName: 'Cursos',
        className: 'inactive',
      },
    ]);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
