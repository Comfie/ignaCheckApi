import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'app-theme';
  private isDarkMode$ = new BehaviorSubject<boolean>(false);

  constructor() {
    this.initializeTheme();
  }

  /**
   * Initialize theme from localStorage or system preference
   */
  private initializeTheme(): void {
    const savedTheme = localStorage.getItem(this.THEME_KEY);

    if (savedTheme) {
      this.isDarkMode$.next(savedTheme === 'dark');
    } else {
      // Check system preference
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.isDarkMode$.next(prefersDark);
    }

    this.applyTheme(this.isDarkMode$.value);
  }

  /**
   * Get current theme as observable
   */
  get theme$(): Observable<boolean> {
    return this.isDarkMode$.asObservable();
  }

  /**
   * Get current theme value
   */
  get isDarkMode(): boolean {
    return this.isDarkMode$.value;
  }

  /**
   * Toggle between light and dark mode
   */
  toggleTheme(): void {
    const newTheme = !this.isDarkMode$.value;
    this.isDarkMode$.next(newTheme);
    this.applyTheme(newTheme);
    localStorage.setItem(this.THEME_KEY, newTheme ? 'dark' : 'light');
  }

  /**
   * Set specific theme
   */
  setTheme(isDark: boolean): void {
    this.isDarkMode$.next(isDark);
    this.applyTheme(isDark);
    localStorage.setItem(this.THEME_KEY, isDark ? 'dark' : 'light');
  }

  /**
   * Apply theme to document
   */
  private applyTheme(isDark: boolean): void {
    if (isDark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }
}
