import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NotificationService } from '../services/notification.service';
import { TokenService } from '../services/token.service';

/**
 * Error Interceptor (Functional)
 * Handles HTTP errors globally
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const notificationService = inject(NotificationService);
  const tokenService = inject(TokenService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An error occurred';

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = error.error.message;
      } else {
        // Server-side error
        switch (error.status) {
          case 400:
            errorMessage = handleBadRequest(error);
            break;
          case 401:
            errorMessage = 'Unauthorized. Please login again.';
            tokenService.clearTokens();
            router.navigate(['/auth/login']);
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
      notificationService.error(errorMessage);

      // Log error to console
      console.error('HTTP Error:', error);

      return throwError(() => error);
    })
  );
};

function handleBadRequest(error: HttpErrorResponse): string {
  if (error.error?.errors) {
    // Validation errors from ASP.NET Core
    const validationErrors = Object.values(error.error.errors).flat() as string[];
    return validationErrors.join(', ');
  }
  return error.error?.title || error.error?.message || 'Invalid request';
}
