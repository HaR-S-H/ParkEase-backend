import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '@/app/core/services/auth.service';

@Component({
  selector: 'app-resend-verification-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './resend-verification-page.component.html',
  styleUrl: './resend-verification-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResendVerificationPageComponent {
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
    
    this.authService.resendVerification(this.email()).subscribe({
      next: () => {
        this.successMessage.set('Verification link has been resent to your email.');
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to resend verification link');
      },
      complete: () => this.loading.set(false),
    });
  }
}
