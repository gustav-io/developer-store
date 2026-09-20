import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from './models';

interface AuthResponse {
  token: string;
  email: string;
  name: string;
  role: string;
}

const TOKEN_KEY = 'developer-store.token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenSignal = signal<string | null>(readToken());

  readonly token = this.tokenSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.tokenSignal() !== null);

  async login(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<ApiResponse<AuthResponse>>('/api/auth', { email, password })
    );
    this.tokenSignal.set(response.data.token);
    try { localStorage.setItem(TOKEN_KEY, response.data.token); } catch { /* storage unavailable */ }
  }

  logout(): void {
    this.tokenSignal.set(null);
    try { localStorage.removeItem(TOKEN_KEY); } catch { /* storage unavailable */ }
  }
}

function readToken(): string | null {
  try { return localStorage.getItem(TOKEN_KEY); } catch { return null; }
}
