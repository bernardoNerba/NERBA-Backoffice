import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';

import { type Register } from '../models/register';
import { type OkResponse } from '../models/okResponse';
import { type UserInfo } from '../models/userInfo';
import { SharedService } from './shared.service';
import { UserUpdate } from '../models/userUpdate';

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
    return this.http.post<OkResponse>(
      `${environment.appUrl}/api/account/register`,
      model
    );
  }

  blockUser(userId: string): Observable<OkResponse> {
    return this.http.put<OkResponse>(
      `${environment.appUrl}/api/account/block-user/${userId}`,
      {}
    );
  }

  private fetchUsers(): void {
    this.loadingSubject.next(true);

    this.http
      .get<Array<UserInfo>>(`${environment.appUrl}/api/user/all/`)
      .subscribe({
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
      `${environment.appUrl}/api/user/update/${model.id}`,
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
