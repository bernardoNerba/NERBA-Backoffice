import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Company } from '../models/company';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';

@Injectable({
  providedIn: 'root',
})
export class CompaniesService {
  private companiesSubject = new BehaviorSubject<Company[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.loadCompanies();
  }

  comapnies$ = this.companiesSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();

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

  createCompany(model: Omit<Company, 'id'>): Observable<OkResponse> {
    return this.http.post<OkResponse>(`${API_ENDPOINTS.companies}`, model);
  }

  updateCompany(model: Company, id: number) {
    return this.http.put<OkResponse>(`${API_ENDPOINTS.companies}${id}`, model);
  }

  deleteCompany(id: number) {
    return this.http.delete<OkResponse>(`${API_ENDPOINTS.companies}${id}`);
  }
}
