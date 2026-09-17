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

  constructor() {
    try {
      const token = localStorage.getItem('token');
      if (token) {
        const payload = JSON.parse(atob(token.split('.')[1] || ''));
        this.username = payload?.email || payload?.unique_name || payload?.sub || '';
      }
    } catch {
      this.username = '';
    }
  }

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