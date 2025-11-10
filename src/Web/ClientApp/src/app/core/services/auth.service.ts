import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '../models/user.model';
import { TokenService } from './token.service';

/**
 * Authentication Service
 * Handles user authentication and authorization
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private readonly API_URL = `${environment.apiUrl}/authentication`;

  constructor(
    private http: HttpClient,
    private tokenService: TokenService,
    private router: Router
  ) {
    // Initialize current user if token exists
    if (this.isAuthenticated()) {
      // TODO: Fetch user from token or API
      // For now, just set null - implement token decoding or user fetch
    }
  }

  /**
   * Login user
   */
  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.API_URL}/login`, request).pipe(
      tap(response => {
        this.tokenService.saveToken(response.token);
        if (response.refreshToken) {
          this.tokenService.saveRefreshToken(response.refreshToken);
        }
        this.currentUserSubject.next(response.user);
      })
    );
  }

  /**
   * Register new user
   */
  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.API_URL}/register`, request);
  }

  /**
   * Logout user
   */
  logout(): void {
    this.tokenService.clearTokens();
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.tokenService.hasToken();
  }

  /**
   * Get current user
   */
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.role === role;
  }

  /**
   * Check if user has any of the specified roles
   */
  hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUser();
    return user ? roles.includes(user.role) : false;
  }
}
