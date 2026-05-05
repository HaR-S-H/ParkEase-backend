import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '@/app/core/services/auth.service';

@Component({
  selector: 'app-forgot-password-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './forgot-password-page.component.html',
  styleUrl: './forgot-password-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordPageComponent {
  private authService = inject(AuthService);

  email = signal('');
  errorMessage = signal('');
  successMessage = signal('');
  loading = signal(false);

  onSubmit() {
    this.errorMessage.set('');
    this.successMessage.set('');
    
    if (!this.email()) {
      this.errorMessage.set('Email is required');
      return;
    }

    this.loading.set(true);
    
    this.authService.forgotPassword(this.email()).subscribe({
      next: () => {
        this.successMessage.set('Password reset link has been sent to your email.');
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to send reset link');
      },
      complete: () => this.loading.set(false),
    });
  }
}
