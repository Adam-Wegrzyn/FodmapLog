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
      error: () => {
        // Always show success-style copy to avoid account enumeration.
        this.isSubmitting = false;
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
