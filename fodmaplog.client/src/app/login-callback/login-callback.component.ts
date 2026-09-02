import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-login-callback',
  templateUrl: './login-callback.component.html',
  styleUrl: './login-callback.component.css'
})
export class LoginCallbackComponent implements OnInit {
  constructor(private router: Router, private route: ActivatedRoute) {}

  ngOnInit(): void {
    // Prefer fragment (#token=...) — avoids JWT in query string / Referer
    const fragmentToken = this.getHashParam('token');
    const fragmentError = this.getHashParam('error');

    this.route.queryParams.subscribe(params => {
      const token = fragmentToken || params['token'];
      const error = fragmentError || params['error'];

      if (token) {
        localStorage.setItem('token', token);
        // Clear fragment from address bar after storing token
        this.router.navigate(['/daily-log'], { replaceUrl: true });
      } else if (error) {
        console.error('Login error:', error);
        this.router.navigate(['/login'], { replaceUrl: true });
      } else {
        console.error('No token or error in fragment/query parameters');
        this.router.navigate(['/login'], { replaceUrl: true });
      }
    });
  }

  private getHashParam(key: string): string | null {
    const hash = window.location.hash?.startsWith('#')
      ? window.location.hash.substring(1)
      : window.location.hash;
    if (!hash) {
      return null;
    }
    const params = new URLSearchParams(hash);
    return params.get(key);
  }
}
