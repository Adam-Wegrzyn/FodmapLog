import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {
  email = '';
  errorMessage = '';
  successMessage = '';
  isSubmitting = false;

  constructor(
    private auth: AuthService,
    private translate: TranslateService,
    private router: Router
  ) {}

  submit(): void {
    this.errorMessage = '';
    this.successMessage = '';
    if (!this.email.trim()) {
      this.errorMessage = this.translate.instant('forgot.emailRequired');
      return;
    }

    this.isSubmitting = true;
    this.auth.forgotPassword(this.email.trim()).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.successMessage = this.translate.instant('forgot.success');
      },
      error: (error: unknown) => {
        this.isSubmitting = false;
        if (error instanceof HttpErrorResponse && error.status >= 500) {
          this.errorMessage = this.translate.instant('forgot.unavailable');
          return;
        }
        this.successMessage = this.translate.instant('forgot.success');
      }
    });
  }

  goReset(): void {
    this.router.navigate(['/reset-password'], {
      queryParams: this.email ? { email: this.email.trim() } : undefined
    });
  }
}
