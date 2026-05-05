import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '@/app/core/services/auth.service';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register-page.component.html',
  styleUrl: './register-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterPageComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  fullName = signal('');
  email = signal('');
  password = signal('');
  phone = signal('');
  role = signal<'DRIVER' | 'MANAGER'>('DRIVER');
  vehiclePlate = signal('');
  
  errorMessage = signal('');
  successMessage = signal('');
  loading = signal(false);

  onSubmit() {
    this.errorMessage.set('');
    this.successMessage.set('');
    
    if (!this.fullName() || !this.email() || !this.password() || !this.phone()) {
      this.errorMessage.set('Please fill out all required fields');
      return;
    }

    if (this.role() === 'DRIVER' && !this.vehiclePlate()) {
      this.errorMessage.set('Vehicle plate is required for drivers');
      return;
    }

    this.loading.set(true);
    
    this.authService.register({
      fullName: this.fullName(),
      email: this.email(),
      password: this.password(),
      phone: this.phone(),
      role: this.role(),
      vehiclePlate: this.role() === 'DRIVER' ? this.vehiclePlate() : undefined,
    }).subscribe({
      next: (res) => {
        this.successMessage.set('Registration successful! Please login.');
        setTimeout(() => this.router.navigateByUrl('/auth/login'), 2000);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Registration failed');
      },
      complete: () => this.loading.set(false),
    });
  }
}
