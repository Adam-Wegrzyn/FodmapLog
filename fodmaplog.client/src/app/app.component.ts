import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { LanguageService, AppLang } from './services/language.service';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'fodmaplog.client';
  loggedIn = false;
  username = '';
  private sessionSub?: Subscription;

  constructor(
    public language: LanguageService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.sessionSub = this.auth.session$.subscribe((loggedIn) => {
      this.loggedIn = loggedIn;
      const profile = this.auth.readTokenProfile();
      this.username = profile.email || profile.sub || '';
    });
  }

  ngOnDestroy(): void {
    this.sessionSub?.unsubscribe();
  }

  setLang(lang: AppLang): void {
    this.language.setLang(lang);
  }

  logout(): void {
    this.auth.clearSession();
    window.location.href = '/login';
  }
}
