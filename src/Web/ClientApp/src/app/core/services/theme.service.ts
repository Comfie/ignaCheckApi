import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'theme';
  private isDarkMode$ = new BehaviorSubject<boolean>(false);
  private mql: MediaQueryList | null = null;

  constructor() {
    // Don't initialize in constructor - wait for explicit init() call
  }

  /**
   * Initialize theme - MUST be called from AppComponent
   */
  init(): void {
    console.log('🚀 ThemeService.init() called');
    const saved = localStorage.getItem(this.THEME_KEY) as 'light' | 'dark' | 'system' | null;

    if (saved === 'system') {
      this.attachSystemListener();
      this.applySystem();
    } else if (saved === 'dark' || saved === 'light') {
      this.detachSystemListener();
      // Apply without animation on init
      const isDark = saved === 'dark';
      this.applyDark(isDark);
      this.isDarkMode$.next(isDark);
    } else {
      // Default to system preference if no saved theme
      this.attachSystemListener();
      this.applySystem();
      localStorage.setItem(this.THEME_KEY, 'system');
    }
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
   * Get current theme state
   */
  get current(): 'light' | 'dark' {
    const isDark = document.documentElement.classList.contains('dark');
    return isDark ? 'dark' : 'light';
  }

  /**
   * Get saved theme mode from localStorage
   */
  getSavedMode(): 'light' | 'dark' | 'system' {
    const saved = localStorage.getItem(this.THEME_KEY) as 'light' | 'dark' | 'system' | null;
    return saved ?? 'system';
  }

  /**
   * Toggle between light and dark mode
   */
  toggle(): void {
    console.log('🎨 ThemeService.toggle() called, current:', this.current);
    const current = this.current;
    this.setTheme(current === 'dark' ? 'light' : 'dark');
  }

  /**
   * Set specific theme
   */
  setTheme(mode: 'light' | 'dark' | 'system'): void {
    console.log('🎨 ThemeService.setTheme() called with:', mode);

    const root = document.documentElement;
    // Add short-lived theming class to scope transitions during toggle
    root.classList.add('theming');
    window.setTimeout(() => root.classList.remove('theming'), 250);

    if (mode === 'dark') {
      this.detachSystemListener();
      this.applyDark(true);
      this.isDarkMode$.next(true);
      localStorage.setItem(this.THEME_KEY, 'dark');
    } else if (mode === 'light') {
      this.detachSystemListener();
      this.applyDark(false);
      this.isDarkMode$.next(false);
      localStorage.setItem(this.THEME_KEY, 'light');
    } else {
      this.attachSystemListener();
      this.applySystem();
      localStorage.setItem(this.THEME_KEY, 'system');
    }
  }

  /**
   * Apply dark mode to document
   */
  private applyDark(dark: boolean): void {
    console.log('🎨 ApplyDark called with:', dark);
    const root = document.documentElement;
    const body = document.body;
    const appRoot = document.querySelector('app-root');

    root.classList.toggle('dark', dark);

    // Cleanup: never leave 'dark' lingering on body/app-root
    body.classList.remove('dark');
    if (appRoot) {
      appRoot.classList.remove('dark');
    }
  }

  /**
   * Apply system theme preference
   */
  private applySystem(): void {
    const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    console.log('🎨 ApplySystem called, prefersDark:', prefersDark);
    this.applyDark(prefersDark);
    this.isDarkMode$.next(prefersDark);
  }

  /**
   * Attach system theme change listener
   */
  private attachSystemListener(): void {
    if (!window.matchMedia) return;

    // Detach any previous handler
    this.detachSystemListener();

    this.mql = window.matchMedia('(prefers-color-scheme: dark)');
    const handler = () => this.applySystem();

    this.mql.addEventListener?.('change', handler as EventListener);
    // Fallback for older Safari
    // @ts-ignore
    this.mql.addListener?.(handler);

    (this as any)._systemHandler = handler;
  }

  /**
   * Detach system theme change listener
   */
  private detachSystemListener(): void {
    if (!this.mql) return;

    const handler = (this as any)._systemHandler as (() => void) | undefined;
    if (handler) {
      this.mql.removeEventListener?.('change', handler as EventListener);
      // @ts-ignore
      this.mql.removeListener?.(handler);
    }

    this.mql = null;
    delete (this as any)._systemHandler;
  }
}
