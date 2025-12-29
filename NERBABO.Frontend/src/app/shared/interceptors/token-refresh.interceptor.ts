import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { API_ENDPOINTS } from '../../core/objects/apiEndpoints';

/**
 * HTTP Interceptor that handles token refresh on 401 errors
 *
 * This interceptor catches 401 Unauthorized responses and attempts to
 * refresh the JWT token. If refresh succeeds, it retries the original
 * request. If refresh fails, it logs out the user and redirects to login.
 */
export const tokenRefreshInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only handle 401 errors
      if (error.status !== 401) {
        return throwError(() => error);
      }

      // Don't try to refresh on login or refresh endpoints to avoid infinite loops
      const isAuthEndpoint =
        req.url.includes('/api/auth/login') ||
        req.url.includes('/api/auth/refresh-user-token');

      if (isAuthEndpoint) {
        return throwError(() => error);
      }

      // Check if user is authenticated
      if (!authService.isAuthenticated) {
        // Not authenticated, redirect to login
        router.navigate(['/login']);
        return throwError(() => error);
      }

      console.log('401 error detected, attempting token refresh...');

      // Attempt to refresh the token
      return authService.refreshToken().pipe(
        switchMap(() => {
          console.log('Token refreshed, retrying original request');

          // Clone the original request with the new token
          const newToken = authService.bearerToken;
          if (!newToken) {
            throw new Error('No token available after refresh');
          }

          const clonedReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${newToken}`,
            },
          });

          // Retry the original request with the new token
          return next(clonedReq);
        }),
        catchError((refreshError) => {
          console.error('Token refresh failed, redirecting to login');

          // Refresh failed, logout and redirect
          authService.logout();
          router.navigate(['/login'], {
            queryParams: { returnUrl: router.url },
          });

          return throwError(() => refreshError);
        })
      );
    })
  );
};
