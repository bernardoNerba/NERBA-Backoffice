import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Course } from '../models/course';
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

  courses$ = this.coursesSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();

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

  create(
    model: Omit<Course, 'id' | 'actionsQnt' | 'modulesQnt' | 'modules'>
  ): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.courses, model);
  }
}
