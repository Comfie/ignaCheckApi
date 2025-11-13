import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faUsers,
  faFolderOpen,
  faClipboardCheck,
  faChartLine,
  faPlus,
  faUserPlus,
  faCrown,
  faCheckCircle,
  faClock
} from '@fortawesome/free-solid-svg-icons';

interface WorkspaceStat {
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

interface TeamMember {
  id: string;
  name: string;
  email: string;
  role: string;
  status: 'active' | 'pending';
  lastActive: Date;
}

interface RecentProject {
  id: string;
  name: string;
  progress: number;
  status: 'active' | 'completed' | 'on-hold';
  team: number;
  dueDate: Date;
}

@Component({
  selector: 'app-workspace-owner-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FontAwesomeModule],
  templateUrl: './workspace-owner-dashboard.component.html',
  styleUrl: './workspace-owner-dashboard.component.css'
})
export class WorkspaceOwnerDashboardComponent implements OnInit {
  // FontAwesome Icons
  faUsers = faUsers;
  faFolderOpen = faFolderOpen;
  faClipboardCheck = faClipboardCheck;
  faChartLine = faChartLine;
  faPlus = faPlus;
  faUserPlus = faUserPlus;
  faCrown = faCrown;
  faCheckCircle = faCheckCircle;
  faClock = faClock;

  workspaceStats: WorkspaceStat[] = [];
  teamMembers: TeamMember[] = [];
  recentProjects: RecentProject[] = [];

  ngOnInit(): void {
    this.loadWorkspaceStats();
    this.loadTeamMembers();
    this.loadRecentProjects();
  }

  private loadWorkspaceStats(): void {
    this.workspaceStats = [
      {
        title: 'Team Members',
        value: 24,
        icon: this.faUsers,
        color: 'text-blue-600 dark:text-blue-400',
        bgColor: 'bg-blue-100 dark:bg-blue-900/30',
        trend: { value: '+3', isPositive: true }
      },
      {
        title: 'Active Projects',
        value: 8,
        icon: this.faFolderOpen,
        color: 'text-green-600 dark:text-green-400',
        bgColor: 'bg-green-100 dark:bg-green-900/30',
        trend: { value: '+2', isPositive: true }
      },
      {
        title: 'Completed Tasks',
        value: 156,
        icon: this.faClipboardCheck,
        color: 'text-purple-600 dark:text-purple-400',
        bgColor: 'bg-purple-100 dark:bg-purple-900/30',
        trend: { value: '+18%', isPositive: true }
      },
      {
        title: 'Compliance Score',
        value: '94%',
        icon: this.faChartLine,
        color: 'text-emerald-600 dark:text-emerald-400',
        bgColor: 'bg-emerald-100 dark:bg-emerald-900/30',
        trend: { value: '+5%', isPositive: true }
      }
    ];
  }

  private loadTeamMembers(): void {
    this.teamMembers = [
      {
        id: '1',
        name: 'John Doe',
        email: 'john@example.com',
        role: 'Admin',
        status: 'active',
        lastActive: new Date(Date.now() - 1000 * 60 * 15)
      },
      {
        id: '2',
        name: 'Jane Smith',
        email: 'jane@example.com',
        role: 'Contributor',
        status: 'active',
        lastActive: new Date(Date.now() - 1000 * 60 * 45)
      },
      {
        id: '3',
        name: 'Mike Johnson',
        email: 'mike@example.com',
        role: 'Viewer',
        status: 'pending',
        lastActive: new Date(Date.now() - 1000 * 60 * 120)
      }
    ];
  }

  private loadRecentProjects(): void {
    this.recentProjects = [
      {
        id: '1',
        name: 'SOC 2 Compliance Audit',
        progress: 75,
        status: 'active',
        team: 5,
        dueDate: new Date(Date.now() + 1000 * 60 * 60 * 24 * 7)
      },
      {
        id: '2',
        name: 'ISO 27001 Certification',
        progress: 100,
        status: 'completed',
        team: 3,
        dueDate: new Date(Date.now() - 1000 * 60 * 60 * 24 * 5)
      },
      {
        id: '3',
        name: 'GDPR Assessment',
        progress: 45,
        status: 'active',
        team: 4,
        dueDate: new Date(Date.now() + 1000 * 60 * 60 * 24 * 14)
      }
    ];
  }

  getRoleBadgeClass(role: string): string {
    const classes = {
      'Admin': 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300',
      'Contributor': 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300',
      'Viewer': 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300'
    };
    return classes[role as keyof typeof classes] || classes['Viewer'];
  }

  getStatusBadgeClass(status: string): string {
    const classes = {
      'active': 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300',
      'pending': 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300',
      'completed': 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300',
      'on-hold': 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300'
    };
    return classes[status as keyof typeof classes] || classes['active'];
  }

  getTimeAgo(date: Date): string {
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
    if (seconds < 60) return `${seconds}s ago`;
    if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
    if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
    return `${Math.floor(seconds / 86400)}d ago`;
  }

  getProgressColor(progress: number): string {
    if (progress >= 75) return 'bg-green-500';
    if (progress >= 50) return 'bg-blue-500';
    if (progress >= 25) return 'bg-yellow-500';
    return 'bg-red-500';
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }
}
