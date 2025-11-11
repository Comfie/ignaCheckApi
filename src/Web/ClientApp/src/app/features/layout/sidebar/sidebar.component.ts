import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { NavigationConfig, MenuItem } from '../../../core/config/navigation.config';
import { UserRole } from '../../../models/enums/user-role.enum';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  menuItems: MenuItem[] = [];
  activeRoute: string = '';
  expandedMenus: Set<string> = new Set();

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadMenu();
    this.trackActiveRoute();
  }

  private loadMenu(): void {
    const user = this.authService.getCurrentUser();
    const userRole = user?.role || UserRole.Viewer;
    this.menuItems = NavigationConfig.getMenuItems(userRole);
  }

  private trackActiveRoute(): void {
    // Set initial active route
    this.activeRoute = this.router.url;

    // Track route changes
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.activeRoute = event.url;
    });
  }

  isActive(item: MenuItem): boolean {
    if (item.route) {
      return this.activeRoute === item.route || this.activeRoute.startsWith(item.route + '/');
    }

    // Check if any children are active
    if (item.children) {
      return item.children.some(child => child.route && this.activeRoute.startsWith(child.route));
    }

    return false;
  }

  toggleMenu(label: string): void {
    if (this.expandedMenus.has(label)) {
      this.expandedMenus.delete(label);
    } else {
      this.expandedMenus.add(label);
    }
  }

  isExpanded(label: string): boolean {
    return this.expandedMenus.has(label);
  }

  isDivider(item: MenuItem): boolean {
    return item.divider === true;
  }

  hasChildren(item: MenuItem): boolean {
    return !!(item.children && item.children.length > 0);
  }

  getBadgeClasses(badgeClass: string): string {
    const classMap: Record<string, string> = {
      'danger': 'bg-red-500/15 text-red-300 border border-red-500/30',
      'warning': 'bg-yellow-500/15 text-yellow-300 border border-yellow-500/30',
      'info': 'bg-blue-500/15 text-blue-300 border border-blue-500/30',
      'secondary': 'bg-gray-500/15 text-gray-300 border border-gray-500/30',
      'success': 'bg-green-500/15 text-green-300 border border-green-500/30',
      'primary': 'bg-primary-500/15 text-primary-300 border border-primary-500/30',
    };
    return classMap[badgeClass] || classMap['secondary'];
  }
}
