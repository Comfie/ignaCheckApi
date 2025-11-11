import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UserRole } from '../../models/enums/user-role.enum';
import { User } from '../../core/models/user.model';

interface DashboardStat {
  title: string;
  value: string | number;
  icon: string;
  color: string;
  trend?: {
    value: string;
    isPositive: boolean;
  };
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  currentUser: User | null = null;
  userRole: UserRole = UserRole.Viewer;
  stats: DashboardStat[] = [];
  recentActivities: any[] = [];
  quickActions: any[] = [];

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
        icon: 'stroke-project',
        color: 'primary',
        trend: { value: '+3 this month', isPositive: true }
      },
      {
        title: 'Total Users',
        value: 156,
        icon: 'stroke-user',
        color: 'success',
        trend: { value: '+12 this week', isPositive: true }
      },
      {
        title: 'Active Projects',
        value: 47,
        icon: 'stroke-board',
        color: 'warning'
      },
      {
        title: 'Compliance Score',
        value: '87%',
        icon: 'stroke-charts',
        color: 'info',
        trend: { value: '+5%', isPositive: true }
      }
    ];

    this.quickActions = [
      { label: 'Create Workspace', route: '/workspaces/create', icon: 'plus' },
      { label: 'Manage Users', route: '/users', icon: 'users' },
      { label: 'View Reports', route: '/reports', icon: 'bar-chart' },
      { label: 'System Settings', route: '/settings/workspace', icon: 'settings' }
    ];
  }

  private loadAdminDashboard(): void {
    // Admin sees workspace-level metrics
    this.stats = [
      {
        title: 'Active Projects',
        value: 8,
        icon: 'stroke-project',
        color: 'primary'
      },
      {
        title: 'Team Members',
        value: 24,
        icon: 'stroke-user',
        color: 'success'
      },
      {
        title: 'Open Findings',
        value: 34,
        icon: 'stroke-learning',
        color: 'danger',
        trend: { value: '-8 this week', isPositive: true }
      },
      {
        title: 'Documents',
        value: 142,
        icon: 'stroke-file',
        color: 'info'
      }
    ];

    this.quickActions = [
      { label: 'Create Project', route: '/projects/create', icon: 'plus' },
      { label: 'Invite Users', route: '/users/invitations', icon: 'user-plus' },
      { label: 'Upload Documents', route: '/documents/upload', icon: 'upload' },
      { label: 'Compliance Report', route: '/reports/compliance', icon: 'file-text' }
    ];
  }

  private loadUserDashboard(): void {
    // Regular users see personal metrics
    this.stats = [
      {
        title: 'My Projects',
        value: 3,
        icon: 'stroke-project',
        color: 'primary'
      },
      {
        title: 'Assigned Tasks',
        value: 12,
        icon: 'stroke-board',
        color: 'warning'
      },
      {
        title: 'My Findings',
        value: 8,
        icon: 'stroke-learning',
        color: 'info'
      },
      {
        title: 'Documents',
        value: 28,
        icon: 'stroke-file',
        color: 'success'
      }
    ];

    this.quickActions = [
      { label: 'View Projects', route: '/projects', icon: 'folder' },
      { label: 'My Tasks', route: '/tasks', icon: 'check-square' },
      { label: 'Upload Document', route: '/documents/upload', icon: 'upload' },
      { label: 'View Reports', route: '/reports', icon: 'bar-chart' }
    ];

    // Load recent activities (mock data)
    this.recentActivities = [
      {
        type: 'document',
        message: 'New document uploaded: Security Policy.pdf',
        time: '2 hours ago',
        icon: 'file-text'
      },
      {
        type: 'finding',
        message: 'Finding #45 assigned to you',
        time: '5 hours ago',
        icon: 'alert-circle'
      },
      {
        type: 'comment',
        message: 'New comment on ISO 27001 Project',
        time: '1 day ago',
        icon: 'message-square'
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
