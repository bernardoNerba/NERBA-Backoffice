import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, tap } from 'rxjs';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';
import {
  ModuleTeaching,
  CreateModuleTeaching,
  UpdateModuleTeaching,
  MinimalModuleTeaching,
  ProcessModuleTeachingPayment,
} from '../models/moduleTeaching';

@Injectable({
  providedIn: 'root',
})
export class ModuleTeachingService {
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();
  private createdSource = new Subject<ModuleTeaching>();

  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();
  createdSource$ = this.createdSource.asObservable();

  constructor(private http: HttpClient) {}

  getAllModuleTeachings(): Observable<ModuleTeaching[]> {
    return this.http.get<ModuleTeaching[]>(API_ENDPOINTS.moduleTeachings);
  }

  getModuleTeachingById(id: number): Observable<ModuleTeaching> {
    return this.http.get<ModuleTeaching>(
      `${API_ENDPOINTS.moduleTeachings}${id}`
    );
  }

  createModuleTeaching(model: CreateModuleTeaching): Observable<OkResponse> {
    return this.http
      .post<OkResponse>(`${API_ENDPOINTS.moduleTeachings}create`, model)
      .pipe(
        tap(() => {
          this.notifyModuleTeachingCreated();
        })
      );
  }

  updateModuleTeaching(model: UpdateModuleTeaching): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(
        `${API_ENDPOINTS.moduleTeachings}update/${model.id}`,
        model
      )
      .pipe(
        tap(() => {
          this.notifyModuleTeachingUpdate(model.id);
        })
      );
  }

  deleteModuleTeaching(id: number): Observable<OkResponse> {
    return this.http
      .delete<OkResponse>(`${API_ENDPOINTS.moduleTeachings}delete/${id}`)
      .pipe(
        tap(() => {
          this.notifyModuleTeachingDelete(id);
        })
      );
  }

  upsert(
    model: CreateModuleTeaching | UpdateModuleTeaching,
    isUpdate: boolean
  ): Observable<OkResponse> {
    if (isUpdate) {
      return this.updateModuleTeaching(model as UpdateModuleTeaching);
    }
    return this.createModuleTeaching(model as CreateModuleTeaching);
  }

  getModuleTeachingByActionAndModule(
    actionId: number,
    moduleId: number
  ): Observable<ModuleTeaching> {
    return this.http.get<ModuleTeaching>(
      `${API_ENDPOINTS.moduleTeachings}action/${actionId}/module/${moduleId}`
    );
  }

  getModuleTeachingByActionMinimal(
    actionId: number
  ): Observable<MinimalModuleTeaching[]> {
    return this.http.get<MinimalModuleTeaching[]>(
      `${API_ENDPOINTS.moduleTeachings}action/${actionId}/`
    );
  }

  getAllProcessModuleTeachingPayments(
    actionId: number
  ): Observable<ProcessModuleTeachingPayment[]> {
    return this.http.get<ProcessModuleTeachingPayment[]>(
      `${API_ENDPOINTS.processModuleTeachingPayment}${actionId}/`
    );
  }

  notifyModuleTeachingCreated() {
    this.createdSource.next({} as ModuleTeaching);
  }

  notifyModuleTeachingUpdate(id: number) {
    this.updatedSource.next(id);
  }

  notifyModuleTeachingDelete(id: number) {
    this.deletedSource.next(id);
  }
}
