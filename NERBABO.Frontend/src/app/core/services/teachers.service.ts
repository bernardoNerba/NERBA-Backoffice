import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { SharedService } from './shared.service';
import { Observable } from 'rxjs';
import { Teacher } from '../objects/teacher';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';

@Injectable({
  providedIn: 'root',
})
export class TeachersService {
  constructor(private http: HttpClient, private sharedService: SharedService) {}

  getTeacherById(id: number): Observable<Teacher> {
    return this.http.get<Teacher>(API_ENDPOINTS.teachers + id);
  }

  getTeacherByPersonId(personId: number): Observable<Teacher> {
    return this.http.get<Teacher>(API_ENDPOINTS.teacherByPerson + personId);
  }

  upsert(model: Teacher, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) return this.update(model);
    return this.create(model);
  }

  private create(
    model: Omit<Teacher, 'id' | 'isLecturingFM' | 'isLecturingCQ'>
  ): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.teachers, model);
  }

  private update(model: Teacher): Observable<OkResponse> {
    return this.http.put<OkResponse>(API_ENDPOINTS.teachers + model.id, model);
  }
}
