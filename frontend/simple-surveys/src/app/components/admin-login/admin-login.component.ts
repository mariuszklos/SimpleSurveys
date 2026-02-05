import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-login.component.html',
  styleUrl: './admin-login.component.scss'
})
export class AdminLoginComponent {
  password = '';
  error = signal<string | null>(null);
  loading = signal(false);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/admin']);
    }
  }

  async onSubmit(): Promise<void> {
    if (!this.password) {
      this.error.set('Password is required');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const success = await this.authService.login(this.password);

    if (success) {
      this.router.navigate(['/admin']);
    } else {
      this.error.set('Invalid password');
      this.loading.set(false);
    }
  }
}
