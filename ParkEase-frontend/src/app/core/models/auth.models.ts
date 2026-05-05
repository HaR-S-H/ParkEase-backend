export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  phone: string;
  role: 'DRIVER' | 'MANAGER' | 'ADMIN';
  vehiclePlate?: string;
  profilePic?: File;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface GoogleLoginRequest {
  idToken: string;
  role?: 'DRIVER' | 'MANAGER' | 'ADMIN';
  phone?: string;
  vehiclePlate?: string;
}

export interface RefreshTokenRequest {
  refreshToken?: string;
}

export interface ValidateTokenRequest {
  token: string;
}

export interface UserResponse {
  userId: string;
  fullName: string;
  email: string;
  phone: string;
  role: string;
  vehiclePlate: string;
  emailVerified: boolean;
  isActive: boolean;
  createdAt: string;
  profilePicUrl?: string | null;
}

export interface RegisterResponse {
  message: string;
  user: UserResponse;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: UserResponse;
}

export interface ValidateTokenResponse {
  isValid: boolean;
}
