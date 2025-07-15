import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Register } from '../models/register';
import { OkResponse } from '../models/okResponse';
import { UserInfo } from '../models/userInfo';
import { SharedService } from './shared.service';
import { UserUpdate } from '../models/userUpdate';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { finalize } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class AccService {
  private usersSubject = new BehaviorSubject<UserInfo[] | null>(null);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private updatedSource = new Subject<string>();
  private deletedSource = new Subject<string>();

  users$ = this.usersSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.fetchUsers();
  }

  register(model: Register): Observable<OkResponse> {
    return this.http
      .post<OkResponse>(API_ENDPOINTS.create_acc, model)
      .pipe(finalize(() => this.notifyUpdate('0'))); // Notify full refresh after create
  }

  blockUser(userId: string): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(`${API_ENDPOINTS.block_acc}${userId}`, {})
      .pipe(finalize(() => this.notifyDelete(userId))); // Treat block/unblock as delete for refresh
  }

  updateUser(model: UserUpdate): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(`${API_ENDPOINTS.update_acc}${model.id}`, model)
      .pipe(finalize(() => this.notifyUpdate(model.id))); // Notify update after success
  }

  getUserById(id: string): Observable<UserInfo> {
    return this.http.get<UserInfo>(`${API_ENDPOINTS.all_accs}${id}`);
  }

  private fetchUsers(): void {
    this.loadingSubject.next(true);
    this.http
      .get<UserInfo[]>(API_ENDPOINTS.all_accs)
      .pipe(finalize(() => this.loadingSubject.next(false)))
      .subscribe({
        next: (data) => {
          this.usersSubject.next(data);
        },
        error: (err) => {
          console.error('Failed to fetch users:', err);
          this.usersSubject.next(null);
          if (err.status === 403 || err.status === 401) {
            this.sharedService.redirectUser();
          }
        },
      });
  }

  get hasUsersData(): boolean {
    return this.usersSubject.getValue() != null;
  }

  getuserById(id: string): UserInfo | undefined {
    return this.usersSubject.getValue()?.find((user) => user.id === id);
  }

  triggerFetchUsers() {
    this.fetchUsers();
  }

  notifyUpdate(userId: string) {
    this.updatedSource.next(userId);
  }

  notifyDelete(userId: string) {
    this.deletedSource.next(userId);
  }
}
