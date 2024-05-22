// auth.guard.ts
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard  {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): Observable<boolean> | Promise<boolean> | boolean {
    return this.authService.isAuthenticated.pipe(
      map(isAuth => {
        if (!isAuth) {
          this.router.navigate(['/login']);
        }
        return isAuth;
      })
    );
  }

}
