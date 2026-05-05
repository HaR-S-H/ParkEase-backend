import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthSessionService } from '@/app/core/services/auth-session.service';

export const authGuard: CanActivateFn = () => {
  const session = inject(AuthSessionService);
  const router = inject(Router);

  if (session.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/auth/login']);
};

export const guestGuard: CanActivateFn = () => {
  const session = inject(AuthSessionService);
  const router = inject(Router);

  if (session.isAuthenticated()) {
    const role = session.user()?.role?.toUpperCase();
    if (role === 'ADMIN') {
      return router.createUrlTree(['/dashboard/admin']);
    } else if (role === 'MANAGER') {
      return router.createUrlTree(['/dashboard/manager']);
    } else {
      return router.createUrlTree(['/dashboard/driver']);
    }
  }

  return true;
};
