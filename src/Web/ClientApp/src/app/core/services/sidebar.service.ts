import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SidebarService {
  private isCollapsed$ = new BehaviorSubject<boolean>(false);

  constructor() {}

  /**
   * Get sidebar collapsed state as observable
   */
  get collapsed$(): Observable<boolean> {
    return this.isCollapsed$.asObservable();
  }

  /**
   * Get current collapsed state
   */
  get isCollapsed(): boolean {
    return this.isCollapsed$.value;
  }

  /**
   * Toggle sidebar collapsed state
   */
  toggle(): void {
    this.isCollapsed$.next(!this.isCollapsed$.value);
  }

  /**
   * Set specific collapsed state
   */
  setCollapsed(collapsed: boolean): void {
    this.isCollapsed$.next(collapsed);
  }
}
