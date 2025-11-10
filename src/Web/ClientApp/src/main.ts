import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';

/**
 * Bootstrap the standalone Angular application
 *
 * This replaces the traditional platformBrowserDynamic().bootstrapModule()
 * approach with the modern bootstrapApplication() for standalone components.
 *
 * All configuration (routing, HTTP, animations, etc.) is provided in app.config.ts
 */
bootstrapApplication(AppComponent, appConfig)
  .catch(err => console.error(err));
