import { Injectable } from '@angular/core';
import { Frame, CreateFrameData, UpdateFrameData } from '../models/frame';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';
import { tap, finalize } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class FrameService {
  private framesSubject = new BehaviorSubject<Frame[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();

  frames$ = this.framesSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.loadFrames();
  }

  loadFrames(): void {
    this.loadingSubject.next(true);
    this.fetchFrames()
      .pipe(finalize(() => this.loadingSubject.next(false)))
      .subscribe({
        next: (data: Frame[]) => {
          this.framesSubject.next(data);
        },
        error: (err: any) => {
          console.error('Failed to fetch frames:', err);
          this.framesSubject.next([]);
          if (err.status === 401 || err.status === 403) {
            this.sharedService.redirectUser();
          }
        },
      });
  }

  private fetchFrames(): Observable<Frame[]> {
    return this.http.get<Frame[]>(API_ENDPOINTS.frames);
  }

  getSingle(id: number): Observable<Frame> {
    return this.http.get<Frame>(`${API_ENDPOINTS.frames}${id}`);
  }

  upsert(model: CreateFrameData | UpdateFrameData, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) return this.update(model as UpdateFrameData);
    return this.create(model as CreateFrameData);
  }

  create(frameData: CreateFrameData): Observable<OkResponse> {
    const formData = this.createFormData(frameData);
    return this.http.post<OkResponse>(`${API_ENDPOINTS.frames}create`, formData);
  }

  update(frameData: UpdateFrameData): Observable<OkResponse> {
    const formData = this.createFormData(frameData);
    return this.http
      .put<OkResponse>(`${API_ENDPOINTS.frames}update/${frameData.id}`, formData)
      .pipe(
        tap(() => this.notifyFrameUpdate(frameData.id)) // Notify update after success
      );
  }

  private createFormData(frameData: CreateFrameData | UpdateFrameData): FormData {
    const formData = new FormData();
    
    // Add text fields
    formData.append('program', frameData.program);
    formData.append('intervention', frameData.intervention);
    formData.append('interventionType', frameData.interventionType);
    formData.append('operation', frameData.operation);
    formData.append('operationType', frameData.operationType);
    
    // Add id for updates
    if ('id' in frameData) {
      formData.append('id', frameData.id.toString());
    }
    
    // Add file fields
    if (frameData.programLogoFile) {
      formData.append('programLogoFile', frameData.programLogoFile);
    }
    
    if (frameData.financementLogoFile) {
      formData.append('financementLogoFile', frameData.financementLogoFile);
    }
    
    return formData;
  }

  downloadLogo(frameId: number, logoType: 'program' | 'financement'): Observable<Blob> {
    return this.http.get(`${API_ENDPOINTS.frames}${frameId}/${logoType}-logo`, {
      responseType: 'blob'
    });
  }

  delete(id: number): Observable<OkResponse> {
    return this.http
      .delete<OkResponse>(`${API_ENDPOINTS.frames}delete/${id}`)
      .pipe(
        tap(() => this.notifyFrameDelete(id)) // Notify delete after success
      );
  }

  notifyFrameUpdate(id: number) {
    this.updatedSource.next(id);
  }

  notifyFrameDelete(id: number) {
    this.deletedSource.next(id);
  }

  triggerFetchFrames() {
    this.loadFrames();
  }
}
