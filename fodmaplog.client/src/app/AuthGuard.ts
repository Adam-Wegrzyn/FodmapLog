import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(): boolean | UrlTree {
    const token = localStorage.getItem('token');
    if (token) {
      // Optionally, add token validation logic here (e.g., check expiration)
      return true;
    }
    // Redirect to login if not authenticated
    return this.router.createUrlTree(['/login']);
  }
}