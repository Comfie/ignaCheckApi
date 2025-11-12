import { Component, OnInit, HostListener } from '@angular/core';
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
  faCircle,
  faBuilding,
  faLightbulb,
  faBook,
  faBars,
  faChevronLeft,
  faChevronRight
} from '@fortawesome/free-solid-svg-icons';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { SidebarService } from '../../../core/services/sidebar.service';
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
  faBuilding = faBuilding;
  faLightbulb = faLightbulb;
  faBook = faBook;
  faBars = faBars;
  faChevronLeft = faChevronLeft;
  faChevronRight = faChevronRight;

  menuItems: MenuItem[] = [];
  activeRoute: string = '';
  expandedMenus: Set<string> = new Set();
  isDarkMode = false;
  isCollapsed = false;

  constructor(
    private authService: AuthService,
    private themeService: ThemeService,
    private sidebarService: SidebarService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadMenu();
    this.trackActiveRoute();

    // Subscribe to theme changes
    this.themeService.theme$.subscribe(isDark => {
      this.isDarkMode = isDark;
    });

    // Subscribe to sidebar state
    this.sidebarService.collapsed$.subscribe(collapsed => {
      this.isCollapsed = collapsed;
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    // Close dropdowns when clicking outside
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
    this.sidebarService.toggle();
  }

  getIconForItem(item: MenuItem): any {
    // Use label-based icon mapping since routes might be in children
    const label = item.label?.toLowerCase() || '';

    if (label.includes('dashboard')) return this.faHome;
    if (label.includes('workspace')) return this.faBuilding;
    if (label.includes('project')) return this.faFolderOpen;
    if (label.includes('document')) return this.faFileAlt;
    if (label.includes('finding')) return this.faLightbulb;
    if (label.includes('framework')) return this.faBook;
    if (label.includes('user') || label.includes('team')) return this.faUsers;
    if (label.includes('report') || label.includes('analytic')) return this.faChartLine;
    if (label.includes('setting')) return this.faCog;
    if (label.includes('help')) return this.faQuestionCircle;

    // Default icon for items with children
    if (this.hasChildren(item)) return this.faFolderOpen;

    return this.faCircle;
  }

  getBadgeClasses(badgeClass: string): string {
    const classMap: Record<string, string> = {
      'danger': 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300 border border-red-200 dark:border-red-800',
      'warning': 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300 border border-yellow-200 dark:border-yellow-800',
      'info': 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300 border border-blue-200 dark:border-blue-800',
      'secondary': 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-600',
      'success': 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300 border border-green-200 dark:border-green-800',
      'primary': 'bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800',
    };
    return classMap[badgeClass] || classMap['secondary'];
  }
}
