import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Tax } from '../models/tax';
import { GeneralInfo } from '../models/generalInfo';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { environment } from '../../../environments/environment.development';
import { OkResponse } from '../models/okResponse';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private ivaTaxesSubject = new BehaviorSubject<Array<Tax>>([]);
  private configurationInfoSubject = new BehaviorSubject<GeneralInfo | null>(
    null
  );
  private loadingSubject = new BehaviorSubject<boolean>(false);

  ivaTaxes$ = this.ivaTaxesSubject.asObservable();
  configurationInfo$ = this.configurationInfoSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.loadGeneralInfo();
  }

  createIvaTax(
    model: Omit<Tax, 'id' | 'valueDecimal | isActive'>
  ): Observable<OkResponse> {
    return this.http.post<OkResponse>(
      `${environment.appUrl}/api/tax/create`,
      model
    );
  }

  updateIvaTax(model: Omit<Tax, 'valueDecimal'>): Observable<OkResponse> {
    return this.http.put<OkResponse>(
      `${environment.appUrl}/api/tax/update/${model.id}`,
      model
    );
  }

  deleteIvaTax(id: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(
      `${environment.appUrl}/api/tax/delete/${id}`
    );
  }

  private loadGeneralInfo(): void {
    this.loadingSubject.next(true);
    this.fetchConfigurationInfo().subscribe({
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
        this.ivaTaxesSubject.next(data);
      },
      error: (err: any) => {
        console.error('Failed to fetch iva taxes info', err);
        this.ivaTaxesSubject.next([]);
        if (err.status === 403 || err.status === 401) {
          this.sharedService.redirectUser();
        }
      },
    });

    this.loadingSubject.next(false);
  }

  private fetchConfigurationInfo(): Observable<GeneralInfo> {
    return this.http.get<GeneralInfo>(`${environment.appUrl}/api/generalinfo/`);
  }

  private fetchTaxes(): Observable<Array<Tax>> {
    return this.http.get<Array<Tax>>(
      `${environment.appUrl}/api/generalinfo/update`
    );
  }
}
