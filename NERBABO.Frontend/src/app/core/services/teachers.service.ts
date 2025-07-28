import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { SharedService } from './shared.service';
import { BehaviorSubject, Observable, Subject, tap } from 'rxjs';
import { Teacher, TeacherForm } from '../models/teacher';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';

@Injectable({
  providedIn: 'root',
})
export class TeachersService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private updatedSource = new Subject<TeacherForm | null>();
  private deletedSource = new Subject<number>();

  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();

  constructor(private http: HttpClient) {}

  getTeacherById(id: number): Observable<Teacher> {
    return this.http.get<Teacher>(API_ENDPOINTS.teachers + id);
  }

  getTeacherByPersonId(personId: number): Observable<Teacher> {
    return this.http.get<Teacher>(API_ENDPOINTS.teacherByPerson + personId);
  }

  upsert(model: TeacherForm, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) return this.update(model);
    return this.create(model);
  }

  private create(
    model: Omit<TeacherForm, 'id' | 'isLecturingFM' | 'isLecturingCQ'>
  ): Observable<OkResponse> {
    return this.http
      .post<OkResponse>(API_ENDPOINTS.teachers, model)
      .pipe(tap(() => this.notifyTeacherUpdate(null)));
  }

  private update(model: TeacherForm): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(API_ENDPOINTS.teachers + 'update/' + model.id, model)
      .pipe(tap(() => this.notifyTeacherUpdate(model)));
  }

  delete(id: number): Observable<OkResponse> {
    return this.http
      .delete<OkResponse>(API_ENDPOINTS.teachers + 'delete/' + id)
      .pipe(tap(() => this.notifyTeacherDelete(id)));
  }

  notifyTeacherUpdate(teacher: TeacherForm | null) {
    this.updatedSource.next(teacher);
  }

  notifyTeacherDelete(id: number) {
    this.deletedSource.next(id);
  }
}
