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
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  // Lazy-loaded feature routes will go here
  // Features can be loaded as:
  // 1. Single component: loadComponent
  // 2. Route children: loadChildren
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];
