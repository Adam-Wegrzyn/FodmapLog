import { Component } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent {
  email = '';
  password = '';
  confirmPassword = '';
  errorMessage = '';
  errorMessages: string[] = [];
  successMessage = '';
  isSubmitting = false;

  constructor(
    private router: Router,
    private authService: AuthService,
    private translate: TranslateService
  ) {}

  signup(): void {
    this.errorMessage = '';
    this.errorMessages = [];
    this.successMessage = '';

    if (!this.email || !this.password || !this.confirmPassword) {
      this.errorMessage = this.translate.instant('signup.allRequired');
      return;
    }

    if (!this.validateEmail(this.email)) {
      this.errorMessage = this.translate.instant('signup.invalidEmail');
      return;
    }

    if (this.password.length < 6) {
      this.errorMessage = this.translate.instant('signup.passwordTooShort');
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = this.translate.instant('signup.passwordMismatch');
      return;
    }

    this.isSubmitting = true;
    this.authService.register(this.email, this.password).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.successMessage = this.translate.instant('signup.success');
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (error: unknown) => {
        this.isSubmitting = false;
        this.errorMessages = this.extractRegisterErrors(error);
        if (this.errorMessages.length === 0) {
          this.errorMessage = this.translate.instant('signup.failed');
        }
      }
    });
  }

  private extractRegisterErrors(error: unknown): string[] {
    if (!(error instanceof HttpErrorResponse)) {
      return [this.translate.instant('signup.failed')];
    }

    if (error.status === 0) {
      return [this.translate.instant('signup.apiUnreachable')];
    }

    const body = error.error;
    const messages: string[] = [];

    if (body?.errors && typeof body.errors === 'object') {
      for (const key of Object.keys(body.errors)) {
        const value = body.errors[key];
        if (Array.isArray(value)) {
          messages.push(...value.map(String));
        } else if (value != null) {
          messages.push(String(value));
        }
      }
    }

    if (messages.length === 0 && typeof body === 'string' && body.trim()) {
      messages.push(body);
    }

    if (messages.length === 0 && body?.title) {
      messages.push(String(body.title));
    }

    if (messages.length === 0 && error.message) {
      messages.push(error.message);
    }

    return messages;
  }

  private validateEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }
}
