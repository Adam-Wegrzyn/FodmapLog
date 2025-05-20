import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent {
  email: string = '';
  password: string = '';
  confirmPassword: string = '';
  errorMessage: string = '';
  errorMessages: string[] = [];
  successMessage: string = '';


  constructor(private http: HttpClient, private router: Router, private authService: AuthService) {}

  signup(): void {
    this.errorMessage = '';
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

    this.authService.register(this.email, this.password).subscribe(
      response => {
        console.log('Registration successful:', response);
        this.successMessage = 'Registration successful! Redirecting to login...';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error => {
        // this.errorMessage = error.errors || 'Registration failed. Please try again.';
        console.log(error.error.errors);
        for (const e in error.error.errors) {
          this.errorMessages.push(error.error.errors[e]);
        }
      }
    );
  }

  private validateEmail(email: string): boolean {
    // Simple email regex for demonstration
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }
}