import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

// Core Module - Import ONCE (provides singleton services, guards, interceptors)
import { CoreModule } from './core/core.module';

/**
 * Root Application Module
 *
 * This is the main entry point of the Angular application.
 * - CoreModule is imported once to provide singleton services
 * - Feature modules will be lazy-loaded via routing
 * - Layout components will be added when generated
 *
 * Next Steps:
 * 1. Generate feature modules: see SETUP-GUIDE.md
 * 2. Generate layout components: ng generate component layout/main-layout
 * 3. Add lazy routes to app-routing.module.ts
 */
@NgModule({
  declarations: [
    AppComponent
    // Layout components will be added here when generated
    // Example:
    // MainLayoutComponent,
    // HeaderComponent,
    // SidebarComponent,
    // FooterComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    CoreModule,  // Imported ONCE - provides auth, interceptors, guards
    AppRoutingModule  // Must be last
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
