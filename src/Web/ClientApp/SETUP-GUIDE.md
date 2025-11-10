# Frontend Setup Guide

## Current Status

✅ **Core Infrastructure Complete**
- Core module with services, guards, and interceptors
- Authentication service with JWT support
- HTTP interceptors for auth, errors, and loading
- Auth and Role guards
- Token service for secure storage
- Loading and notification services
- TypeScript models and enums
- Shared module structure

## Next Steps: Generate Feature Modules

Use the Angular CLI to generate the remaining feature modules and components. Run these commands from `/home/user/ignaCheckApi/src/Web/ClientApp`:

### 1. Authentication Feature

```bash
# Generate auth module with routing
ng generate module features/auth --routing

# Generate login component
ng generate component features/auth/login --export

# Generate register component
ng generate component features/auth/register --export

# Generate forgot-password component
ng generate component features/auth/forgot-password --export

# Generate verify-email component
ng generate component features/auth/verify-email --export
```

### 2. Dashboard Feature

```bash
# Generate dashboard module with routing
ng generate module features/dashboard --routing

# Generate dashboard component
ng generate component features/dashboard --export

# Generate dashboard widgets
ng generate component features/dashboard/widgets/compliance-overview
ng generate component features/dashboard/widgets/recent-projects
ng generate component features/dashboard/widgets/findings-summary
ng generate component features/dashboard/widgets/recent-activity
```

### 3. Projects Feature

```bash
# Generate projects module with routing
ng generate module features/projects --routing

# Generate project service
ng generate service features/projects/services/project

# Generate project components
ng generate component features/projects/project-list --export
ng generate component features/projects/project-detail --export
ng generate component features/projects/project-create --export
ng generate component features/projects/project-card

# Generate project models
# (Create manually in features/projects/models/project.model.ts)
```

### 4. Documents Feature

```bash
# Generate documents module with routing
ng generate module features/documents --routing

# Generate document service
ng generate service features/documents/services/document

# Generate document components
ng generate component features/documents/document-list --export
ng generate component features/documents/document-upload --export
ng generate component features/documents/document-viewer --export
ng generate component features/documents/document-card
```

### 5. Frameworks Feature

```bash
# Generate frameworks module with routing
ng generate module features/frameworks --routing

# Generate framework service
ng generate service features/frameworks/services/framework

# Generate framework components
ng generate component features/frameworks/framework-list --export
ng generate component features/frameworks/framework-detail --export
ng generate component features/frameworks/control-list
```

### 6. Findings Feature

```bash
# Generate findings module with routing
ng generate module features/findings --routing

# Generate finding service
ng generate service features/findings/services/finding

# Generate finding components
ng generate component features/findings/finding-list --export
ng generate component features/findings/finding-detail --export
ng generate component features/findings/finding-card
ng generate component features/findings/finding-comments
```

### 7. Reports Feature

```bash
# Generate reports module with routing
ng generate module features/reports --routing

# Generate report service
ng generate service features/reports/services/report

# Generate report components
ng generate component features/reports/compliance-dashboard --export
ng generate component features/reports/framework-report --export
ng generate component features/reports/executive-summary --export
ng generate component features/reports/audit-trail --export
```

### 8. Settings Feature

```bash
# Generate settings module with routing
ng generate module features/settings --routing

# Generate settings service
ng generate service features/settings/services/settings

# Generate settings components
ng generate component features/settings/workspace-settings --export
ng generate component features/settings/user-management --export
ng generate component features/settings/notification-preferences --export
```

### 9. Profile Feature

```bash
# Generate profile module with routing
ng generate module features/profile --routing

# Generate profile component
ng generate component features/profile --export
```

### 10. Layout Components

```bash
# Generate main layout
ng generate component layout/main-layout

# Generate header
ng generate component layout/header

# Generate sidebar
ng generate component layout/sidebar

# Generate footer
ng generate component layout/footer
```

### 11. Shared Components

```bash
# Generate shared components
ng generate component shared/components/loading-spinner --export
ng generate component shared/components/page-header --export
ng generate component shared/components/confirm-dialog --export
ng generate component shared/components/data-table --export
ng generate component shared/components/file-upload --export
ng generate component shared/components/breadcrumb --export

# Generate shared directives
ng generate directive shared/directives/has-role --export

# Generate shared pipes
ng generate pipe shared/pipes/file-size --export
ng generate pipe shared/pipes/time-ago --export
```

## Manual Implementation Tasks

### 1. Update app.module.ts

```typescript
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

// Core Module (import once)
import { CoreModule } from './core/core.module';

// Layout Components
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { HeaderComponent } from './layout/header/header.component';
import { SidebarComponent } from './layout/sidebar/sidebar.component';
import { FooterComponent } from './layout/footer/footer.component';

@NgModule({
  declarations: [
    AppComponent,
    MainLayoutComponent,
    HeaderComponent,
    SidebarComponent,
    FooterComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    CoreModule,  // Import once
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

### 2. Update app-routing.module.ts

```typescript
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';

