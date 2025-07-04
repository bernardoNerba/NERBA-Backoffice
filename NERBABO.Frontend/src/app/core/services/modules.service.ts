import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Module } from '../models/module';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { API_ENDPOINTS } from '../objects/apiEndpoints';

@Injectable({
  providedIn: 'root',
})
export class ModulesService {
  private modulesSubject = new BehaviorSubject<Module[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(true);

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.fetchModules();
  }

  modules$ = this.modulesSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();

  private fetchModules(): void {
    this.loadingSubject.next(true);

    this.http.get<Module[]>(API_ENDPOINTS.modules).subscribe({
      next: (data: Module[]) => {
        this.modulesSubject.next(data);
      },
      error: (err) => {
        console.error('Failed to fetch modules', err);
        this.modulesSubject.next([]);
        if (err.status === 403 || err.status === 401) {
          this.sharedService.redirectUser();
        }
      },
    });
    this.loadingSubject.next(false);
  }
}
