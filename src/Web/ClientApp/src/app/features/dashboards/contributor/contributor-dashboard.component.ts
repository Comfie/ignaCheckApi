import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faFolderOpen,
  faClipboardCheck,
  faClock,
  faChartLine,
  faPlus,
  faCheckCircle
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-contributor-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FontAwesomeModule],
  templateUrl: './contributor-dashboard.component.html',
  styleUrl: './contributor-dashboard.component.css'
})
export class ContributorDashboardComponent implements OnInit {
  // FontAwesome Icons
  faFolderOpen = faFolderOpen;
  faClipboardCheck = faClipboardCheck;
  faClock = faClock;
  faChartLine = faChartLine;
  faPlus = faPlus;
  faCheckCircle = faCheckCircle;

  stats = [
    { title: 'My Projects', value: 5, icon: this.faFolderOpen, color: 'text-blue-600 dark:text-blue-400', bgColor: 'bg-blue-100 dark:bg-blue-900/30' },
    { title: 'Tasks Assigned', value: 12, icon: this.faClipboardCheck, color: 'text-green-600 dark:text-green-400', bgColor: 'bg-green-100 dark:bg-green-900/30' },
    { title: 'In Progress', value: 4, icon: this.faClock, color: 'text-yellow-600 dark:text-yellow-400', bgColor: 'bg-yellow-100 dark:bg-yellow-900/30' },
    { title: 'Completed', value: 8, icon: this.faCheckCircle, color: 'text-purple-600 dark:text-purple-400', bgColor: 'bg-purple-100 dark:bg-purple-900/30' }
  ];

  myProjects = [
    { id: '1', name: 'SOC 2 Audit', progress: 65, status: 'active', dueDate: new Date(Date.now() + 1000 * 60 * 60 * 24 * 5) },
    { id: '2', name: 'GDPR Compliance', progress: 45, status: 'active', dueDate: new Date(Date.now() + 1000 * 60 * 60 * 24 * 10) },
    { id: '3', name: 'Security Assessment', progress: 80, status: 'active', dueDate: new Date(Date.now() + 1000 * 60 * 60 * 24 * 3) }
  ];

  recentTasks = [
    { id: '1', title: 'Review security policies', project: 'SOC 2 Audit', priority: 'high', dueDate: new Date(Date.now() + 1000 * 60 * 60 * 24 * 2) },
    { id: '2', title: 'Update documentation', project: 'GDPR Compliance', priority: 'medium', dueDate: new Date(Date.now() + 1000 * 60 * 60 * 24 * 4) },
    { id: '3', title: 'Conduct risk assessment', project: 'Security Assessment', priority: 'high', dueDate: new Date(Date.now() + 1000 * 60 * 60 * 24 * 1) }
  ];

  ngOnInit(): void {}

  getProgressColor(progress: number): string {
    if (progress >= 75) return 'bg-green-500';
    if (progress >= 50) return 'bg-blue-500';
    if (progress >= 25) return 'bg-yellow-500';
    return 'bg-red-500';
  }

  getPriorityClass(priority: string): string {
    const classes = {
      'high': 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300',
      'medium': 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300',
      'low': 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300'
    };
    return classes[priority as keyof typeof classes] || classes['medium'];
  }
}
