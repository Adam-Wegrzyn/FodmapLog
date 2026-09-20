import { Component } from '@angular/core';
import { LanguageService, AppLang } from './services/language.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'fodmaplog.client';
  isAuthenticated = false;
  username = '';

  constructor(public language: LanguageService) {
    try {
      const token = localStorage.getItem('token');
      if (token) {
        const payload = JSON.parse(atob(token.split('.')[1] || ''));
        this.username = payload?.email || payload?.unique_name || payload?.sub || '';
      }
    } catch {
      this.username = '';
    }
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  setLang(lang: AppLang): void {
    this.language.setLang(lang);
  }

  logout(): void {
    localStorage.removeItem('token');
    this.isAuthenticated = false;
    this.username = '';
    window.location.href = '/login';
  }
}
