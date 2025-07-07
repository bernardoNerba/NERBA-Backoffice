import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Action } from '../models/action';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../objects/apiEndpoints';

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
}
