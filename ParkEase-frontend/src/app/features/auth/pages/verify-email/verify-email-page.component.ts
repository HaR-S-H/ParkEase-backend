import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '@/app/core/services/auth.service';

@Component({
  selector: 'app-verify-email-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './verify-email-page.component.html',
  styleUrl: './verify-email-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerifyEmailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  status = signal<'verifying' | 'success' | 'error'>('verifying');
  errorMessage = signal('');

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      if (!token) {
        this.status.set('error');
        this.errorMessage.set('Missing verification token');
        return;
      }
      
      this.authService.verifyEmail(token).subscribe({
        next: () => {
          this.status.set('success');
        },
        error: (err) => {
          this.status.set('error');
          this.errorMessage.set(err.error?.message || 'Email verification failed. The link might be expired.');
        }
      });
    });
  }
}
