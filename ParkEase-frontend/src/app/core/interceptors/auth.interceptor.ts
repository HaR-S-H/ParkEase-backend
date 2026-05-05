import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthSessionService } from '@/app/core/services/auth-session.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const session = inject(AuthSessionService);
  const token = session.accessToken();

  if (token) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(authReq);
  }

  return next(req);
};
