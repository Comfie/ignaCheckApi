import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faBuilding,
  faUsers,
  faFolderOpen,
  faChartLine,
  faClipboardCheck,
  faFileAlt,
  faLightbulb,
  faPlus,
  faUserPlus,
  faUpload,
  faFileText,
  faFolder,
  faCheckSquare,
  faBarChart,
  faClock,
  faArrowUp,
  faArrowDown
} from '@fortawesome/free-solid-svg-icons';
import { AuthService } from '../../core/services/auth.service';
import { UserRole } from '../../models/enums/user-role.enum';
import { User } from '../../core/models/user.model';

interface DashboardStat {
  title: string;
  value: string | number;
  icon: any;
  color: string;
  bgColor: string;
  trend?: {
    value: string;
    isPositive: boolean;
  };
}

interface QuickAction {
  label: string;
  route: string;
  icon: any;
  color: string;
}

interface RecentActivity {
  type: string;
  message: string;
  time: string;
  icon: any;
  color: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FontAwesomeModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  // FontAwesome Icons
  faBuilding = faBuilding;
  faUsers = faUsers;
  faFolderOpen = faFolderOpen;
  faChartLine = faChartLine;
  faArrowUp = faArrowUp;
  faArrowDown = faArrowDown;
  faClock = faClock;
  faClipboardCheck = faClipboardCheck;

