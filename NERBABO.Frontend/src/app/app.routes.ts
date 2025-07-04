import { Routes } from '@angular/router';
import { authGuard } from './shared/guards/auth.guard';
import { unauthOnlyGuard } from './shared/guards/unauth-only.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(
        (m) => m.LoginComponent
      ),
    canActivate: [unauthOnlyGuard],
  },
  // {
  //   path: 'register',
  //   loadComponent: () =>
  //     import('./features/user/register/register.component').then(
  //       (m) => m.RegisterComponent
  //     ),
  //   canActivate: [authGuard],
  // },
  {
    path: 'frames',
    loadComponent: () =>
      import('./features/frames/index-frames/index-frames.component').then(
        (m) => m.IndexFramesComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'modules',
    loadComponent: () =>
      import('./features/modules/index-modules/index-modules.component').then(
        (m) => m.IndexModulesComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'modules/:id',
    loadComponent: () =>
      import('./features/modules/view-modules/view-modules.component').then(
        (m) => m.ViewModulesComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'logout',
    loadComponent: () =>
      import('./features/auth/logout/logout.component').then(
        (m) => m.LogoutComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(
        (m) => m.DashboardComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'config',
    loadComponent: () =>
      import('./features/global-config/global-config.component').then(
        (m) => m.GlobalConfigComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'accs',
    loadComponent: () =>
      import('./features/acc/index-acc/index-acc.component').then(
        (m) => m.IndexAccComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'companies',
    loadComponent: () =>
      import(
        './features/companies/index-companies/index-companies.component'
      ).then((m) => m.IndexCompaniesComponent),
    canActivate: [authGuard],
  },
  {
    path: 'companies/:id',
    loadComponent: () =>
      import(
        './features/companies/view-companies/view-companies.component'
      ).then((m) => m.ViewCompaniesComponent),
    canActivate: [authGuard],
  },
  {
    path: 'people',
    loadComponent: () =>
      import('./features/people/index-people/index-people.component').then(
        (m) => m.IndexPeopleComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'people/:id',
    loadComponent: () =>
      import('./features/people/view-people/view-people.component').then(
        (m) => m.ViewPeopleComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'not-found',
    loadComponent: () =>
      import('./shared/not-found/not-found.component').then(
        (m) => m.NotFoundComponent
      ),
  },
  {
    path: '**',
    redirectTo: 'not-found',
  },
];
