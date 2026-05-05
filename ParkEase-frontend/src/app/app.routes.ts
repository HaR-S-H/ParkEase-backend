import { Routes } from '@angular/router';
import { LoginPageComponent } from '@/app/features/auth/pages/login/login-page.component';
import { RegisterPageComponent } from '@/app/features/auth/pages/register/register-page.component';
import { ForgotPasswordPageComponent } from '@/app/features/auth/pages/forgot-password/forgot-password-page.component';
import { ResendVerificationPageComponent } from '@/app/features/auth/pages/resend-verification/resend-verification-page.component';
import { VerifyEmailPageComponent } from '@/app/features/auth/pages/verify-email/verify-email-page.component';
import { DriverDashboardPageComponent } from '@/app/features/dashboard/pages/driver-dashboard/driver-dashboard-page.component';
import { MyBookingsPageComponent } from '@/app/features/dashboard/pages/my-bookings/my-bookings-page.component';
import { MyVehiclesPageComponent } from '@/app/features/dashboard/pages/my-vehicles/my-vehicles-page.component';
import { BookSpotPageComponent } from '@/app/features/dashboard/pages/book-spot/book-spot-page.component';
import { ManagerDashboardPageComponent } from '@/app/features/dashboard/pages/manager-dashboard/manager-dashboard-page.component';
import { AdminDashboardPageComponent } from '@/app/features/dashboard/pages/admin-dashboard/admin-dashboard-page.component';
import { AddLotPageComponent } from '@/app/features/dashboard/pages/add-lot/add-lot-page.component';
import { authGuard, guestGuard } from '@/app/core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  { path: 'auth/login', component: LoginPageComponent, canActivate: [guestGuard] },
  { path: 'auth/register', component: RegisterPageComponent, canActivate: [guestGuard] },
  { path: 'auth/forgot-password', component: ForgotPasswordPageComponent, canActivate: [guestGuard] },
  { path: 'auth/resend-verification', component: ResendVerificationPageComponent, canActivate: [guestGuard] },
  { path: 'auth/verify-email', component: VerifyEmailPageComponent },
  
  { path: 'dashboard/driver', component: DriverDashboardPageComponent, canActivate: [authGuard] },
  { path: 'dashboard/driver/bookings', component: MyBookingsPageComponent, canActivate: [authGuard] },
  { path: 'dashboard/driver/vehicles', component: MyVehiclesPageComponent, canActivate: [authGuard] },
  { path: 'dashboard/driver/book/:lotId', component: BookSpotPageComponent, canActivate: [authGuard] },
  { path: 'dashboard/manager', component: ManagerDashboardPageComponent, canActivate: [authGuard] },
  { path: 'dashboard/manager/add-lot', component: AddLotPageComponent, canActivate: [authGuard] },
  { path: 'dashboard/admin', component: AdminDashboardPageComponent, canActivate: [authGuard] }
];