  currentUser: User | null = null;
  userRole: UserRole = UserRole.Viewer;
  stats: DashboardStat[] = [];
  recentActivities: RecentActivity[] = [];
  quickActions: QuickAction[] = [];

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.loadUserInfo();
    this.loadDashboardData();
  }

  private loadUserInfo(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.userRole = this.currentUser?.role || UserRole.Viewer;
  }

  private loadDashboardData(): void {
    // Load role-specific dashboard data
    switch (this.userRole) {
      case UserRole.Owner:
        this.loadOwnerDashboard();
        break;
      case UserRole.Admin:
        this.loadAdminDashboard();
        break;
      case UserRole.Contributor:
      case UserRole.Viewer:
      default:
        this.loadUserDashboard();
        break;
    }
  }

  private loadOwnerDashboard(): void {
    // Owner sees global/system-wide metrics
    this.stats = [
      {
        title: 'Total Workspaces',
        value: 12,
        icon: faBuilding,
        color: 'text-blue-600 dark:text-blue-400',
        bgColor: 'bg-blue-100 dark:bg-blue-900/30',
        trend: { value: '+3 this month', isPositive: true }
      },
      {
        title: 'Total Users',
        value: 156,
        icon: faUsers,
        color: 'text-green-600 dark:text-green-400',
        bgColor: 'bg-green-100 dark:bg-green-900/30',
        trend: { value: '+12 this week', isPositive: true }
      },
      {
        title: 'Active Projects',
        value: 47,
        icon: faFolderOpen,
        color: 'text-purple-600 dark:text-purple-400',
        bgColor: 'bg-purple-100 dark:bg-purple-900/30'
      },
      {
        title: 'Compliance Score',
        value: '87%',
        icon: faChartLine,
        color: 'text-orange-600 dark:text-orange-400',
        bgColor: 'bg-orange-100 dark:bg-orange-900/30',
        trend: { value: '+5%', isPositive: true }
      }
    ];

    this.quickActions = [
      { label: 'Create Workspace', route: '/workspaces/create', icon: faPlus, color: 'blue' },
      { label: 'Manage Users', route: '/users', icon: faUsers, color: 'green' },
      { label: 'View Reports', route: '/reports', icon: faBarChart, color: 'purple' },
      { label: 'System Settings', route: '/settings/workspace', icon: faClipboardCheck, color: 'orange' }
    ];
  }

  private loadAdminDashboard(): void {
    // Admin sees workspace-level metrics
    this.stats = [
      {
        title: 'Active Projects',
        value: 8,
        icon: faFolderOpen,
        color: 'text-blue-600 dark:text-blue-400',
        bgColor: 'bg-blue-100 dark:bg-blue-900/30'
      },
      {
        title: 'Team Members',
        value: 24,
        icon: faUsers,
        color: 'text-green-600 dark:text-green-400',
        bgColor: 'bg-green-100 dark:bg-green-900/30'
      },
      {
        title: 'Open Findings',
        value: 34,
        icon: faLightbulb,
        color: 'text-red-600 dark:text-red-400',
        bgColor: 'bg-red-100 dark:bg-red-900/30',
        trend: { value: '-8 this week', isPositive: true }
      },
      {
        title: 'Documents',
        value: 142,
        icon: faFileAlt,
        color: 'text-purple-600 dark:text-purple-400',
        bgColor: 'bg-purple-100 dark:bg-purple-900/30'
      }
    ];

    this.quickActions = [
      { label: 'Create Project', route: '/projects/create', icon: faPlus, color: 'blue' },
      { label: 'Invite Users', route: '/users/invitations', icon: faUserPlus, color: 'green' },
      { label: 'Upload Documents', route: '/documents/upload', icon: faUpload, color: 'purple' },
      { label: 'Compliance Report', route: '/reports/compliance', icon: faFileText, color: 'orange' }
    ];
  }

  private loadUserDashboard(): void {
    // Regular users see personal metrics
    this.stats = [
      {
        title: 'My Projects',
        value: 3,
        icon: faFolderOpen,
        color: 'text-blue-600 dark:text-blue-400',
        bgColor: 'bg-blue-100 dark:bg-blue-900/30'
      },
      {
        title: 'Assigned Tasks',
        value: 12,
        icon: faCheckSquare,
        color: 'text-orange-600 dark:text-orange-400',
        bgColor: 'bg-orange-100 dark:bg-orange-900/30'
      },
      {
        title: 'My Findings',
        value: 8,
        icon: faLightbulb,
        color: 'text-purple-600 dark:text-purple-400',
        bgColor: 'bg-purple-100 dark:bg-purple-900/30'
      },
      {
        title: 'Documents',
        value: 28,
        icon: faFileAlt,
        color: 'text-green-600 dark:text-green-400',
        bgColor: 'bg-green-100 dark:bg-green-900/30'
      }
    ];

    this.quickActions = [
      { label: 'View Projects', route: '/projects', icon: faFolder, color: 'blue' },
      { label: 'My Tasks', route: '/tasks', icon: faCheckSquare, color: 'orange' },
      { label: 'Upload Document', route: '/documents/upload', icon: faUpload, color: 'purple' },
      { label: 'View Reports', route: '/reports', icon: faBarChart, color: 'green' }
    ];

    // Load recent activities (mock data)
    this.recentActivities = [
      {
        type: 'document',
        message: 'New document uploaded: Security Policy.pdf',
        time: '2 hours ago',
        icon: faFileText,
        color: 'blue'
      },
      {
        type: 'finding',
        message: 'Finding #45 assigned to you',
        time: '5 hours ago',
        icon: faLightbulb,
        color: 'orange'
      },
      {
        type: 'comment',
        message: 'New comment on ISO 27001 Project',
        time: '1 day ago',
        icon: faClipboardCheck,
        color: 'green'
      },
      {
        type: 'user',
        message: 'Sarah Johnson joined the workspace',
        time: '2 days ago',
        icon: faUserPlus,
        color: 'purple'
      },
      {
        type: 'project',
        message: 'Project "GDPR Compliance" completed',
        time: '3 days ago',
        icon: faCheckSquare,
        color: 'green'
      }
    ];
  }

  isOwner(): boolean {
    return this.userRole === UserRole.Owner;
  }

  isAdmin(): boolean {
    return this.userRole === UserRole.Admin;
  }

  isContributor(): boolean {
    return this.userRole === UserRole.Contributor;
  }

  isViewer(): boolean {
    return this.userRole === UserRole.Viewer;
  }

  getRoleDisplayName(): string {
    return this.userRole;
  }

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good Morning';
    if (hour < 18) return 'Good Afternoon';
    return 'Good Evening';
  }
}