const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule)
      },
      {
        path: 'projects',
        loadChildren: () => import('./features/projects/projects.module').then(m => m.ProjectsModule)
      },
      {
        path: 'documents',
        loadChildren: () => import('./features/documents/documents.module').then(m => m.DocumentsModule)
      },
      {
        path: 'frameworks',
        loadChildren: () => import('./features/frameworks/frameworks.module').then(m => m.FrameworksModule)
      },
      {
        path: 'findings',
        loadChildren: () => import('./features/findings/findings.module').then(m => m.FindingsModule)
      },
      {
        path: 'reports',
        loadChildren: () => import('./features/reports/reports.module').then(m => m.ReportsModule)
      },
      {
        path: 'settings',
        loadChildren: () => import('./features/settings/settings.module').then(m => m.SettingsModule),
        canActivate: [RoleGuard],
        data: { roles: ['Owner', 'Admin'] }
      },
      {
        path: 'profile',
        loadChildren: () => import('./features/profile/profile.module').then(m => m.ProfileModule)
      }
    ]
  },
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
```

### 3. Feature Module Routing Example

**features/projects/projects-routing.module.ts**:
```typescript
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProjectListComponent } from './project-list/project-list.component';
import { ProjectDetailComponent } from './project-detail/project-detail.component';
import { ProjectCreateComponent } from './project-create/project-create.component';

const routes: Routes = [
  {
    path: '',
    component: ProjectListComponent
  },
  {
    path: 'new',
    component: ProjectCreateComponent
  },
  {
    path: ':id',
    component: ProjectDetailComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProjectsRoutingModule { }
```

### 4. Feature Module Example

**features/projects/projects.module.ts**:
```typescript
import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { ProjectsRoutingModule } from './projects-routing.module';

// Components
import { ProjectListComponent } from './project-list/project-list.component';
import { ProjectDetailComponent } from './project-detail/project-detail.component';
import { ProjectCreateComponent } from './project-create/project-create.component';
import { ProjectCardComponent } from './project-card/project-card.component';

// Services
import { ProjectService } from './services/project.service';

@NgModule({
  declarations: [
    ProjectListComponent,
    ProjectDetailComponent,
    ProjectCreateComponent,
    ProjectCardComponent
  ],
  imports: [
    SharedModule,
    ProjectsRoutingModule
  ],
  providers: [
    ProjectService
  ]
})
export class ProjectsModule { }
```

### 5. Service Implementation Example

**features/projects/services/project.service.ts**:
```typescript
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { ProjectsClient, CreateProjectCommand, ProjectDto } from '../../../web-api-client';
import { NotificationService } from '../../../core/services/notification.service';

@Injectable()
export class ProjectService {
  constructor(
    private projectsClient: ProjectsClient,
    private notificationService: NotificationService
  ) {}

  getProjects(): Observable<ProjectDto[]> {
    return this.projectsClient.get().pipe(
      tap(() => console.log('Projects loaded')),
      catchError(error => {
        this.notificationService.error('Failed to load projects');
        throw error;
      })
    );
  }

  getProject(id: string): Observable<ProjectDto> {
    return this.projectsClient.get2(id).pipe(
      catchError(error => {
        this.notificationService.error('Failed to load project');
        throw error;
      })
    );
  }

  createProject(command: CreateProjectCommand): Observable<string> {
    return this.projectsClient.create(command).pipe(
      tap(() => this.notificationService.success('Project created successfully')),
      catchError(error => {
        this.notificationService.error('Failed to create project');
        throw error;
      })
    );
  }

  deleteProject(id: string): Observable<void> {
    return this.projectsClient.delete(id).pipe(
      tap(() => this.notificationService.success('Project deleted successfully')),
      catchError(error => {
        this.notificationService.error('Failed to delete project');
        throw error;
      })
    );
  }
}
```

### 6. Component Implementation Example

**features/projects/project-list/project-list.component.ts**:
```typescript
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { ProjectDto } from '../../../web-api-client';
import { ProjectService } from '../services/project.service';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-project-list',
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.scss']
})
export class ProjectListComponent implements OnInit {
  projects$: Observable<ProjectDto[]>;
  loading$ = this.loadingService.loading$;

  constructor(
    private projectService: ProjectService,
    private loadingService: LoadingService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.projects$ = this.projectService.getProjects();
  }

  onCreateProject(): void {
    this.router.navigate(['/projects/new']);
  }

