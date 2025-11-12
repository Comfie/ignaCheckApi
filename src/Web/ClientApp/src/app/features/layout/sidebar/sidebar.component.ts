import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faHome,
  faChartLine,
  faUsers,
  faFolderOpen,
  faClipboardCheck,
  faFileAlt,
  faCog,
  faQuestionCircle,
  faChevronDown,
  faCircle
} from '@fortawesome/free-solid-svg-icons';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { NavigationConfig, MenuItem } from '../../../core/config/navigation.config';
import { UserRole } from '../../../models/enums/user-role.enum';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, FontAwesomeModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent implements OnInit {
  // FontAwesome Icons
  faHome = faHome;
  faChartLine = faChartLine;
  faUsers = faUsers;
  faFolderOpen = faFolderOpen;
  faClipboardCheck = faClipboardCheck;
  faFileAlt = faFileAlt;
  faCog = faCog;
  faQuestionCircle = faQuestionCircle;
  faChevronDown = faChevronDown;
  faCircle = faCircle;

  menuItems: MenuItem[] = [];
  activeRoute: string = '';
  expandedMenus: Set<string> = new Set();
  isDarkMode = false;
  isSidebarCollapsed = false;

  constructor(
    private authService: AuthService,
    private themeService: ThemeService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadMenu();
    this.trackActiveRoute();

    // Subscribe to theme changes
    this.themeService.theme$.subscribe(isDark => {
      this.isDarkMode = isDark;
    });
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

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  getIconForRoute(route?: string): any {
    if (!route) return this.faCircle;

    if (route.includes('dashboard')) return this.faHome;
    if (route.includes('analytics')) return this.faChartLine;
    if (route.includes('users') || route.includes('team')) return this.faUsers;
    if (route.includes('projects')) return this.faFolderOpen;
    if (route.includes('audits') || route.includes('compliance')) return this.faClipboardCheck;
    if (route.includes('reports')) return this.faFileAlt;
    if (route.includes('settings')) return this.faCog;
    if (route.includes('help')) return this.faQuestionCircle;

    return this.faCircle;
  }

  getBadgeClasses(badgeClass: string): string {
    const classMap: Record<string, string> = {
      'danger': 'bg-red-500/20 text-red-400 dark:bg-red-500/30 dark:text-red-300 border border-red-500/40',
      'warning': 'bg-yellow-500/20 text-yellow-400 dark:bg-yellow-500/30 dark:text-yellow-300 border border-yellow-500/40',
      'info': 'bg-blue-500/20 text-blue-400 dark:bg-blue-500/30 dark:text-blue-300 border border-blue-500/40',
      'secondary': 'bg-gray-500/20 text-gray-400 dark:bg-gray-500/30 dark:text-gray-300 border border-gray-500/40',
      'success': 'bg-green-500/20 text-green-400 dark:bg-green-500/30 dark:text-green-300 border border-green-500/40',
      'primary': 'bg-indigo-500/20 text-indigo-400 dark:bg-indigo-500/30 dark:text-indigo-300 border border-indigo-500/40',
    };
    return classMap[badgeClass] || classMap['secondary'];
  }
}
