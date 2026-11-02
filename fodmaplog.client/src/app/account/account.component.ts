import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { LanguageService, AppLang } from '../services/language.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrl: './account.component.css'
})
export class AccountComponent implements OnInit {
  email = '';
  showDelete = false;
  deletePassword = '';
  deleteConfirm = '';
  deleteError: string | null = null;
  isDeleting = false;

  constructor(
    public language: LanguageService,
    private auth: AuthService,
    private router: Router,
    private translate: TranslateService
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

  openDelete(): void {
    this.showDelete = true;
    this.deletePassword = '';
    this.deleteConfirm = '';
    this.deleteError = null;
  }

  cancelDelete(): void {
    if (this.isDeleting) {
      return;
    }
    this.showDelete = false;
    this.deleteError = null;
  }

  confirmDelete(): void {
    this.deleteError = null;
    if (!this.deletePassword && this.deleteConfirm !== 'DELETE') {
      this.deleteError = this.translate.instant('account.deleteNeedPassword');
      return;
    }

    this.isDeleting = true;
    this.auth.deleteAccount(this.deletePassword, this.deleteConfirm || undefined).subscribe({
      next: () => {
        this.isDeleting = false;
        this.auth.clearSession();
        this.router.navigate(['/login']);
      },
      error: (error: unknown) => {
        this.isDeleting = false;
        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.deleteError = this.translate.instant('account.deleteBadPassword');
          return;
        }
        this.deleteError = this.translate.instant('account.deleteFail');
      }
    });
  }
}
