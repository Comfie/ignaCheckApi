import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

/**
 * Loading Interceptor
 * Shows/hides loading indicator for HTTP requests
 */
@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
  constructor(private loadingService: LoadingService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Show loading indicator
    this.loadingService.show();

    return next.handle(request).pipe(
      finalize(() => {
        // Hide loading indicator when request completes
        this.loadingService.hide();
      })
    );
  }
}
