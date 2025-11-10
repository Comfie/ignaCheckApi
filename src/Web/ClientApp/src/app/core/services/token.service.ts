import { Injectable } from '@angular/core';

/**
 * Token Service
 * Handles JWT token storage and retrieval
 */
@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly TOKEN_KEY = 'ignacheck_token';
  private readonly REFRESH_TOKEN_KEY = 'ignacheck_refresh_token';

  /**
   * Save JWT token to localStorage
   */
  saveToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  /**
   * Get JWT token from localStorage
   */
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  /**
   * Save refresh token to localStorage
   */
  saveRefreshToken(refreshToken: string): void {
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  /**
   * Get refresh token from localStorage
   */
  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  /**
   * Remove tokens from localStorage
   */
  clearTokens(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
  }

  /**
   * Check if token exists
   */
  hasToken(): boolean {
    return !!this.getToken();
  }
}
