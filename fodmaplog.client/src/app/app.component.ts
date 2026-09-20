import { Component } from '@angular/core';
import { LanguageService, AppLang } from './services/language.service';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'fodmaplog.client';
  isAuthenticated = false;
  username = '';

  constructor(
    public language: LanguageService,
    private auth: AuthService
  ) {
    const profile = this.auth.readTokenProfile();
    this.username = profile.email || profile.sub || '';
  }

  isLoggedIn(): boolean {
    return this.auth.isTokenValid();
  }

  setLang(lang: AppLang): void {
    this.language.setLang(lang);
  }

  logout(): void {
    this.auth.clearSession();
    this.isAuthenticated = false;
    this.username = '';
    window.location.href = '/login';
  }
}
