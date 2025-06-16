import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Tax } from '../models/tax';
import { GeneralInfo } from '../models/generalInfo';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { OkResponse } from '../models/okResponse';
import { API_ENDPOINTS } from '../objects/apiEndpoints';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private taxesSubject = new BehaviorSubject<Array<Tax>>([]);
  private configurationInfoSubject = new BehaviorSubject<GeneralInfo | null>(
    null
  );
  private loadingSubject = new BehaviorSubject<boolean>(false);

  taxes$ = this.taxesSubject.asObservable();
  configurationInfo$ = this.configurationInfoSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.loadConfigs();
  }

  createIvaTax(
    model: Omit<Tax, 'id' | 'valueDecimal | isActive'>
  ): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.create_tax, model);
  }

  updateIvaTax(model: Omit<Tax, 'valueDecimal'>): Observable<OkResponse> {
    return this.http.put<OkResponse>(
      `${API_ENDPOINTS.update_tax}${model.id}`,
      model
    );
  }

  deleteIvaTax(id: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(`${API_ENDPOINTS.delete_tax}${id}`);
  }

  private loadConfigs(): void {
    this.loadingSubject.next(true);
    this.fetchGeneralInfo().subscribe({
      next: (data: GeneralInfo) => {
        this.configurationInfoSubject.next(data);
      },
      error: (err: any) => {
        console.error('Failed to fetch configuration info', err);
        this.configurationInfoSubject.next(null);
        if (err.status === 403 || err.status === 401) {
          this.sharedService.redirectUser();
        }
      },
    });

    this.fetchTaxes().subscribe({
      next: (data: Tax[]) => {
        this.taxesSubject.next(data);
      },
      error: (err: any) => {
        console.error('Failed to fetch iva taxes info', err);
        this.taxesSubject.next([]);
        if (err.status === 403 || err.status === 401) {
          this.sharedService.redirectUser();
        }
      },
    });

    this.loadingSubject.next(false);
  }

  private fetchTaxes(): Observable<Tax[]> {
    return this.http.get<Tax[]>(`${API_ENDPOINTS.get_taxes}`);
  }

  private fetchGeneralInfo(): Observable<GeneralInfo> {
    return this.http.get<GeneralInfo>(`${API_ENDPOINTS.get_general_conf}`);
  }

  updateGeneralInfo(model: GeneralInfo): Observable<OkResponse> {
    return this.http.put<OkResponse>(API_ENDPOINTS.update_general_conf, model);
  }

  triggerFetchConfigs() {
    this.loadConfigs();
  }
}
