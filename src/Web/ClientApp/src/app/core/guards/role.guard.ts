import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

/**
 * Role Guard
 * Protects routes based on user roles
 * Usage: Add to route data: { roles: ['Owner', 'Admin'] }
 */
@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    const requiredRoles = route.data['roles'] as string[];

    if (!requiredRoles || requiredRoles.length === 0) {
      return true;
    }

    if (this.authService.hasAnyRole(requiredRoles)) {
      return true;
    }

    // User doesn't have required role
    this.notificationService.error('You don\'t have permission to access this page');
    this.router.navigate(['/dashboard']);
    return false;
  }
}
