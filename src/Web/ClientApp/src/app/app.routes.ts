import { Routes } from '@angular/router';

/**
 * Main Application Routes
 *
 * Feature modules will be lazy-loaded using the new standalone component routing.
 *
 * Example lazy-loaded routes:
 * {
 *   path: 'auth',
 *   loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
 * },
 * {
 *   path: 'dashboard',
 *   loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
 *   canActivate: [authGuard]
 * }
 */
export const routes: Routes = [
  {
    path: '',
    redirectTo: '/auth/login',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  // Protected routes will go here with authGuard
  // {
  //   path: 'dashboard',
  //   loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
  //   canActivate: [authGuard]
  // }
  {
    path: '**',
    redirectTo: '/auth/login'
  }
];
