import type { Duser } from './user';

export interface LoginRequest {
  emailOrUsername: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
  walletAddress?: string;
}

export interface AuthResponse {
  token: string;
  user: Duser;
}

export interface ResetPasswordRequest {
  email: string;
}

export interface ConfirmResetPasswordRequest {
  token: string;
  newPassword: string;
}
