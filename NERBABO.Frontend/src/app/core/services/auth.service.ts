import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { jwtDecode } from 'jwt-decode';

import { type User } from '../models/user';
import { type Login } from '../models/login';
import { type JwtPayload } from '../models/jwtPayload';
import { UserRole } from '../models/userRole';
import { OkResponse } from '../models/okResponse';
import { API_ENDPOINTS } from '../objects/apiEndpoints';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private userSubject = new BehaviorSubject<User | null>(null);
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage(): void {
    // try to load user from the storage
    try {
      const userData = localStorage.getItem(`${environment.userKey}`);
      if (userData) {
        // parse and store the value as a observable
        const user = JSON.parse(userData);
        this.userSubject.next(user);
      } else {
        // Ensure null is emitted if no user
        this.userSubject.next(null);
      }
    } catch (error) {
      console.error('Error loading user from storage:', error);
      this.logout();
    }
  }

  logout(): void {
    // TODO: Invalidate jwt
    this.removeUser();
  }

  login(model: Login): Observable<void> {
    return this.http.post<User>(API_ENDPOINTS.login, model).pipe(
      map((user: User) => {
        if (user) {
          this.setUser(user);
        }
      })
    );
  }

  get isAuthenticated(): boolean {
    return this.userSubject.getValue() !== null;
  }

  get bearerToken() {
    // gets the current user jwt
    return this.userSubject.getValue()?.jwt;
  }

  private setUser(user: User): void {
    // Sets a user to the local storage
    localStorage.setItem(environment.userKey, JSON.stringify(user));
    this.userSubject.next(user);
  }

  private removeUser(): void {
    // Removes user key from the local storage
    localStorage.removeItem(environment.userKey);
    this.userSubject.next(null);
  }

  get getUserClaims(): any {
    const token = this.userSubject.getValue()?.jwt; // or sessionStorage or from cookies
    if (!token) return null;

    try {
      const decoded = jwtDecode<JwtPayload>(token);
      return decoded;
    } catch (err) {
      console.error('Invalid token', err);
      return null;
    }
  }

  get isUserAdmin(): boolean {
    const claims = this.getUserClaims;
    return Array.isArray(claims?.role) && claims.role.includes('Admin');
  }

  assignRole(model: UserRole): Observable<OkResponse> {
    return this.http.post<OkResponse>(
      `${environment.appUrl}/api/atuh/set-role/`,
      model
    );
  }
}
