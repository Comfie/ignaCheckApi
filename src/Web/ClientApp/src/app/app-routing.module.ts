import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

/**
 * Main Application Routing
 *
 * Feature modules will be lazy-loaded.
 * Use Angular CLI to generate feature modules:
 *
 * ng generate module features/auth --routing
 * ng generate module features/dashboard --routing
 * ng generate module features/projects --routing
 *
 * Then add lazy routes below following the pattern in SETUP-GUIDE.md
 */
const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  // Lazy-loaded feature modules will go here
  // Example:
  // {
  //   path: 'auth',
  //   loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  // },
  // {
  //   path: 'dashboard',
  //   loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule),
  //   canActivate: [AuthGuard]
  // },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
