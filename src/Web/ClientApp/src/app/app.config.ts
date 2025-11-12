import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';

/**
 * Application Configuration
 *
 * This configuration provides all necessary providers for the standalone application:
 * - Router configuration with routes
 * - HTTP client with interceptors (JWT, Error handling, Loading)
 * - Browser animations
 * - Zone change detection optimization
 * - BASE_URL provider for API configuration
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        jwtInterceptor,
        errorInterceptor,
        loadingInterceptor
      ])
    ),
    provideAnimations(),
    {
      provide: 'BASE_URL',
      useFactory: () => document.getElementsByTagName('base')[0].href
    }
  ]
};


