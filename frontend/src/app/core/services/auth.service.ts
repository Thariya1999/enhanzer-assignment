import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { AuthSession, LoginRequest } from '../models/auth.models';

const STORAGE_KEY = 'enhanzer.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _session = signal<AuthSession | null>(this.restore());
  readonly session = this._session.asReadonly();

  get token(): string | null {
    return this._session()?.token ?? null;
  }

  isLoggedIn(): boolean {
    const s = this._session();
    return !!s && new Date(s.expiresAt).getTime() > Date.now();
  }

  login(request: LoginRequest): Observable<AuthSession> {
    return this.http.post<AuthSession>('/api/auth/login', request).pipe(
      tap((session) => {
        // sessionStorage: cleared when the tab closes, never sent automatically (unlike cookies)
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
        this._session.set(session);
      }),
    );
  }

  logout(redirect = true): void {
    sessionStorage.removeItem(STORAGE_KEY);
    this._session.set(null);
    if (redirect) void this.router.navigate(['/login']);
  }

  private restore(): AuthSession | null {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const s = JSON.parse(raw) as AuthSession;
      return new Date(s.expiresAt).getTime() > Date.now() ? s : null;
    } catch {
      return null;
    }
  }
}
