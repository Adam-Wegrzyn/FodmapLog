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
loginWithGoogle(): void {
  window.location.href = `${this.url}/external-login?provider=Google&returnUrl=${environment.frontendBaseUrl}/login-callback`;
}
}
