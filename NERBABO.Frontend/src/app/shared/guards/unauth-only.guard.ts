import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { User } from '../../core/models/user';

export const unauthOnlyGuard: CanActivateFn = (
  route,
  state
): Observable<boolean | UrlTree> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.user$.pipe(
    tap((user) => console.log('Guard User:', user)),
    map((user: User | null) => {
      if (!user) {
        return true;
      }
      return router.createUrlTree(['dashboard']);
    }),
    catchError((error) => {
      console.error('Error in Guard:', error);

      return of(true);
    })
  );
};
