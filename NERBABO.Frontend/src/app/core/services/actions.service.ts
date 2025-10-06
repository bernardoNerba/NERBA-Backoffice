import { Injectable } from '@angular/core';
import { BehaviorSubject, finalize, Observable, Subject, tap } from 'rxjs';
import { Action, ActionKpi } from '../models/action';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';
import { ActionForm } from '../models/actionForm';
import { SharedService } from './shared.service';
import { CoursesService } from './courses.service';
import { KpiCardComponent } from '../../shared/components/kpi-card/kpi-card.component';

@Injectable({
  providedIn: 'root',
})
export class ActionsService {
  private actionsSubject = new BehaviorSubject<Action[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(true);
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();

  actions$ = this.actionsSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();

  constructor(
    private http: HttpClient,
    private sharedService: SharedService,
    private coursesService: CoursesService
  ) {
    this.fetchActions();
  }

  getActionsByModuleId(id: number): Observable<Action[]> {
    return this.http.get<Action[]>(`${API_ENDPOINTS.actionsByModule}${id}`);
  }

  getActionsByCourseId(id: number): Observable<Action[]> {
    return this.http.get<Action[]>(`${API_ENDPOINTS.actionsByCourse}${id}`);
  }

  getActionsByCoordinatorId(coordinatorId: string): Observable<Action[]> {
    return this.http.get<Action[]>(
      `${API_ENDPOINTS.actionsByCoordinator}${coordinatorId}`
    );
  }

  private fetchActions(): void {
    this.loadingSubject.next(true);
    this.http
      .get<Action[]>(API_ENDPOINTS.actions)
      .pipe(finalize(() => this.loadingSubject.next(false)))
      .subscribe({
        next: (data) => {
          this.actionsSubject.next(data);
        },
        error: (err) => {
          console.error('Failed to fetch actions:', err);
          this.actionsSubject.next([]);
          if (err.status === 403 || err.status === 401) {
            this.sharedService.redirectUser();
          }
        },
      });
  }

  getActionById(id: number): Observable<Action> {
    return this.http.get<Action>(`${API_ENDPOINTS.actions}${id}`);
  }

  upsert(model: ActionForm, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) return this.updateAction(model.id, model);
    return this.createAction(model);
  }

  createAction(model: Omit<ActionForm, 'id'>): Observable<OkResponse> {
    return this.http.post<OkResponse>(`${API_ENDPOINTS.actions}`, model).pipe(
      tap(() => {
        this.notifyUpdate(0);
        this.coursesService.notifyModifiedAction(model.courseId);
      })
    );
  }

  updateAction(id: number, model: Partial<ActionForm>): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(`${API_ENDPOINTS.actions}${id}`, model)
      .pipe(
        tap(() => {
          this.notifyUpdate(id);
          this.coursesService.notifyModifiedAction(model.courseId!);
        })
      );
  }

  deleteAction(id: number, courseId: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(`${API_ENDPOINTS.actions}${id}`).pipe(
      tap(() => {
        this.notifyDelete(id);
        this.coursesService.notifyModifiedAction(courseId);
      })
    );
  }

  changeStatus(id: number, status: string): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(
        `${API_ENDPOINTS.actions}${id}/status?status=${status}`,
        {}
      )
      .pipe(
        tap(() => {
          this.notifyUpdate(id);
        })
      );
  }

  notifyUpdate(actionId: number) {
    this.updatedSource.next(actionId);
  }

  notifyDelete(actionId: number) {
    this.deletedSource.next(actionId);
  }

  triggerFetchActions() {
    this.fetchActions();
  }

  getActiveActions(): Observable<Action[]> {
    return this.http.get<Action[]>(API_ENDPOINTS.actions);
  }

  getKpis(actionId: number): Observable<ActionKpi> {
    const url = API_ENDPOINTS.kpisAction + actionId;
    console.log('KPI request URL:', url);
    return this.http.get<ActionKpi>(url);
  }
}
