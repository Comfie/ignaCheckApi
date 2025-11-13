import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faBuilding,
  faUsers,
  faChartLine,
  faServer,
  faPlus,
  faEllipsisV,
  faCheckCircle,
  faExclamationTriangle,
  faClock
} from '@fortawesome/free-solid-svg-icons';

interface SystemStat {
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

interface Workspace {
  id: string;
  name: string;
  owner: string;
  users: number;
  projects: number;
  status: 'active' | 'inactive' | 'trial';
  createdAt: Date;
}

interface SystemActivity {
  id: string;
  type: 'workspace_created' | 'user_added' | 'subscription_changed' | 'system_alert';
  message: string;
  timestamp: Date;
  icon: any;
  color: string;
}

@Component({
  selector: 'app-super-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FontAwesomeModule],
  templateUrl: './super-admin-dashboard.component.html',
  styleUrl: './super-admin-dashboard.component.css'
})
export class SuperAdminDashboardComponent implements OnInit {
  // FontAwesome Icons
  faBuilding = faBuilding;
  faUsers = faUsers;
  faChartLine = faChartLine;
  faServer = faServer;
  faPlus = faPlus;
  faEllipsisV = faEllipsisV;
  faCheckCircle = faCheckCircle;
  faExclamationTriangle = faExclamationTriangle;
  faClock = faClock;

  systemStats: SystemStat[] = [];
  recentWorkspaces: Workspace[] = [];
  systemActivities: SystemActivity[] = [];

  ngOnInit(): void {
    this.loadSystemStats();
    this.loadRecentWorkspaces();
    this.loadSystemActivities();
  }

  private loadSystemStats(): void {
    this.systemStats = [
      {
        title: 'Total Workspaces',
        value: 48,
        icon: this.faBuilding,
        color: 'text-blue-600 dark:text-blue-400',
        bgColor: 'bg-blue-100 dark:bg-blue-900/30',
        trend: { value: '+12%', isPositive: true }
      },
      {
        title: 'Total Users',
        value: 2847,
        icon: this.faUsers,
        color: 'text-green-600 dark:text-green-400',
        bgColor: 'bg-green-100 dark:bg-green-900/30',
        trend: { value: '+8%', isPositive: true }
      },
      {
        title: 'Active Projects',
        value: 324,
        icon: this.faChartLine,
        color: 'text-purple-600 dark:text-purple-400',
        bgColor: 'bg-purple-100 dark:bg-purple-900/30',
        trend: { value: '+24%', isPositive: true }
      },
      {
        title: 'System Health',
        value: '99.9%',
        icon: this.faServer,
        color: 'text-emerald-600 dark:text-emerald-400',
        bgColor: 'bg-emerald-100 dark:bg-emerald-900/30',
        trend: { value: 'Optimal', isPositive: true }
      }
    ];
  }

  private loadRecentWorkspaces(): void {
    this.recentWorkspaces = [
      {
        id: '1',
        name: 'Acme Corporation',
        owner: 'John Smith',
        users: 45,
        projects: 12,
        status: 'active',
        createdAt: new Date('2024-01-15')
      },
      {
        id: '2',
        name: 'TechStart Inc',
        owner: 'Sarah Johnson',
        users: 23,
        projects: 8,
        status: 'trial',
        createdAt: new Date('2024-02-01')
      },
      {
        id: '3',
        name: 'Global Finance Ltd',
        owner: 'Michael Brown',
        users: 67,
        projects: 24,
        status: 'active',
        createdAt: new Date('2024-01-08')
      }
    ];
  }

  private loadSystemActivities(): void {
    this.systemActivities = [
      {
        id: '1',
        type: 'workspace_created',
        message: 'New workspace "Enterprise Solutions" created',
        timestamp: new Date(Date.now() - 1000 * 60 * 15),
        icon: this.faBuilding,
        color: 'text-blue-600 dark:text-blue-400'
      },
      {
        id: '2',
        type: 'user_added',
        message: '15 new users registered across all workspaces',
        timestamp: new Date(Date.now() - 1000 * 60 * 45),
        icon: this.faUsers,
        color: 'text-green-600 dark:text-green-400'
      },
      {
        id: '3',
        type: 'subscription_changed',
        message: 'Acme Corporation upgraded to Enterprise plan',
        timestamp: new Date(Date.now() - 1000 * 60 * 120),
        icon: this.faCheckCircle,
        color: 'text-purple-600 dark:text-purple-400'
      },
      {
        id: '4',
        type: 'system_alert',
        message: 'Database backup completed successfully',
        timestamp: new Date(Date.now() - 1000 * 60 * 180),
        icon: this.faServer,
        color: 'text-emerald-600 dark:text-emerald-400'
      }
    ];
  }

  getStatusBadgeClass(status: string): string {
    const classes = {
      'active': 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300',
      'inactive': 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300',
      'trial': 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300'
    };
    return classes[status as keyof typeof classes] || classes['inactive'];
  }

  getTimeAgo(date: Date): string {
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);

    if (seconds < 60) return `${seconds}s ago`;
    if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
    if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
    return `${Math.floor(seconds / 86400)}d ago`;
  }
}
