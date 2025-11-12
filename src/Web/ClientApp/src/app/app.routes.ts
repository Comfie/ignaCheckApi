import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

/**
 * Main Application Routes
 *
 * - Public routes (auth) are accessible without authentication
 * - Protected routes require authentication (authGuard)
 * - Role-specific routes use roleGuard with data: { roles: [...] }
 */
export const routes: Routes = [
  // Default route redirects to dashboard if authenticated
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },

  // Public authentication routes
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },

  // Protected routes with main layout
  {
    path: '',
    loadComponent: () => import('./features/layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      // Dashboard - Default redirect based on role
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Role-Specific Dashboards
      {
        path: 'dashboard/super-admin',
        canActivate: [roleGuard],
        data: { roles: ['SuperAdmin'] },
        loadComponent: () => import('./features/dashboards/super-admin/super-admin-dashboard.component').then(m => m.SuperAdminDashboardComponent)
      },
      {
        path: 'dashboard/workspace-owner',
        canActivate: [roleGuard],
        data: { roles: ['Owner', 'Admin'] },
        loadComponent: () => import('./features/dashboards/workspace-owner/workspace-owner-dashboard.component').then(m => m.WorkspaceOwnerDashboardComponent)
      },
      {
        path: 'dashboard/contributor',
        canActivate: [roleGuard],
        data: { roles: ['Contributor'] },
        loadComponent: () => import('./features/dashboards/contributor/contributor-dashboard.component').then(m => m.ContributorDashboardComponent)
      },
      {
        path: 'dashboard/viewer',
        canActivate: [roleGuard],
        data: { roles: ['Viewer'] },
        loadComponent: () => import('./features/dashboards/viewer/viewer-dashboard.component').then(m => m.ViewerDashboardComponent)
      },

      // Workspaces - Owner only (placeholder for now)
      {
        path: 'workspaces',
        canActivate: [roleGuard],
        data: { roles: ['Owner'] },
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Projects - All authenticated users (placeholder for now)
      {
        path: 'projects',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Documents - All authenticated users (placeholder for now)
      {
        path: 'documents',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Findings - All authenticated users (placeholder for now)
      {
        path: 'findings',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Frameworks - All authenticated users (placeholder for now)
      {
        path: 'frameworks',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Users - Owner & Admin only (placeholder for now)
      {
        path: 'users',
        canActivate: [roleGuard],
        data: { roles: ['Owner', 'Admin'] },
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Reports - All authenticated users (placeholder for now)
      {
        path: 'reports',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Settings - All authenticated users (placeholder for now)
      {
        path: 'settings',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      }
    ]
  },

  // Fallback route
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];
