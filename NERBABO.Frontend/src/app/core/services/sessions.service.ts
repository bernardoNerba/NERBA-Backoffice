import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { SharedService } from './shared.service';
import { OkResponse } from '../models/okResponse';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { CreateSession, Session } from '../objects/sessions';

@Injectable({
  providedIn: 'root',
})
export class SessionsService {
  private loadingSubject = new BehaviorSubject<boolean>(true);
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();

  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();

  constructor(private http: HttpClient, private sharedService: SharedService) {}

  getSessionsByActionId(actionId: number): Observable<Session[]> {
    return this.http.get<Session[]>(API_ENDPOINTS.sessionsByAction + actionId);
  }

  create(session: CreateSession): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.sessions, session);
  }

  delete(sessionId: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(API_ENDPOINTS.sessions + sessionId);
  }
}
