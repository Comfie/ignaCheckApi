import { Injectable } from '@angular/core';

/**
 * Notification Service
 * Handles toast notifications
 * TODO: Integrate with ngx-toastr or similar library
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  /**
   * Show success notification
   */
  success(message: string, title?: string): void {
    // TODO: Implement with toast library
    console.log('✅ SUCCESS:', title || 'Success', message);
    alert(`${title || 'Success'}: ${message}`);
  }

  /**
   * Show error notification
   */
  error(message: string, title?: string): void {
    // TODO: Implement with toast library
    console.error('❌ ERROR:', title || 'Error', message);
    alert(`${title || 'Error'}: ${message}`);
  }

  /**
   * Show warning notification
   */
  warning(message: string, title?: string): void {
    // TODO: Implement with toast library
    console.warn('⚠️  WARNING:', title || 'Warning', message);
    alert(`${title || 'Warning'}: ${message}`);
  }

  /**
   * Show info notification
   */
  info(message: string, title?: string): void {
    // TODO: Implement with toast library
    console.info('ℹ️  INFO:', title || 'Info', message);
  }
}
