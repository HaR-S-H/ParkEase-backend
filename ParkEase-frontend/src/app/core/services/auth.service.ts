import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '@/environments/environment';
import {
  GoogleLoginRequest,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
} from '@/app/core/models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiGatewayBaseUrl}/auth`;

  register(payload: RegisterRequest) {
    const formData = new FormData();
    formData.append('fullName', payload.fullName);
    formData.append('email', payload.email);
    formData.append('password', payload.password);
    formData.append('phone', payload.phone);
    formData.append('role', payload.role);
    
    if (payload.vehiclePlate) {
      formData.append('vehiclePlate', payload.vehiclePlate);
    }
    if (payload.profilePic) {
      formData.append('profilePic', payload.profilePic);
    }

    return this.http.post<RegisterResponse>(`${this.baseUrl}/register`, formData);
  }

  login(payload: LoginRequest) {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, payload);
  }

  googleLogin(payload: GoogleLoginRequest) {
    return this.http.post<LoginResponse>(`${this.baseUrl}/google-login`, payload);
  }

  forgotPassword(email: string) {
    return this.http.post<void>(`${this.baseUrl}/forgot-password`, { email });
  }

  resendVerification(email: string) {
    return this.http.post<void>(`${this.baseUrl}/resend-verification`, { email });
  }

  verifyEmail(token: string) {
    return this.http.get<void>(`${this.baseUrl}/verify-email?token=${token}`);
  }
}
