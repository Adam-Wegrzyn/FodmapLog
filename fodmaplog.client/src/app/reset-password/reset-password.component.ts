import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent implements OnInit {
  email = '';
  resetCode = '';
  newPassword = '';
  confirmPassword = '';
  errorMessage = '';
  successMessage = '';
  isSubmitting = false;

  constructor(
    private auth: AuthService,
    private translate: TranslateService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const q = this.route.snapshot.queryParamMap;
    this.email = q.get('email') || '';
    this.resetCode = q.get('code') || q.get('resetCode') || '';
  }

  submit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.email.trim() || !this.resetCode.trim() || !this.newPassword) {
      this.errorMessage = this.translate.instant('reset.allRequired');
      return;
    }
    if (this.newPassword.length < 6) {
      this.errorMessage = this.translate.instant('reset.passwordTooShort');
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = this.translate.instant('reset.passwordMismatch');
      return;
    }

    this.isSubmitting = true;
    this.auth.resetPassword(this.email.trim(), this.resetCode.trim(), this.newPassword).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.successMessage = this.translate.instant('reset.success');
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: () => {
        this.isSubmitting = false;
        this.errorMessage = this.translate.instant('reset.failed');
      }
    });
  }
}
