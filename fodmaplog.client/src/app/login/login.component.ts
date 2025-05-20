import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  email: string = '';
  password: string = '';
  rememberMe: boolean = false;
  errorMessage: string = '';
  environment = environment;


  constructor(private http: HttpClient,
    private router: Router,
    private auth: AuthService
  ) { }

  login(): void {
    this.auth.login(this.email, this.password).subscribe(
      response => {
        console.log('Login successful:', response);
        localStorage.setItem('token', response.token);
        this.router.navigate(['/daily-log']);
      },
      error => {
        console.error('Login failed:', error);
        this.errorMessage = 'Invalid email or password.';
      },
    );
}
 loginWithGoogle() {
   this.auth.loginWithGoogle();
  // this.auth.loginWithGoogle().subscribe(
  //   response => {
  //     console.log('Google login successful:', response);
  //     localStorage.setItem('token', response.token);
  //     this.router.navigate(['/daily-log']);
  //   },
  //   error => {
  //     console.error('Google login failed:', error);
  //     this.errorMessage = 'Google login failed.';
  //   }
  // );
}
}


