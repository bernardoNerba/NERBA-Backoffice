import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, map, Observable, of, tap } from 'rxjs';
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
  private isRefreshing = false;

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

        // Check if token is expired
        if (this.isTokenExpired(user.jwt)) {
          console.warn('Token expired on load, clearing user data');
          this.removeUser();
          return;
        }

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
    // Call backend to invalidate token
    const token = this.bearerToken;

    if (token) {
      // Make server call to invalidate token (fire and forget)
      this.http.post(API_ENDPOINTS.logout, {}).pipe(
        catchError((error) => {
          console.error('Logout server call failed:', error);
          // Continue with client-side logout even if server call fails
          return of(null);
        })
      ).subscribe(() => {
        console.log('Token invalidated on server');
      });
    }

    // Clear user data and reset refresh state immediately
    this.isRefreshing = false;
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

  get bearerToken(): string | undefined {
    // gets the current user jwt
    const token = this.userSubject.getValue()?.jwt;

    // Don't return expired tokens
    if (token && this.isTokenExpired(token)) {
      console.warn('Token expired, returning undefined');
      return undefined;
    }

    return token;
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

  /**
   * Checks if a JWT token is expired
   * @param token - The JWT token to check
   * @returns true if token is expired or invalid, false otherwise
   */
  isTokenExpired(token: string): boolean {
    if (!token) return true;

    try {
      const decoded = jwtDecode<JwtPayload & { exp: number }>(token);
      const currentTime = Math.floor(Date.now() / 1000);

      // Add 5 second buffer to account for clock skew
      return decoded.exp <= currentTime + 5;
    } catch (error) {
      console.error('Error decoding token:', error);
      return true;
    }
  }

  /**
   * Gets the token expiration time in milliseconds
   * @param token - The JWT token
   * @returns Expiration time in milliseconds, or null if invalid
   */
  getTokenExpirationTime(token: string): number | null {
    try {
      const decoded = jwtDecode<JwtPayload & { exp: number }>(token);
      return decoded.exp * 1000; // Convert to milliseconds
    } catch {
      return null;
    }
  }

  /**
   * Refreshes the current user's JWT token
   * @returns Observable of void on success, throws error on failure
   */
  refreshToken(): Observable<void> {
    // Prevent multiple simultaneous refresh attempts
    if (this.isRefreshing) {
      return of(void 0);
    }

    this.isRefreshing = true;

    return this.http.get<User>(API_ENDPOINTS.refresh_token).pipe(
      tap((user: User) => {
        if (user && user.jwt) {
          console.log('Token refreshed successfully');
          this.setUser(user);
          this.isRefreshing = false;
        }
      }),
      map(() => void 0),
      catchError((error) => {
        console.error('Token refresh failed:', error);
        this.isRefreshing = false;
        // Clear user data and logout on refresh failure
        this.logout();
        throw error;
      })
    );
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

  get userRoles(): string[] {
    const claims = this.getUserClaims;
    let roles = [];
    if (!Array.isArray(claims?.role)) {
      roles.push(claims?.role);
    } else {
      roles = claims?.role;
    }
    return roles;
  }

  get isUserAdmin(): boolean {
    const claims = this.getUserClaims;
    return Array.isArray(claims?.role) && claims.role.includes('Admin');
  }

  assignRole(model: UserRole): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.set_role, model);
  }
}
