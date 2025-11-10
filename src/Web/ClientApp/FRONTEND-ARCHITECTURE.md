# Frontend Architecture Guide

**Angular Version:** 20.3.10 (Latest)
**Architecture:** Standalone Components (Module-Free)

## Overview

This Angular application uses the modern **standalone components** architecture introduced in Angular 14+ and recommended as the default since Angular 15. This approach eliminates NgModules in favor of self-contained, composable components.

### Key Benefits
- ✅ Less boilerplate code (no module files)
- ✅ Better tree-shaking and smaller bundle sizes
- ✅ Simpler mental model (components import what they need)
- ✅ Faster compilation
- ✅ Component-level lazy loading
- ✅ Future-proof (Angular's recommended approach)

## Project Structure

```
src/app/
├── core/                          # Core services, guards, and interceptors
│   ├── guards/
│   │   ├── auth.guard.ts         # Functional guard for authentication
│   │   └── role.guard.ts         # Functional guard for role-based access
│   ├── interceptors/
│   │   ├── jwt.interceptor.ts    # Functional interceptor for JWT tokens
│   │   ├── error.interceptor.ts  # Functional interceptor for error handling
│   │   └── loading.interceptor.ts # Functional interceptor for loading state
│   ├── services/
│   │   ├── auth.service.ts       # Authentication logic
│   │   ├── token.service.ts      # Token storage/retrieval
│   │   ├── loading.service.ts    # Loading state management
│   │   └── notification.service.ts # Toast notifications
│
├── shared/                        # Reusable components/pipes/directives
│   ├── components/
│   │   ├── loading-spinner/      # Loading indicator
│   │   ├── page-header/          # Page title component
│   │   ├── confirm-dialog/       # Confirmation modal
│   │   ├── data-table/           # Reusable table component
│   │   └── file-upload/          # File upload component
│   ├── directives/
│   │   └── has-role.directive.ts # *hasRole structural directive
│   ├── pipes/
│   │   ├── file-size.pipe.ts     # Format file sizes
│   │   └── time-ago.pipe.ts      # Relative time formatting
│   └── shared.module.ts          # Shared module exports
│
├── features/                      # Feature modules (lazy-loaded)
│   ├── auth/
│   │   ├── login/
│   │   ├── register/
│   │   ├── forgot-password/
│   │   ├── auth-routing.module.ts
│   │   └── auth.module.ts
│   ├── dashboard/
│   │   ├── dashboard.component.ts
│   │   ├── widgets/              # Dashboard widgets
│   │   ├── dashboard-routing.module.ts
│   │   └── dashboard.module.ts
│   ├── projects/
│   │   ├── project-list/
│   │   ├── project-detail/
│   │   ├── project-create/
│   │   ├── services/project.service.ts
│   │   ├── models/project.model.ts
│   │   ├── projects-routing.module.ts
│   │   └── projects.module.ts
│   ├── documents/
│   │   ├── document-list/
│   │   ├── document-upload/
│   │   ├── document-viewer/
│   │   ├── services/document.service.ts
│   │   ├── models/document.model.ts
│   │   ├── documents-routing.module.ts
│   │   └── documents.module.ts
│   ├── frameworks/
│   │   ├── framework-list/
│   │   ├── framework-detail/
│   │   ├── services/framework.service.ts
│   │   ├── models/framework.model.ts
│   │   ├── frameworks-routing.module.ts
│   │   └── frameworks.module.ts
│   ├── findings/
│   │   ├── finding-list/
│   │   ├── finding-detail/
│   │   ├── services/finding.service.ts
│   │   ├── models/finding.model.ts
│   │   ├── findings-routing.module.ts
│   │   └── findings.module.ts
│   ├── reports/
│   │   ├── compliance-dashboard/
│   │   ├── framework-report/
│   │   ├── executive-summary/
│   │   ├── services/report.service.ts
│   │   ├── models/report.model.ts
│   │   ├── reports-routing.module.ts
│   │   └── reports.module.ts
│   ├── settings/
│   │   ├── workspace-settings/
│   │   ├── user-management/
│   │   ├── notification-preferences/
│   │   ├── services/settings.service.ts
│   │   ├── settings-routing.module.ts
│   │   └── settings.module.ts
│   └── profile/
│       ├── profile.component.ts
│       ├── profile-routing.module.ts
│       └── profile.module.ts
│
├── layout/                        # Layout components
│   ├── header/
│   │   ├── header.component.ts
│   │   ├── header.component.html
│   │   └── header.component.scss
│   ├── sidebar/
│   │   ├── sidebar.component.ts
│   │   ├── sidebar.component.html
│   │   └── sidebar.component.scss
│   ├── footer/
│   │   ├── footer.component.ts
│   │   ├── footer.component.html
│   │   └── footer.component.scss
│   └── main-layout/
│       ├── main-layout.component.ts  # Container with header/sidebar/footer
│       ├── main-layout.component.html
│       └── main-layout.component.scss
│
├── models/                        # Shared models/interfaces
│   ├── api-response.model.ts
│   ├── pagination.model.ts
│   └── enums/
│       ├── project-status.enum.ts
│       ├── compliance-status.enum.ts
│       └── user-role.enum.ts
│
├── app.routes.ts                  # Route definitions (replaces app-routing.module.ts)
├── app.config.ts                  # Application configuration (replaces app.module.ts)
├── app.component.ts               # Root standalone component
├── app.component.html
├── app.component.scss
└── main.ts                        # Bootstrap with bootstrapApplication()
```

## Standalone Components Architecture

### No More NgModules! 🎉

This application uses **zero NgModules**. Instead:
- Components declare their dependencies directly via `imports: []`
- Services use `providedIn: 'root'` for singleton behavior
- Guards and interceptors are functional (not class-based)
- Routing uses standalone route configuration

### Application Bootstrap

**Old Way (NgModule):**
```typescript
platformBrowserDynamic().bootstrapModule(AppModule)
```

**New Way (Standalone):**
```typescript
// main.ts
bootstrapApplication(AppComponent, appConfig)
```

### Configuration (app.config.ts)

All application-level configuration is in `app.config.ts`:

```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([
      jwtInterceptor,
      errorInterceptor,
      loadingInterceptor
    ])),
    provideAnimations(),
    { provide: 'BASE_URL', useFactory: () => document.getElementsByTagName('base')[0].href }
  ]
};
```

## Component Loading Strategy

### Eager Loading
- AppComponent (root)
- Layout components (when implemented)
- Shared utilities (imported as needed)

### Lazy Loading
- **Component-level lazy loading** with `loadComponent`:
  ```typescript
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component')
      .then(m => m.DashboardComponent)
  }
  ```

- **Route-level lazy loading** with `loadChildren`:
  ```typescript
  {
    path: 'projects',
    loadChildren: () => import('./features/projects/project.routes')
      .then(m => m.PROJECT_ROUTES)
  }
  ```

## Routing Structure (app.routes.ts)

```typescript
export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: '',
    component: MainLayoutComponent, // Standalone component
    canActivate: [authGuard], // Functional guard
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component')
          .then(m => m.DashboardComponent),
        canActivate: [authGuard]
      },
      {
        path: 'projects',
        loadChildren: () => import('./features/projects/project.routes')
          .then(m => m.PROJECT_ROUTES)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component')
          .then(m => m.SettingsComponent),
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Owner', 'Admin'] }
      }
    ]
  },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];
```

## Functional Guards and Interceptors

### Functional Guards (Modern Approach)

**Old Way (Class-based):**
```typescript
@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(): boolean {
    if (this.auth.isAuthenticated()) return true;
    this.router.navigate(['/login']);
    return false;
  }
}
```

**New Way (Functional):**
```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) return true;
  router.navigate(['/login']);
  return false;
};
```

### Functional Interceptors

**Old Way (Class-based):**
```typescript
@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private tokenService: TokenService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    const token = this.tokenService.getToken();
    if (token) {
      req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    }
    return next.handle(req);
  }
}
```

**New Way (Functional):**
```typescript
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const token = tokenService.getToken();

  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }
  return next(req);
};
```

## State Management

### Option 1: Service-Based (Current)
- Simple services with RxJS BehaviorSubject
- Good for small to medium apps
- Less boilerplate

```typescript
@Injectable()
export class ProjectState {
  private projects$ = new BehaviorSubject<Project[]>([]);

  getProjects() {
    return this.projects$.asObservable();
  }

  setProjects(projects: Project[]) {
    this.projects$.next(projects);
  }
}
```

### Option 2: NgRx (For Scale)
- Redux pattern for Angular
- Better for large applications
- More boilerplate but better debugging

## HTTP Client Pattern

### Feature Services
Each feature module has its own service that wraps the auto-generated API client:

```typescript
@Injectable()
export class ProjectService {
  constructor(
    private projectsClient: ProjectsClient,
    private notificationService: NotificationService
  ) {}

  getProjects(): Observable<Project[]> {
    return this.projectsClient.getAll().pipe(
      tap(projects => console.log('Projects loaded:', projects.length)),
      catchError(error => {
        this.notificationService.error('Failed to load projects');
        return throwError(() => error);
      })
    );
  }

  createProject(command: CreateProjectCommand): Observable<string> {
    return this.projectsClient.create(command).pipe(
      tap(() => this.notificationService.success('Project created')),
      catchError(error => {
        this.notificationService.error('Failed to create project');
        return throwError(() => error);
      })
    );
  }
}
```

## Creating Standalone Components

### Component Structure

Every standalone component declares its own dependencies:

```typescript
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-example',
  standalone: true,  // ⭐ Key property
  imports: [
    CommonModule,    // For *ngIf, *ngFor, etc.
    RouterModule,    // For routerLink
    OtherComponent   // Import other standalone components
  ],
  templateUrl: './example.component.html',
  styleUrls: ['./example.component.scss']
})
export class ExampleComponent { }
```

### Generating Components

Use Angular CLI with `--standalone` flag (default in Angular 17+):

```bash
# Generate standalone component
ng generate component features/dashboard --standalone

# Generate standalone component with routing
ng generate component features/projects/project-list --standalone
```

## Component Patterns

### Smart vs Presentational Components

**Smart Components (Container)** - Located in feature folders
- Standalone components that import needed dependencies
- Handle business logic and state
- Connect to services
- Pass data to presentational components

```typescript
// features/projects/project-list/project-list.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { ProjectService } from '../services/project.service';
import { ProjectCardComponent } from '@shared/components/project-card';
import { LoadingService } from '@core/services/loading.service';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, ProjectCardComponent], // Import what you need
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.scss']
})
export class ProjectListComponent implements OnInit {
  projects$: Observable<Project[]>;
  loading$ = this.loadingService.loading$;

  constructor(
    private projectService: ProjectService,
    private loadingService: LoadingService
  ) {}

  ngOnInit() {
    this.projects$ = this.projectService.getProjects();
  }

  onCreateProject(command: CreateProjectCommand) {
    this.projectService.createProject(command).subscribe();
  }
}
```

**Presentational Components (Dumb)** - Located in shared folder
- Standalone components that are reusable
- Only display data
- Emit events via @Output
- Import minimal dependencies

```typescript
// shared/components/project-card/project-card.component.ts
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-project-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './project-card.component.html',
  styleUrls: ['./project-card.component.scss']
})
export class ProjectCardComponent {
  @Input() project!: Project;
  @Output() select = new EventEmitter<string>();

  onSelect() {
    this.select.emit(this.project.id);
  }
}
```

## Authentication Flow

```
1. User enters credentials
   ↓
2. AuthService.login(email, password)
   ↓
3. API returns JWT token
   ↓
4. TokenService.saveToken(token)
   ↓
5. AuthService updates currentUser$
   ↓
6. Router navigates to /dashboard
   ↓
7. JwtInterceptor adds token to all HTTP requests
   ↓
8. AuthGuard protects routes
```

## Error Handling

### Global Error Interceptor
- Catches all HTTP errors
- Shows user-friendly messages
- Handles 401 (redirect to login)
- Handles 403 (show access denied)
- Logs errors to console (production: send to monitoring)

### Component-Level Error Handling
```typescript
this.projectService.getProjects().subscribe({
  next: (projects) => this.projects = projects,
  error: (error) => {
    // Error already handled by interceptor
    // Optionally add component-specific handling
    this.hasError = true;
  }
});
```

## Loading States

### Global Loading
- Managed by LoadingInterceptor
- Shows spinner in header/overlay
- Automatic for all HTTP requests

### Component Loading
```typescript
export class ProjectListComponent {
  loading = false;

  loadProjects() {
    this.loading = true;
    this.projectService.getProjects().subscribe({
      next: (projects) => {
        this.projects = projects;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}
```

## Best Practices

### 1. One Component Per File
```
✅ project-list.component.ts
✅ project-list.component.html
✅ project-list.component.scss
✅ project-list.component.spec.ts
```

### 2. Use TypeScript Strict Mode
```json
// tsconfig.json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true
  }
}
```

### 3. Use OnPush Change Detection
```typescript
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectListComponent {
  // Component code
}
```

### 4. Unsubscribe from Observables
```typescript
export class ProjectListComponent implements OnDestroy {
  private destroy$ = new Subject<void>();

  ngOnInit() {
    this.projectService.getProjects()
      .pipe(takeUntil(this.destroy$))
      .subscribe();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

### 5. Use Async Pipe
```html
<!-- Automatically subscribes and unsubscribes -->
<div *ngFor="let project of projects$ | async">
  {{ project.name }}
</div>
```

## Styling Strategy

### Global Styles
- `src/styles.scss` - Global styles
- Bootstrap 5 for grid/utilities
- Bootstrap Icons for icons

### Component Styles
- Scoped to component
- Use SCSS for nesting and variables

### Theme Variables
```scss
// styles.scss
:root {
  --primary-color: #0066cc;
  --secondary-color: #6c757d;
  --success-color: #28a745;
  --danger-color: #dc3545;
  --warning-color: #ffc107;
}
```

## Testing Strategy

### Unit Tests
- Test services with TestBed
- Test components with ComponentFixture
- Mock dependencies

### Integration Tests
- Test feature modules
- Test routing
- Test HTTP calls (with HttpClientTestingModule)

### E2E Tests (Optional)
- Use Playwright or Cypress
- Test critical user flows

## Performance Optimization

### 1. Lazy Loading
- All feature modules lazy-loaded
- Reduces initial bundle size

### 2. OnPush Change Detection
- Reduces change detection cycles
- Better performance

### 3. TrackBy Functions
```html
<div *ngFor="let project of projects; trackBy: trackByProjectId">
</div>
```

```typescript
trackByProjectId(index: number, project: Project): string {
  return project.id;
}
```

### 4. Virtual Scrolling
```html
<cdk-virtual-scroll-viewport itemSize="50">
  <div *cdkVirtualFor="let item of items">
    {{ item.name }}
  </div>
</cdk-virtual-scroll-viewport>
```

## Deployment

### Build for Production
```bash
npm run build -- --configuration production
```

### Environment-Specific Builds
```bash
# Development
ng build

# Staging
ng build --configuration staging

# Production
ng build --configuration production
```

### Output
- `dist/` folder contains compiled files
- Deploy to Azure Static Web Apps, Vercel, or Netlify

## Next Steps

1. ✅ Implement authentication flow
2. ✅ Create dashboard with widgets
3. ✅ Build project management features
4. ✅ Implement document upload/viewing
5. ✅ Create compliance framework browsing
6. ✅ Build findings management
7. ✅ Implement reporting dashboards
8. ✅ Add real-time notifications
9. ✅ Implement search functionality
10. ✅ Add user management (admin)

## Resources

- [Angular Style Guide](https://angular.io/guide/styleguide)
- [RxJS Best Practices](https://rxjs.dev/guide/overview)
- [Angular Performance Guide](https://angular.io/guide/performance-best-practices)
- [Testing Guide](https://angular.io/guide/testing)