  onViewProject(id: string): void {
    this.router.navigate(['/projects', id]);
  }
}
```

**features/projects/project-list/project-list.component.html**:
```html
<div class="container-fluid">
  <app-page-header
    title="Projects"
    [actions]="[
      { label: 'New Project', icon: 'plus', click: onCreateProject.bind(this) }
    ]">
  </app-page-header>

  <div class="row" *ngIf="projects$ | async as projects; else loading">
    <div class="col-md-4" *ngFor="let project of projects">
      <app-project-card
        [project]="project"
        (click)="onViewProject(project.id)">
      </app-project-card>
    </div>

    <div *ngIf="projects.length === 0" class="col-12">
      <div class="alert alert-info">
        No projects found. Create your first project to get started!
      </div>
    </div>
  </div>

  <ng-template #loading>
    <app-loading-spinner></app-loading-spinner>
  </ng-template>
</div>
```

## Quick Start Script

Create a bash script to generate all modules at once:

```bash
#!/bin/bash
# generate-features.sh

echo "Generating Authentication feature..."
ng generate module features/auth --routing
ng generate component features/auth/login --export
ng generate component features/auth/register --export
ng generate component features/auth/forgot-password --export

echo "Generating Dashboard feature..."
ng generate module features/dashboard --routing
ng generate component features/dashboard --export

echo "Generating Projects feature..."
ng generate module features/projects --routing
ng generate service features/projects/services/project
ng generate component features/projects/project-list --export
ng generate component features/projects/project-detail --export
ng generate component features/projects/project-create --export

echo "Generating Documents feature..."
ng generate module features/documents --routing
ng generate service features/documents/services/document
ng generate component features/documents/document-list --export

echo "Generating Frameworks feature..."
ng generate module features/frameworks --routing
ng generate service features/frameworks/services/framework
ng generate component features/frameworks/framework-list --export

echo "Generating Findings feature..."
ng generate module features/findings --routing
ng generate service features/findings/services/finding
ng generate component features/findings/finding-list --export

echo "Generating Reports feature..."
ng generate module features/reports --routing
ng generate service features/reports/services/report
ng generate component features/reports/compliance-dashboard --export

echo "Generating Settings feature..."
ng generate module features/settings --routing
ng generate component features/settings/workspace-settings --export

echo "Generating Profile feature..."
ng generate module features/profile --routing
ng generate component features/profile --export

echo "Generating Layout components..."
ng generate component layout/main-layout
ng generate component layout/header
ng generate component layout/sidebar
ng generate component layout/footer

echo "Generating Shared components..."
ng generate component shared/components/loading-spinner --export
ng generate component shared/components/page-header --export

echo "Done! Now manually update app.module.ts and app-routing.module.ts"
```

Make it executable and run:
```bash
chmod +x generate-features.sh
./generate-features.sh
```

## Styling Setup

### Install Bootstrap and Icons

```bash
npm install bootstrap bootstrap-icons
```

### Update angular.json

Add to `styles` array:
```json
"styles": [
  "node_modules/bootstrap/dist/css/bootstrap.min.css",
  "node_modules/bootstrap-icons/font/bootstrap-icons.css",
  "src/styles.scss"
],
"scripts": [
  "node_modules/bootstrap/dist/js/bootstrap.bundle.min.js"
]
```

### Global Styles (src/styles.scss)

```scss
// Import Bootstrap
@import '~bootstrap/scss/bootstrap';
@import '~bootstrap-icons/font/bootstrap-icons.css';

// Custom theme variables
:root {
  --primary-color: #0066cc;
  --secondary-color: #6c757d;
  --success-color: #28a745;
  --danger-color: #dc3545;
  --warning-color: #ffc107;
  --info-color: #17a2b8;
}

// Global styles
body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
}

.loading-spinner {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 200px;
}
```

## Development Tips

1. **Use Angular CLI** for generating components, services, modules
2. **Follow naming conventions**: kebab-case for files, PascalCase for classes
3. **Use TypeScript strict mode** for better type safety
4. **Implement OnPush change detection** for better performance
5. **Use async pipe** to avoid manual subscriptions
6. **Create feature-specific models** instead of using auto-generated DTOs directly
7. **Write unit tests** as you develop
8. **Use shared components** for reusability
9. **Keep components small** and focused on single responsibility

## Testing

```bash
# Run unit tests
npm test

# Run with coverage
npm test -- --code-coverage

# Run E2E tests
npm run e2e
```

## Build

```bash
# Development build
npm run build

# Production build
npm run build -- --configuration production

# Analyze bundle size
npm run build -- --stats-json
npx webpack-bundle-analyzer dist/stats.json
```

## Next Steps

1. Run the generation scripts above
2. Implement authentication flow
3. Create dashboard widgets
4. Build project management features
5. Implement document upload/viewing
6. Create findings management
7. Build reporting dashboards
8. Add user management (admin)
9. Implement real-time notifications
10. Add search functionality

## Need Help?

- [Angular Documentation](https://angular.io/docs)
- [Angular CLI](https://angular.io/cli)
- [RxJS Documentation](https://rxjs.dev/)
- [Bootstrap 5](https://getbootstrap.com/docs/5.3/)

Your frontend is now ready for rapid feature development! 🚀
