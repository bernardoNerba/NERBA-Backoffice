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
    path: 'actions',
    loadComponent: () =>
      import('./features/actions/index-actions/index-actions.component').then(
        (m) => m.IndexActionsComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'actions/:id',
    loadComponent: () =>
      import('./features/actions/view-actions/view-actions.component').then(
        (m) => m.ViewActionsComponent
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
    path: 'courses',
    loadComponent: () =>
      import('./features/courses/index-courses/index-courses.component').then(
        (m) => m.IndexCoursesComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'courses/:id',
    loadComponent: () =>
      import('./features/courses/view-courses/view-courses.component').then(
        (m) => m.ViewCoursesComponent
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
    path: 'people/:id/teacher',
    loadComponent: () =>
      import('./features/teachers/view-teachers/view-teachers.component').then(
        (m) => m.ViewTeachersComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'people/:id/student',
    loadComponent: () =>
      import('./features/students/view-students/view-students.component').then(
        (m) => m.ViewStudentsComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'people/:id/acc',
    loadComponent: () =>
      import('./features/acc/view-acc/view-acc.component').then(
        (m) => m.ViewAccComponent
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
