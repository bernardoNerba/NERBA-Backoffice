import { Injectable } from '@angular/core';
import { OkResponse } from '../models/okResponse';
import { BehaviorSubject, Observable, Subject, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { Student, StudentForm } from '../models/student';
import { PeopleService } from './people.service';

@Injectable({
  providedIn: 'root',
})
export class StudentsService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();

  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();

  constructor(private http: HttpClient, private peopleService: PeopleService) {}

  getByPersonId(personId: number): Observable<Student> {
    return this.http.get<Student>(
      API_ENDPOINTS.students + 'person/' + personId
    );
  }

  getById(id: number): Observable<Student> {
    return this.http.get<Student>(API_ENDPOINTS.students + id);
  }

  upsert(model: StudentForm, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) return this.update(model);
    return this.create(model);
  }

  delete(id: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(API_ENDPOINTS.students + id).pipe(
      tap(() => {
        this.notifyStudentDelete(id);
      })
    );
  }

  private create(model: Omit<StudentForm, 'id'>): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.students, model).pipe(
      tap(() => {
        this.notifyStudentUpdate(0);
        this.peopleService.notifyPersonUpdate(model.personId);
      })
    );
  }

  private update(model: StudentForm): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(API_ENDPOINTS.students + model.id, model)
      .pipe(
        tap(() => {
          this.notifyStudentUpdate(model.id);
          this.peopleService.notifyPersonUpdate(model.personId);
        })
      );
  }

  notifyStudentUpdate(id: number) {
    this.updatedSource.next(id);
  }

  notifyStudentDelete(id: number) {
    this.deletedSource.next(id);
  }
}
