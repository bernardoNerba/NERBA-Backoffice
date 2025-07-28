import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject, tap } from 'rxjs';
import { Company } from '../models/company';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';
import { Student } from '../models/student';

@Injectable({
  providedIn: 'root',
})
export class CompaniesService {
  private companiesSubject = new BehaviorSubject<Company[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.loadCompanies();
  }

  companies$ = this.companiesSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();

  private loadCompanies(): void {
    this.loadingSubject.next(true);

    this.fetchCompanies().subscribe({
      next: (data: Company[]) => {
        this.companiesSubject.next(data);
      },
      error: (err: any) => {
        this.companiesSubject.next([]);
        if (err.status === 403 || err.status === 401) {
          this.sharedService.redirectUser();
        }
      },
    });

    this.loadingSubject.next(false);
  }

  private fetchCompanies(): Observable<Company[]> {
    return this.http.get<Company[]>(`${API_ENDPOINTS.companies}`);
  }

  triggerFetch() {
    this.loadCompanies();
  }

  upsert(model: Company, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) return this.updateCompany(model, model.id);
    return this.createCompany(model);
  }

  createCompany(model: Omit<Company, 'id'>): Observable<OkResponse> {
    return this.http
      .post<OkResponse>(`${API_ENDPOINTS.companies}`, model)
      .pipe(tap(() => this.notifyUpdate(0)));
  }

  updateCompany(model: Company, id: number) {
    return this.http
      .put<OkResponse>(`${API_ENDPOINTS.companies}${id}`, model)
      .pipe(tap(() => this.notifyUpdate(id)));
  }

  deleteCompany(id: number) {
    return this.http
      .delete<OkResponse>(`${API_ENDPOINTS.companies}${id}`)
      .pipe(tap(() => this.notifyDelete(id)));
  }

  getCompanyById(id: number) {
    return this.http.get<Company>(`${API_ENDPOINTS.companies}${id}`);
  }

  getStudentsByCompanyId(id: number) {
    return this.http.get<Student[]>(`${API_ENDPOINTS.studentsByCompany}${id}`);
  }

  notifyUpdate(id: number) {
    this.updatedSource.next(id);
  }

  notifyDelete(id: number) {
    this.deletedSource.next(id);
  }
}
