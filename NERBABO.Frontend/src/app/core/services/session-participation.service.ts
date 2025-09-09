import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import {
  SessionParticipation,
  CreateSessionParticipation,
  UpdateSessionParticipation,
  UpsertSessionAttendance,
} from '../models/sessionParticipation';

@Injectable({
  providedIn: 'root',
})
export class SessionParticipationService {
  // Reactive streams
  private createdSource = new Subject<number>();
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();

  createdSource$ = this.createdSource.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();

  constructor(private http: HttpClient) {}

  create(
    sessionParticipation: CreateSessionParticipation
  ): Observable<SessionParticipation> {
    return this.http
      .post<SessionParticipation>(
        API_ENDPOINTS.sessionParticipations,
        sessionParticipation
      )
      .pipe(tap(() => this.createdSource.next(sessionParticipation.sessionId)));
  }

  update(
    sessionParticipation: UpdateSessionParticipation
  ): Observable<SessionParticipation> {
    return this.http
      .put<SessionParticipation>(
        API_ENDPOINTS.sessionParticipations,
        sessionParticipation
      )
      .pipe(tap(() => this.updatedSource.next(sessionParticipation.sessionId)));
  }

  getAll(): Observable<SessionParticipation[]> {
    return this.http.get<SessionParticipation[]>(
      API_ENDPOINTS.sessionParticipations
    );
  }

  getById(id: number): Observable<SessionParticipation> {
    return this.http.get<SessionParticipation>(
      `${API_ENDPOINTS.sessionParticipations}${id}`
    );
  }

  getBySessionId(sessionId: number): Observable<SessionParticipation[]> {
    return this.http.get<SessionParticipation[]>(
      `${API_ENDPOINTS.sessionParticipationsBySession}${sessionId}`
    );
  }

  getByActionId(actionId: number): Observable<SessionParticipation[]> {
    return this.http.get<SessionParticipation[]>(
      `${API_ENDPOINTS.sessionParticipationsByAction}${actionId}`
    );
  }

  upsertSessionAttendance(
    upsertData: UpsertSessionAttendance
  ): Observable<SessionParticipation[]> {
    return this.http
      .post<SessionParticipation[]>(
        API_ENDPOINTS.upsertSessionAttendance,
        upsertData
      )
      .pipe(tap(() => this.updatedSource.next(upsertData.sessionId)));
  }

  delete(id: number): Observable<void> {
    return this.http
      .delete<void>(`${API_ENDPOINTS.sessionParticipations}${id}`)
      .pipe(tap(() => this.deletedSource.next(id)));
  }
}
