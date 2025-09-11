import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import {
  AvaliationByModule,
  UpdateModuleAvaliation,
} from '../models/moduleAvaliation';
import { API_ENDPOINTS } from '../objects/apiEndpoints';

@Injectable({
  providedIn: 'root',
})
export class ModuleAvaliationsService {
  private updatedSource$ = new Subject<void>();
  public updated$ = this.updatedSource$.asObservable();

  constructor(private http: HttpClient) {}

  getByActionId(actionId: number): Observable<AvaliationByModule[]> {
    return this.http.get<AvaliationByModule[]>(
      API_ENDPOINTS.moduleAvaliationsByActionId + actionId
    );
  }

  updateModuleAvaliation(
    id: number,
    data: UpdateModuleAvaliation
  ): Observable<any> {
    return this.http.put(API_ENDPOINTS.moduleAvaliations, data);
  }

  private notifyUpdated(): void {
    this.updatedSource$.next();
  }

  public triggerUpdate(): void {
    this.notifyUpdated();
  }
}
