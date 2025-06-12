import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { Observable, of, map, catchError, tap } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { SharedService } from '../../core/services/shared.service';
import { User } from '../../core/models/user';

/**
 * Auth guard for checking if a user is authenticated
 * @returns Observable<boolean | UrlTree>
 */
export const authGuard: CanActivateFn = (
  route,
  state
): Observable<boolean | UrlTree> => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const sharedService = inject(SharedService);

  // Debug the user observable
  console.log('Auth Guard Running');

  return authService.user$.pipe(
    tap((user) => console.log('Auth Guard User:', user)),
    map((user: User | null) => {
      if (user) {
        console.log('User authenticated, allowing access');
        return true;
      }

      console.log('User not authenticated, redirecting to', 'login');
      sharedService.showError('Area Restrita.');

      const url = state.url;
      return router.createUrlTree(['login'], {
        queryParams: { returnUrl: url },
      });
    }),
    catchError((error) => {
      console.error('Error in authGuard:', error);
      return of(router.createUrlTree(['login']));
    })
  );
};
