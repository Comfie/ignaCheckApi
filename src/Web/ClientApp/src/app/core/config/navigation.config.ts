import { UserRole } from '../../models/enums/user-role.enum';

export interface MenuItem {
  label?: string;
  icon?: string;
  iconStroke?: string;
  iconFill?: string;
  route?: string;
  roles?: UserRole[];
  children?: MenuItem[];
  divider?: boolean;
  badge?: {
    text: string;
    class: string;
  };
}

/**
 * Navigation Menu Configuration
 * Defines sidebar menu items with role-based access control
 */
export class NavigationConfig {

  /**
   * Get complete menu based on user role
   */
  static getMenuItems(userRole: UserRole): MenuItem[] {
    const allMenuItems = this.getAllMenuItems();

    // Filter menu items based on user role
    return allMenuItems.filter(item => this.hasAccess(item, userRole));
  }

  /**
   * Check if user has access to a menu item
   */
  private static hasAccess(item: MenuItem, userRole: UserRole): boolean {
    if (item.roles && item.roles.length > 0) {
      return item.roles.includes(userRole);
    }

    // If no roles specified, accessible to all
    return true;
  }

  /**
   * Define all menu items with role-based access
   */
  private static getAllMenuItems(): MenuItem[] {
    return [
      // Dashboard Section
      {
        label: 'Dashboard',
        iconStroke: 'stroke-home',
        iconFill: 'fill-home',
        route: '/dashboard'
      },

      // System Management - SuperAdmin Only
      {
        label: 'System Management',
        iconStroke: 'stroke-setting',
        iconFill: 'fill-setting',
        children: [
          {
            label: 'All Workspaces',
            route: '/system/workspaces'
          },
          {
            label: 'System Users',
            route: '/system/users'
          },
          {
            label: 'System Health',
            route: '/system/health'
          },
          {
            label: 'Activity Logs',
            route: '/system/logs'
          }
        ],
        roles: [UserRole.SuperAdmin]
      },

      // Workspaces - Owner Only
      {
        label: 'Workspaces',
        iconStroke: 'stroke-project',
        iconFill: 'fill-project',
        route: '/workspaces',
        roles: [UserRole.Owner]
      },

      { divider: true },

      // Main Application Section
      {
        label: 'Projects',
        iconStroke: 'stroke-project',
        iconFill: 'fill-project',
        children: [
          {
            label: 'All Projects',
            route: '/projects'
          },
          {
            label: 'Create New',
            route: '/projects/create',
            roles: [UserRole.Owner, UserRole.Admin, UserRole.Contributor]
          }
        ]
      },

      {
        label: 'Documents',
        iconStroke: 'stroke-file',
        iconFill: 'fill-file',
        route: '/documents'
      },

      {
        label: 'Findings',
        iconStroke: 'stroke-learning',
        iconFill: 'fill-learning',
        route: '/findings'
      },

      {
        label: 'Frameworks',
        iconStroke: 'stroke-board',
        iconFill: 'fill-board',
        route: '/frameworks'
      },

      { divider: true },

      // User Management - Owner & Admin Only
      {
        label: 'Users',
        iconStroke: 'stroke-user',
        iconFill: 'fill-user',
        children: [
          {
            label: 'All Users',
            route: '/users',
            roles: [UserRole.Owner, UserRole.Admin]
          },
          {
            label: 'Invitations',
            route: '/users/invitations',
            roles: [UserRole.Owner, UserRole.Admin]
          },
          {
            label: 'Roles & Permissions',
            route: '/users/roles',
            roles: [UserRole.Owner, UserRole.Admin]
          }
        ],
        roles: [UserRole.Owner, UserRole.Admin]
      },

      // Reports & Analytics
      {
        label: 'Reports',
        iconStroke: 'stroke-charts',
        iconFill: 'fill-charts',
        children: [
          {
            label: 'Compliance Dashboard',
            route: '/reports/compliance'
          },
          {
            label: 'Audit Trail',
            route: '/reports/audit-trail',
            roles: [UserRole.Owner, UserRole.Admin]
          },
          {
            label: 'Export Reports',
            route: '/reports/export'
          }
        ]
      },

      { divider: true },

      // Settings
      {
        label: 'Settings',
        iconStroke: 'stroke-setting',
        iconFill: 'fill-setting',
        children: [
          {
            label: 'Profile',
            route: '/settings/profile'
          },
          {
            label: 'Workspace Settings',
            route: '/settings/workspace',
            roles: [UserRole.Owner, UserRole.Admin]
          },
          {
            label: 'Notifications',
            route: '/settings/notifications'
          },
          {
            label: 'Security',
            route: '/settings/security'
          }
        ]
      }
    ];
  }

  /**
   * Get dashboard route based on user role
   */
  static getDashboardRoute(userRole: UserRole): string {
    switch (userRole) {
      case UserRole.SuperAdmin:
        return '/dashboard/super-admin';
      case UserRole.Owner:
        return '/dashboard/workspace-owner';
      case UserRole.Admin:
        return '/dashboard/workspace-owner'; // Admin uses same dashboard as Owner
      case UserRole.Contributor:
        return '/dashboard/contributor';
      case UserRole.Viewer:
        return '/dashboard/viewer';
      default:
        return '/dashboard';
    }
  }
}
