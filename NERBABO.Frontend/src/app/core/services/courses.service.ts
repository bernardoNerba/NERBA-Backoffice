import { Injectable } from '@angular/core';
import { BehaviorSubject, finalize, Observable, Subject, tap } from 'rxjs';
import { Course, CourseKpi } from '../models/course';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { SharedService } from './shared.service';
import { OkResponse } from '../models/okResponse';

@Injectable({
  providedIn: 'root',
})
export class CoursesService {
  coursesSubject = new BehaviorSubject<Course[]>([]);
  loadingSubject = new BehaviorSubject<boolean>(true);
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();
  private changeStatusSource = new Subject<number>();
  private assignModuleSource = new Subject<number>();
  private modifiedActionSource = new Subject<number>();

  courses$ = this.coursesSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();
  changeStatusSource$ = this.changeStatusSource.asObservable();
  assignModuleSource$ = this.assignModuleSource.asObservable();
  modifiedActionSource$ = this.modifiedActionSource.asObservable();

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.fetchCourses();
  }

  triggerFetchCourses() {
    this.fetchCourses();
  }

  private fetchCourses(): void {
    this.loadingSubject.next(true);

    this.http.get<Course[]>(API_ENDPOINTS.courses).subscribe({
      next: (data: Course[]) => {
        this.coursesSubject.next(data);
      },
      error: (error) => {
        console.error('Failed to fetch courses:', error);
        this.coursesSubject.next([]);
        if (error.status === 403 || error.status === 401) {
          this.sharedService.redirectUser();
        }
      },
    });
    this.loadingSubject.next(false);
  }

  upsert(model: Course, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) return this.update(model, model.id);
    return this.create(model);
  }

  getActive(): Observable<Course[]> {
    return this.http.get<Course[]>(API_ENDPOINTS.courses_active);
  }

  private create(
    model: Omit<Course, 'id' | 'actionsQnt' | 'modulesQnt'>
  ): Observable<OkResponse> {
    return this.http
      .post<OkResponse>(API_ENDPOINTS.courses, model)
      .pipe(tap(() => this.notifyCourseUpdate(0)));
  }

  private update(
    model: Omit<Course, 'actionsQnt' | 'modulesQnt'>,
    id: number
  ): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(API_ENDPOINTS.courses + id, model)
      .pipe(tap(() => this.notifyCourseUpdate(id)));
  }

  delete(id: number): Observable<OkResponse> {
    return this.http
      .delete<OkResponse>(API_ENDPOINTS.courses + id)
      .pipe(tap(() => this.notifyCourseDelete(id)));
  }

  changeStatus(id: number, status: string): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(
        `${API_ENDPOINTS.courses}${id}/status?status=${status}`,
        {}
      )
      .pipe(tap(() => this.notifyCourseChangeStatus(id)));
  }

  getSingleCourse(id: number): Observable<Course> {
    return this.http.get<Course>(API_ENDPOINTS.courses + id);
  }

  getCoursesByFrameId(frameId: number): Observable<Course[]> {
    return this.http.get<Course[]>(`${API_ENDPOINTS.courses}frame/${frameId}`);
  }

  assignModules(moduleIds: number[], courseId: number): Observable<OkResponse> {
    return this.http.put<OkResponse>(
      `${API_ENDPOINTS.courses}${courseId}/modules`,
      moduleIds
    );
  }

  getKpis(courseId: number): Observable<CourseKpi> {
    const url = API_ENDPOINTS.kpisCourse + courseId;

    return this.http.get<CourseKpi>(url);
  }

  notifyCourseUpdate(id: number) {
    this.updatedSource.next(id);
  }

  notifyCourseDelete(id: number) {
    this.deletedSource.next(id);
  }

  notifyCourseChangeStatus(id: number) {
    this.changeStatusSource.next(id);
  }

  notifyCourseAssignModule(id: number) {
    this.assignModuleSource.next(id);
  }

  notifyModifiedAction(id: number) {
    this.modifiedActionSource.next(id);
  }
}
