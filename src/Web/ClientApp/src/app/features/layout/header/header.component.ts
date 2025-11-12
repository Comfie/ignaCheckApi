import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faBell,
  faSearch,
  faMoon,
  faSun,
  faUser,
  faCog,
  faSignOutAlt,
  faBars
} from '@fortawesome/free-solid-svg-icons';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ThemeService } from '../../../core/services/theme.service';
import { SidebarService } from '../../../core/services/sidebar.service';
import { User } from '../../../core/models/user.model';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, FontAwesomeModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit {
  // FontAwesome Icons
  faBell = faBell;
  faSearch = faSearch;
  faMoon = faMoon;
  faSun = faSun;
  faUser = faUser;
  faCog = faCog;
  faSignOutAlt = faSignOutAlt;
  faBars = faBars;

  currentUser: User | null = null;
  showUserMenu = false;
  showNotifications = false;
  isDarkMode = false;
  isSidebarCollapsed = false;

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private themeService: ThemeService,
    private sidebarService: SidebarService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCurrentUser();
    this.themeService.theme$.subscribe(isDark => {
      this.isDarkMode = isDark;
    });
    this.sidebarService.collapsed$.subscribe(collapsed => {
      this.isSidebarCollapsed = collapsed;
    });
  }

  private loadCurrentUser(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });

    if (!this.currentUser) {
      this.currentUser = this.authService.getCurrentUser();
    }
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
    if (this.showUserMenu) {
      this.showNotifications = false;
    }
  }

  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
    if (this.showNotifications) {
      this.showUserMenu = false;
    }
  }

  closeDropdowns(): void {
    this.showUserMenu = false;
    this.showNotifications = false;
  }

  onLogout(): void {
    if (confirm('Are you sure you want to logout?')) {
      this.authService.logout();
      this.notificationService.success('You have been logged out successfully');
    }
  }

  navigateToProfile(): void {
    this.closeDropdowns();
    this.router.navigate(['/settings/profile']);
  }

  navigateToSettings(): void {
    this.closeDropdowns();
    this.router.navigate(['/settings']);
  }

  getUserInitials(): string {
    if (!this.currentUser || !this.currentUser.displayName) {
      return 'U';
    }

    const names = this.currentUser.displayName.split(' ');
    if (names.length >= 2) {
      return `${names[0][0]}${names[names.length - 1][0]}`.toUpperCase();
    }

    return this.currentUser.displayName.substring(0, 2).toUpperCase();
  }

  getRoleBadgeClass(): string {
    const role = this.currentUser?.role;
    switch (role) {
      case 'Owner':
        return 'badge-danger';
      case 'Admin':
        return 'badge-warning';
      case 'Contributor':
        return 'badge-info';
      case 'Viewer':
      default:
        return 'badge-secondary';
    }
  }

  getRoleBadgeClasses(): string {
    const role = this.currentUser?.role;
    switch (role) {
      case 'Owner':
        return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300';
      case 'Admin':
        return 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300';
      case 'Contributor':
        return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300';
      case 'Viewer':
      default:
        return 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300';
    }
  }
}
