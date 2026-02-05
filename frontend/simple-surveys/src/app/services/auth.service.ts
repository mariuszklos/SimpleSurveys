import { Injectable, signal } from '@angular/core';
import { ApiService } from './api.service';
import { Router } from '@angular/router';
import { catchError, tap } from 'rxjs/operators';
import { of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  isAuthenticated = signal(false);
  isLoading = signal(true);

  constructor(
    private api: ApiService,
    private router: Router
  ) {
    this.checkAuth();
  }

  checkAuth(): void {
    this.isLoading.set(true);
    this.api.checkAdminAuth().pipe(
      tap(response => {
        this.isAuthenticated.set(response.authenticated);
        this.isLoading.set(false);
      }),
      catchError(() => {
        this.isAuthenticated.set(false);
        this.isLoading.set(false);
        return of(null);
      })
    ).subscribe();
  }

  login(password: string): Promise<boolean> {
    return new Promise((resolve) => {
      this.api.adminLogin(password).pipe(
        tap(() => {
          this.isAuthenticated.set(true);
          resolve(true);
        }),
        catchError(() => {
          resolve(false);
          return of(null);
        })
      ).subscribe();
    });
  }

  logout(): void {
    this.api.adminLogout().subscribe(() => {
      this.isAuthenticated.set(false);
      this.router.navigate(['/admin/login']);
    });
  }
}
