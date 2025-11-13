import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faFolderOpen,
  faEye,
  faChartBar,
  faFileAlt,
  faInfoCircle
} from '@fortawesome/free-solid-svg-icons';

interface ViewerStat {
  title: string;
  value: number;
  icon: any;
  color: string;
  bgColor: string;
}

interface ViewableProject {
  id: string;
  name: string;
  progress: number;
  status: 'active' | 'completed' | 'on-hold';
  lastUpdated: Date;
  description: string;
}

interface RecentActivity {
  id: string;
  action: string;
  project: string;
  timestamp: Date;
  user: string;
}

@Component({
  selector: 'app-viewer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FontAwesomeModule],
  templateUrl: './viewer-dashboard.component.html',
  styleUrl: './viewer-dashboard.component.css'
})
export class ViewerDashboardComponent implements OnInit {
  // FontAwesome Icons
  faFolderOpen = faFolderOpen;
  faEye = faEye;
  faChartBar = faChartBar;
  faFileAlt = faFileAlt;
  faInfoCircle = faInfoCircle;

  stats: ViewerStat[] = [
    { title: 'Projects Assigned', value: 3, icon: this.faFolderOpen, color: 'text-blue-600 dark:text-blue-400', bgColor: 'bg-blue-100 dark:bg-blue-900/30' },
    { title: 'Active Projects', value: 2, icon: this.faEye, color: 'text-green-600 dark:text-green-400', bgColor: 'bg-green-100 dark:bg-green-900/30' },
    { title: 'Reports Available', value: 8, icon: this.faFileAlt, color: 'text-purple-600 dark:text-purple-400', bgColor: 'bg-purple-100 dark:bg-purple-900/30' },
    { title: 'Documents', value: 24, icon: this.faChartBar, color: 'text-orange-600 dark:text-orange-400', bgColor: 'bg-orange-100 dark:bg-orange-900/30' }
  ];

  assignedProjects: ViewableProject[] = [
    {
      id: '1',
      name: 'SOC 2 Audit',
      progress: 65,
      status: 'active',
      lastUpdated: new Date(Date.now() - 1000 * 60 * 60 * 2),
      description: 'Annual security compliance audit'
    },
    {
      id: '2',
      name: 'GDPR Compliance Review',
      progress: 45,
      status: 'active',
      lastUpdated: new Date(Date.now() - 1000 * 60 * 60 * 5),
      description: 'Data protection compliance assessment'
    },
    {
      id: '3',
      name: 'Security Assessment Q1',
      progress: 100,
      status: 'completed',
      lastUpdated: new Date(Date.now() - 1000 * 60 * 60 * 24 * 3),
      description: 'Quarterly security review and assessment'
    }
  ];

  recentActivities: RecentActivity[] = [
    {
      id: '1',
      action: 'Updated project documentation',
      project: 'SOC 2 Audit',
      timestamp: new Date(Date.now() - 1000 * 60 * 30),
      user: 'John Doe'
    },
    {
      id: '2',
      action: 'Completed security assessment',
      project: 'GDPR Compliance Review',
      timestamp: new Date(Date.now() - 1000 * 60 * 60 * 2),
      user: 'Jane Smith'
    },
    {
      id: '3',
      action: 'Added new compliance checklist',
      project: 'SOC 2 Audit',
      timestamp: new Date(Date.now() - 1000 * 60 * 60 * 4),
      user: 'Mike Johnson'
    },
    {
      id: '4',
      action: 'Generated monthly report',
      project: 'Security Assessment Q1',
      timestamp: new Date(Date.now() - 1000 * 60 * 60 * 6),
      user: 'Sarah Wilson'
    }
  ];

  ngOnInit(): void {}

  getStatusBadgeClass(status: string): string {
    const classes = {
      'active': 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300',
      'completed': 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300',
      'on-hold': 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300'
    };
    return classes[status as keyof typeof classes] || classes['active'];
  }

  getProgressColor(progress: number): string {
    if (progress >= 75) return 'bg-green-500';
    if (progress >= 50) return 'bg-blue-500';
    if (progress >= 25) return 'bg-yellow-500';
    return 'bg-red-500';
  }

  getTimeAgo(date: Date): string {
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
    if (seconds < 60) return `${seconds}s ago`;
    if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
    if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
    return `${Math.floor(seconds / 86400)}d ago`;
  }
}
