import { Injectable } from '@angular/core';
import { OkResponse } from '../models/okResponse';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { Student, StudentForm } from '../models/student';

@Injectable({
  providedIn: 'root',
})
export class StudentsService {
  constructor(private http: HttpClient) {}

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
    return this.http.delete<OkResponse>(API_ENDPOINTS.students + id);
  }

  private create(model: Omit<StudentForm, 'id'>): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.students, model);
  }

  private update(model: StudentForm): Observable<OkResponse> {
    return this.http.put<OkResponse>(API_ENDPOINTS.students + model.id, model);
  }
}
