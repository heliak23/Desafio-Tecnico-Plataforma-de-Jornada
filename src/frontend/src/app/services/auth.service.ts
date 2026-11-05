import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginResponse, UserRole } from '../models/types';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'http://localhost:5000/api/auth';
  private readonly tokenKey = 'dnd_auth_token';
  private readonly userKey = 'dnd_auth_user';

  readonly currentUser = signal<LoginResponse | null>(this.getStoredUser());
  readonly isAuthenticated = computed(() => !!this.currentUser());
  readonly isManager = computed(() => this.currentUser()?.role === UserRole.Manager);
  readonly isHR = computed(() => this.currentUser()?.role === UserRole.HR);
  readonly isManagerOrHR = computed(() => this.isManager() || this.isHR());

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap(response => {
        this.saveSession(response);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getDemoAccounts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/demo-accounts`);
  }

  resetDatabase(): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>('http://localhost:5000/api/admin/reset-database', {});
  }

  private saveSession(response: LoginResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.userKey, JSON.stringify(response));
    this.currentUser.set(response);
  }

  private getStoredUser(): LoginResponse | null {
    const raw = localStorage.getItem(this.userKey);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as LoginResponse;
    } catch {
      return null;
    }
  }
}

