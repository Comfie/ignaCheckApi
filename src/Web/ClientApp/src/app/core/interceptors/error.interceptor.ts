import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { NotificationService } from '../services/notification.service';
import { TokenService } from '../services/token.service';

/**
 * Error Interceptor
 * Handles HTTP errors globally
 */
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    private notificationService: NotificationService,
    private tokenService: TokenService
  ) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'An error occurred';

        if (error.error instanceof ErrorEvent) {
          // Client-side error
          errorMessage = error.error.message;
        } else {
          // Server-side error
          switch (error.status) {
            case 400:
              errorMessage = this.handleBadRequest(error);
              break;
            case 401:
              errorMessage = 'Unauthorized. Please login again.';
              this.tokenService.clearTokens();
              this.router.navigate(['/auth/login']);
              break;
            case 403:
              errorMessage = 'Access denied. You don\'t have permission to access this resource.';
              break;
            case 404:
              errorMessage = 'Resource not found.';
              break;
            case 500:
              errorMessage = 'Internal server error. Please try again later.';
              break;
            default:
              errorMessage = error.message || 'Something went wrong';
          }
        }

        // Show error notification
        this.notificationService.error(errorMessage);

        // Log error to console
        console.error('HTTP Error:', error);

        return throwError(() => error);
      })
    );
  }

  private handleBadRequest(error: HttpErrorResponse): string {
    if (error.error?.errors) {
      // Validation errors from ASP.NET Core
      const validationErrors = Object.values(error.error.errors).flat() as string[];
      return validationErrors.join(', ');
    }
    return error.error?.title || error.error?.message || 'Invalid request';
  }
}
