import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { type Register } from '../models/register';
import { type OkResponse } from '../models/okResponse';
import { type UserInfo } from '../models/userInfo';
import { SharedService } from './shared.service';
import { UserUpdate } from '../models/userUpdate';
import { API_ENDPOINTS } from '../objects/apiEndpoints';

@Injectable({
  providedIn: 'root',
})
export class AccService {
  private usersSubject = new BehaviorSubject<Array<UserInfo> | null>(null);
  private loadingSubject = new BehaviorSubject<boolean>(true);

  users$ = this.usersSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.fetchUsers();
  }

  register(model: Register): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.create_acc, model);
  }

  blockUser(userId: string): Observable<OkResponse> {
    return this.http.put<OkResponse>(`${API_ENDPOINTS.block_acc}${userId}`, {});
  }

  private fetchUsers(): void {
    this.loadingSubject.next(true);

    this.http.get<Array<UserInfo>>(API_ENDPOINTS.all_accs).subscribe({
      next: (data) => {
        this.usersSubject.next(data);
      },
      error: (err) => {
        this.usersSubject.next(null);
        if (err.status === 403 || err.status === 401) {
          this.sharedService.redirectUser();
        }
      },
    });
    this.loadingSubject.next(false);
  }

  updateUser(model: UserUpdate): Observable<OkResponse> {
    return this.http.put<OkResponse>(
      `${API_ENDPOINTS.update_acc}${model.id}`,
      model
    );
  }

  get hasUsersData() {
    return this.usersSubject.getValue() != null;
  }

  getuserById(id: string): UserInfo | undefined {
    return this.usersSubject.getValue()?.find((user) => user.id === id);
  }

  triggerFetchUsers() {
    this.fetchUsers();
  }
}
