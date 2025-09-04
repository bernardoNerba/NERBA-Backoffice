import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import {
  ActionEnrollment,
  CreateActionEnrollment,
  UpdateActionEnrollment,
} from '../models/actionEnrollment';
import { OkResponse } from '../models/okResponse';

@Injectable({
  providedIn: 'root',
})
export class ActionEnrollmentService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();
  private createdSource = new Subject<number>();

  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();
  createdSource$ = this.createdSource.asObservable();

  constructor(private http: HttpClient) {}

  getByActionId(actionId: number): Observable<ActionEnrollment[]> {
    return this.http.get<ActionEnrollment[]>(
      API_ENDPOINTS.actionEnrollments + 'by-action/' + actionId
    );
  }

  getById(id: number): Observable<ActionEnrollment> {
    return this.http.get<ActionEnrollment>(
      API_ENDPOINTS.actionEnrollments + id
    );
  }

  getAll(): Observable<ActionEnrollment[]> {
    this.loadingSubject.next(true);
    return this.http
      .get<ActionEnrollment[]>(API_ENDPOINTS.actionEnrollments)
      .pipe(tap(() => this.loadingSubject.next(false)));
  }

  // enrollment is any but should be CreateActionEnrollment | UpdateActionEnrollment...
  upsert(enrollment: any, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) return this.update(enrollment);
    return this.create(enrollment);
  }

  create(enrollment: CreateActionEnrollment): Observable<OkResponse> {
    this.loadingSubject.next(true);
    return this.http
      .post<OkResponse>(API_ENDPOINTS.actionEnrollments + 'create', enrollment)
      .pipe(
        tap((response) => {
          this.loadingSubject.next(false);
          this.createdSource.next((response.data as any)?.enrollmentId || 0);
        })
      );
  }

  update(enrollment: UpdateActionEnrollment): Observable<OkResponse> {
    this.loadingSubject.next(true);
    return this.http
      .put<OkResponse>(
        API_ENDPOINTS.actionEnrollments + 'update/' + enrollment.id,
        enrollment
      )
      .pipe(
        tap((response) => {
          this.loadingSubject.next(false);
          this.updatedSource.next((response.data as any)?.enrollmentId || 0);
        })
      );
  }

  delete(id: number): Observable<OkResponse> {
    this.loadingSubject.next(true);
    return this.http
      .delete<OkResponse>(API_ENDPOINTS.actionEnrollments + 'delete/' + id)
      .pipe(
        tap(() => {
          this.loadingSubject.next(false);
          this.deletedSource.next(id);
        })
      );
  }
}
