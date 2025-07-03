import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Company } from '../models/company';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { API_ENDPOINTS } from '../objects/apiEndpoints';

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
    return this.http.get<Company[]>(`${API_ENDPOINTS.get_companies}`);
  }
}
