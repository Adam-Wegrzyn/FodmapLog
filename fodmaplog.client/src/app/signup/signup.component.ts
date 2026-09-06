import { Component } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
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

  constructor(private router: Router, private authService: AuthService) {}

  signup(): void {
    this.errorMessage = '';
    this.errorMessages = [];
    this.successMessage = '';

    if (!this.email || !this.password || !this.confirmPassword) {
      this.errorMessage = 'All fields are required.';
      return;
    }

    if (!this.validateEmail(this.email)) {
      this.errorMessage = 'Please enter a valid email address.';
      return;
    }

    if (this.password.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters.';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.isSubmitting = true;
    this.authService.register(this.email, this.password).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.successMessage = 'Registration successful! Redirecting to login...';
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (error: unknown) => {
        this.isSubmitting = false;
        this.errorMessages = this.extractRegisterErrors(error);
        if (this.errorMessages.length === 0) {
          this.errorMessage = 'Registration failed. Please try again.';
        }
      }
    });
  }

  private extractRegisterErrors(error: unknown): string[] {
    if (!(error instanceof HttpErrorResponse)) {
      return ['Registration failed. Please try again.'];
    }

    if (error.status === 0) {
      return [
        'Cannot reach the API (connection refused). Start FodmapLog.Server on http://localhost:5115 and try again.'
      ];
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
