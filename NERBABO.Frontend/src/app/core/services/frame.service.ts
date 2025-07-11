import { Injectable } from '@angular/core';
import { Frame } from '../models/frame';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { environment } from '../../../environments/environment.development';
import { OkResponse } from '../models/okResponse';
import { API_ENDPOINTS } from '../objects/apiEndpoints';

@Injectable({
  providedIn: 'root',
})
export class FrameService {
  private framesSubject = new BehaviorSubject<Frame[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  frames$ = this.framesSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.loadFrames();
  }

  loadFrames(): void {
    this.loadingSubject.next(true);
    this.fetchFrames().subscribe({
      next: (data: Frame[]) => {
        this.framesSubject.next(data);
      },
      error: (err: any) => {
        console.error('Failed to fetch people:', err);
        this.framesSubject.next([]);
        if (err.status === 401 || err.status === 403) {
          this.sharedService.redirectUser();
        }
      },
    });
    this.loadingSubject.next(false);
  }

  private fetchFrames(): Observable<Frame[]> {
    return this.http.get<Frame[]>(API_ENDPOINTS.frames);
  }

  getSingle(id: number): Observable<Frame> {
    return this.http.get<Frame>(API_ENDPOINTS.frames + id);
  }

  create(frame: Omit<Frame, 'id'>): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.frames + `create`, frame);
  }

  update(id: number, frame: Frame): Observable<OkResponse> {
    return this.http.put<OkResponse>(
      API_ENDPOINTS.frames + `update/${id}`,
      frame
    );
  }

  delete(id: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(API_ENDPOINTS.frames + `delete/${id}`);
  }

  triggerFetchFrames() {
    this.loadFrames();
  }
}
