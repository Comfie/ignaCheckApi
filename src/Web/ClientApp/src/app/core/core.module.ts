import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';

// Interceptors
import { JwtInterceptor } from './interceptors/jwt.interceptor';
import { ErrorInterceptor } from './interceptors/error.interceptor';
import { LoadingInterceptor } from './interceptors/loading.interceptor';

// Services
import { AuthService } from './services/auth.service';
import { TokenService } from './services/token.service';
import { LoadingService } from './services/loading.service';
import { NotificationService } from './services/notification.service';

/**
 * Core Module
 *
 * Contains singleton services, guards, and interceptors.
 * Should only be imported once in AppModule.
 */
@NgModule({
  imports: [
    CommonModule,
    HttpClientModule
  ],
  providers: [
    // Services
    AuthService,
    TokenService,
    LoadingService,
    NotificationService,

    // HTTP Interceptors
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: LoadingInterceptor,
      multi: true
    }
  ]
})
export class CoreModule {
  /**
   * Prevent reimport of the CoreModule
   */
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error('CoreModule is already loaded. Import it only in AppModule.');
    }
  }
}
