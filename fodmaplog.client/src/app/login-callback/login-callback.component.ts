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
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      const error = params['error'];

      if (token) {
        localStorage.setItem('token', token);
        this.router.navigate(['/daily-log']);
      } else if (error) {
        console.error('Login error:', error);
        this.router.navigate(['/login']);
      } else {
        console.error('No token or error in query parameters');
        this.router.navigate(['/login']);
      }
    });
  }
}





