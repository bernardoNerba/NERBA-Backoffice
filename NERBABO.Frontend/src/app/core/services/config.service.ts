import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Tax } from '../models/tax';
import { GeneralInfo } from '../models/generalInfo';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { OkResponse } from '../models/okResponse';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { ModuleCategory } from '../../features/global-config/module-categories/module-category.model';
import { Module } from '../models/module';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private taxesSubject = new BehaviorSubject<Array<Tax>>([]);
  private moduleCategoriesSubject = new BehaviorSubject<Array<ModuleCategory>>([]);
  private configurationInfoSubject = new BehaviorSubject<GeneralInfo | null>(
    null
  );
  private loadingSubject = new BehaviorSubject<boolean>(false);

  taxes$ = this.taxesSubject.asObservable();
  moduleCategories$ = this.moduleCategoriesSubject.asObservable();
  configurationInfo$ = this.configurationInfoSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.loadConfigs();
  }

  upsert(model: Tax, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) return this.updateTax(model);
    return this.createTax(model);
  }

  private createTax(
    model: Omit<Tax, 'id' | 'valueDecimal | isActive'>
  ): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.create_tax, model);
  }

  updateTax(model: Omit<Tax, 'valueDecimal'>): Observable<OkResponse> {
    return this.http.put<OkResponse>(
      `${API_ENDPOINTS.update_tax}${model.id}`,
      model
    );
  }

  deleteIvaTax(id: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(`${API_ENDPOINTS.delete_tax}${id}`);
  }

  fetchModuleCategory(id: number | string): Observable<ModuleCategory> {
    return this.http.get<ModuleCategory>(`${API_ENDPOINTS.get_module_categories}${id}`);
  }

  upsertModuleCategory(model: ModuleCategory, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) return this.updateModuleCategory(model);
    return this.createModuleCategory(model);
  }

  private createModuleCategory(
    model: Omit<ModuleCategory, 'id'>
  ): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.create_module_category, model);
  }

  updateModuleCategory(model: ModuleCategory): Observable<OkResponse> {
    return this.http.put<OkResponse>(
      `${API_ENDPOINTS.update_module_category}${model.id}`,
      model
    );
  }

  deleteModuleCategory(id: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(`${API_ENDPOINTS.delete_module_category}${id}`);
  }

  private loadConfigs(): void {
    this.loadingSubject.next(true);
    this.loadModuleCategories();
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

  private loadModuleCategories(): void {
    this.fetchModuleCategories().subscribe({
      next: (data: ModuleCategory[]) => {
        this.moduleCategoriesSubject.next(data);
      },
      error: (err: any) => {
        console.error('Failed to fetch module categories info', err);
        this.moduleCategoriesSubject.next([]);
        if (err.status === 403 || err.status === 401) {
          this.sharedService.redirectUser();
        }
      },
    });
  }

  private fetchTaxes(): Observable<Tax[]> {
    return this.http.get<Tax[]>(API_ENDPOINTS.get_taxes);
  }

  private fetchModuleCategories(): Observable<ModuleCategory[]> {
    return this.http.get<ModuleCategory[]>(API_ENDPOINTS.get_module_categories);
  }

  private fetchGeneralInfo(): Observable<GeneralInfo> {
    return this.http.get<GeneralInfo>(API_ENDPOINTS.get_general_conf);
  }

  fetchTaxesByType(type: string): Observable<Tax[]> {
    return this.http.get<Tax[]>(API_ENDPOINTS.get_taxes_by_type + type);
  }

  updateGeneralInfo(
    model: GeneralInfo,
    logoFile?: File
  ): Observable<OkResponse> {
    const formData = new FormData();
    formData.append('designation', model.designation);
    formData.append('site', model.site);
    formData.append('hourValueTeacher', model.hourValueTeacher.toString());
    formData.append(
      'hourValueAlimentation',
      model.hourValueAlimentation.toString()
    );
    formData.append('bankEntity', model.bankEntity);
    formData.append('iban', model.iban);
    formData.append('nipc', model.nipc);
    formData.append('ivaId', model.ivaId.toString());
    formData.append('email', model.email);
    formData.append('slug', model.slug);
    formData.append('phoneNumber', model.phoneNumber);
    formData.append('website', model.website);
    formData.append('insurancePolicy', model.insurancePolicy);
    formData.append('facilitiesCharacterization', model.facilitiesCharacterization);

    if (logoFile) {
      formData.append('logo', logoFile);
    }

    return this.http.put<OkResponse>(
      API_ENDPOINTS.update_general_conf,
      formData
    );
  }

  triggerFetchConfigs() {
    this.loadConfigs();
  }
}
