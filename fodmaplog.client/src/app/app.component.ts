import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'fodmaplog.client';
  isAuthenticated: boolean = false;
  username: string = '';
  // Add any other properties you need for the navbar/menu
  errorMessage: string = '';
  successMessage: string = '';

  constructor() {}

  isLoggedIn(): boolean {
    // Check if a JWT token exists in localStorage
    return !!localStorage.getItem('token');
  }

  logout(): void {
    localStorage.removeItem('token');
    this.isAuthenticated = false;
    this.username = '';
    // Optionally, redirect to login or home
    window.location.href = '/login';
  }
}