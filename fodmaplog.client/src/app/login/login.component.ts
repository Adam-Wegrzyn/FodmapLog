import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email = '';
  password = '';
  rememberMe = false;
  errorMessage = '';
  environment = environment;

  constructor(
    private http: HttpClient,
    private router: Router,
    private auth: AuthService,
    private translate: TranslateService
  ) { }

  login(): void {
    this.auth.login(this.email, this.password).subscribe(
      response => {
        localStorage.setItem('token', response.token);
        this.router.navigate(['/daily-log']);
      },
      error => {
        console.error('Login failed:', error);
        if (error?.status === 0) {
          this.errorMessage = this.translate.instant('login.apiUnreachable');
        } else {
          this.errorMessage = this.translate.instant('login.invalidCredentials');
        }
      },
    );
  }

  loginWithGoogle(): void {
    this.auth.loginWithGoogle();
  }
}
