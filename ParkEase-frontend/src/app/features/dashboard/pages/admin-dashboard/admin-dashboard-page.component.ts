import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthSessionService } from '@/app/core/services/auth-session.service';

@Component({
  selector: 'app-admin-dashboard-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-8 text-center" style="padding: 3rem; color: white;">
      <h1 style="font-size: 2rem; margin-bottom: 1rem;">Admin Dashboard</h1>
      <p>Welcome, {{ user()?.fullName }}!</p>
      <button (click)="logout()" style="margin-top: 2rem; padding: 0.5rem 1rem; background: #ef4444; color: white; border: none; border-radius: 8px; cursor: pointer;">
        Logout
      </button>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboardPageComponent {
  private session = inject(AuthSessionService);
  private router = inject(Router);

  user = this.session.user;

  logout() {
    this.session.clearSession();
    this.router.navigateByUrl('/auth/login');
  }
}
