import { Injectable, computed, signal } from '@angular/core';
import { LoginResponse, UserResponse } from '@/app/core/models/auth.models';

const ACCESS_TOKEN_KEY = 'parkease_access_token';
const REFRESH_TOKEN_KEY = 'parkease_refresh_token';
const EXPIRES_AT_KEY = 'parkease_token_expires_at';
const USER_KEY = 'parkease_user';

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private readonly accessTokenSignal = signal<string | null>(localStorage.getItem(ACCESS_TOKEN_KEY));
  private readonly refreshTokenSignal = signal<string | null>(localStorage.getItem(REFRESH_TOKEN_KEY));
  private readonly expiresAtSignal = signal<string | null>(localStorage.getItem(EXPIRES_AT_KEY));
  private readonly userSignal = signal<UserResponse | null>(this.readStoredUser());

  readonly accessToken = computed(() => this.accessTokenSignal());
  readonly refreshToken = computed(() => this.refreshTokenSignal());
  readonly expiresAt = computed(() => this.expiresAtSignal());
  readonly user = computed(() => this.userSignal());
  readonly isAuthenticated = computed(() => {
    const token = this.accessTokenSignal();
    const expiresAt = this.expiresAtSignal();

    if (!token || !expiresAt) {
      return false;
    }

    return new Date(expiresAt).getTime() > Date.now();
  });

  setSession(response: LoginResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, response.token);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(EXPIRES_AT_KEY, response.expiresAt);
    localStorage.setItem(USER_KEY, JSON.stringify(response.user));

    this.accessTokenSignal.set(response.token);
    this.refreshTokenSignal.set(response.refreshToken);
    this.expiresAtSignal.set(response.expiresAt);
    this.userSignal.set(response.user);
  }

  clearSession(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(EXPIRES_AT_KEY);
    localStorage.removeItem(USER_KEY);

    this.accessTokenSignal.set(null);
    this.refreshTokenSignal.set(null);
    this.expiresAtSignal.set(null);
    this.userSignal.set(null);
  }

  private readStoredUser(): UserResponse | null {
    const stored = localStorage.getItem(USER_KEY);
    if (!stored) {
      return null;
    }

    try {
      return JSON.parse(stored) as UserResponse;
    } catch {
      return null;
    }
  }
}
