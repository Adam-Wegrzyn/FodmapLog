import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LanguageService, AppLang } from '../services/language.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrl: './account.component.css'
})
export class AccountComponent implements OnInit {
  email = '';

  constructor(
    public language: LanguageService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const profile = this.auth.readTokenProfile();
    this.email = profile.email || profile.sub || '';
  }

  setLang(lang: AppLang): void {
    this.language.setLang(lang);
  }

  logout(): void {
    this.auth.clearSession();
    this.router.navigate(['/login']);
  }
}
