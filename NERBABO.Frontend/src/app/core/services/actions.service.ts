import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Action } from '../models/action';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';
import { ActionForm } from '../models/actionForm';

@Injectable({
  providedIn: 'root',
})
export class ActionsService {
  private actionsSubject = new BehaviorSubject<Action[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(true);

  actions$ = this.actionsSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient) {}

  getActionsByModuleId(id: number): Observable<Action[]> {
    return this.http.get<Action[]>(`${API_ENDPOINTS.actionsByModule}${id}`);
  }

  getActionsByCourseId(id: number): Observable<Action[]> {
    return this.http.get<Action[]>(`${API_ENDPOINTS.actionsByCourse}${id}`);
  }

  create(model: Omit<ActionForm, 'id'>): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.actions, model);
  }
}
