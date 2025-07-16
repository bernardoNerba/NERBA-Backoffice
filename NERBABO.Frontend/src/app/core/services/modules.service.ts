import { Injectable } from '@angular/core';
import { BehaviorSubject, finalize, Observable, Subject } from 'rxjs';
import { Module } from '../models/module';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';
import { Course } from '../models/course';

@Injectable({
  providedIn: 'root',
})
export class ModulesService {
  private modulesSubject = new BehaviorSubject<Module[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(true);
  private updatedSource = new Subject<number>();
  private deleteSource = new Subject<number>();
  private toggleSource = new Subject<number>();

  modules$ = this.modulesSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deleteSource.asObservable();
  toggleSource$ = this.toggleSource.asObservable();

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.fetchModules(); // Fetch all modules for index-modules view
  }

  createModule(model: Omit<Module, 'id'>): Observable<OkResponse> {
    return this.http.post<OkResponse>(`${API_ENDPOINTS.modules}`, model);
  }

  toggleModuleIsActive(id: number): void {
    this.http
      .put<OkResponse>(`${API_ENDPOINTS.modules}${id}/toggle`, {})
      .subscribe({
        next: (value) => {
          this.notifyModuleToggle(id); // Notify toggle instead of full fetch
          this.sharedService.showSuccess(value.message);
        },
        error: (error) => {
          console.log(error);
          this.sharedService.handleErrorResponse(error);
        },
      });
  }

  updateModule(id: number, model: Module): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(`${API_ENDPOINTS.modules}${id}`, model)
      .pipe(finalize(() => this.notifyModuleUpdate(id))); // Notify update after success
  }

  getSingleModule(id: number): Observable<Module> {
    return this.http.get<Module>(`${API_ENDPOINTS.modules}${id}`);
  }

  getActiveModules(): Observable<Module[]> {
    return this.http.get<Module[]>(API_ENDPOINTS.modules_active);
  }

  deleteModule(id: number): Observable<OkResponse> {
    return this.http
      .delete<OkResponse>(`${API_ENDPOINTS.modules}${id}`)
      .pipe(finalize(() => this.notifyModuleDelete(id))); // Notify delete after success
  }

  getCoursesByModule(id: number): Observable<Course[]> {
    return this.http.get<Course[]>(`${API_ENDPOINTS.courses}module/${id}`);
  }

  notifyModuleUpdate(id: number) {
    this.updatedSource.next(id);
  }

  notifyModuleDelete(id: number) {
    this.deleteSource.next(id);
  }

  notifyModuleToggle(id: number) {
    this.toggleSource.next(id);
  }

  private fetchModules(): void {
    this.loadingSubject.next(true);
    this.http.get<Module[]>(API_ENDPOINTS.modules).subscribe({
      next: (data: Module[]) => {
        this.modulesSubject.next(data);
        this.loadingSubject.next(false);
      },
      error: (err) => {
        console.error('Failed to fetch modules', err);
        this.modulesSubject.next([]);
        this.loadingSubject.next(false);
        if (err.status === 403 || err.status === 401) {
          this.sharedService.redirectUser();
        }
      },
    });
  }

  triggerFetch() {
    this.fetchModules();
  }
}
