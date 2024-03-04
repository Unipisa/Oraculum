import { Component, OnInit } from '@angular/core';
import { AuthService } from 'src/app/services/auth.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { LoginResponse, OidcSecurityService } from 'angular-auth-oidc-client';
import language from 'src/app/config/language';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  username: string = '';
  password: string = '';
  language = language;

  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private router: Router,
    private oidcSecurityService: OidcSecurityService
  ) {}

  ngOnInit() {
    this.oidcSecurityService
      .checkAuth()
      .subscribe((loginResponse: LoginResponse) => {
        const { isAuthenticated, userData, accessToken, idToken, configId } =
          loginResponse;
      });
  }

  login() {
    this.authService.login().subscribe({
      next: (data) => {
        // Success case is handled within the service
      },
      error: (err) => {
        // Error case is handled within the service, but you can still do something here if needed
      },
    });
  }
}
