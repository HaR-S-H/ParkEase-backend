import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '@/app/core/services/auth.service';
import { AuthSessionService } from '@/app/core/services/auth-session.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent {
  private authService = inject(AuthService);
  private authSession = inject(AuthSessionService);
  private router = inject(Router);

  email = signal('');
  password = signal('');
  errorMessage = signal('');
  loading = signal(false);

  onSubmit() {
    this.errorMessage.set('');
    if (!this.email() || !this.password()) {
      this.errorMessage.set('Email and password are required');
      return;
    }
    this.loading.set(true);
    this.authService.login({ email: this.email(), password: this.password() }).subscribe({
      next: (res) => {
        this.authSession.setSession(res);
        const role = res.user.role.toUpperCase();
        if (role === 'ADMIN') this.router.navigateByUrl('/dashboard/admin');
        else if (role === 'MANAGER') this.router.navigateByUrl('/dashboard/manager');
        else this.router.navigateByUrl('/dashboard/driver');
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Login failed');
      },
      complete: () => this.loading.set(false),
    });
  }

  onGoogleLogin() {
    // In a real implementation we would open Google's SDK.
    // For now, this is a placeholder stub matching backend requirements
    alert('Google login SDK would open here.');
  }
}
