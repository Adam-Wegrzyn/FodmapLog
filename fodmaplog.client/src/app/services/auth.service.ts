import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private http: HttpClient) { }
  url: string = environment.authApiUrl
  urlIdentity: string = environment.authIdentityApiUrl

  login(email: string, password: string): Observable<any> {
    return this.http.post(`${this.url}/login`, { email, password });
  }

  register(email: string, password: string): Observable<any> {
    return this.http.post(`${this.urlIdentity}/register`, { email, password });
  }

  forgotPassword(email: string): Observable<unknown> {
    return this.http.post(`${this.urlIdentity}/forgotPassword`, { email });
  }

  resetPassword(email: string, resetCode: string, newPassword: string): Observable<unknown> {
    return this.http.post(`${this.urlIdentity}/resetPassword`, {
      email,
      resetCode,
      newPassword
    });
  }

  loginWithGoogle(): void {
    window.location.href = `${this.url}/external-login?provider=Google&returnUrl=${environment.frontendBaseUrl}/login-callback`;
  }

  /** Returns email/sub from JWT, or empty fields if missing/invalid. */
  readTokenProfile(): { email: string; sub: string; exp: number | null } {
    try {
      const token = localStorage.getItem('token');
      if (!token) {
        return { email: '', sub: '', exp: null };
      }
      const payload = JSON.parse(atob(token.split('.')[1] || ''));
      return {
        email: payload?.email || payload?.unique_name || '',
        sub: payload?.sub || '',
        exp: typeof payload?.exp === 'number' ? payload.exp : null
      };
    } catch {
      return { email: '', sub: '', exp: null };
    }
  }

  isTokenValid(): boolean {
    const profile = this.readTokenProfile();
    if (!localStorage.getItem('token')) {
      return false;
    }
    if (profile.exp == null) {
      return true;
    }
    return profile.exp * 1000 > Date.now();
  }

  clearSession(): void {
    localStorage.removeItem('token');
  }
}
