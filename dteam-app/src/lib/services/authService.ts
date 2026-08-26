import { api } from './api';
import type { Duser, LoginRequest, RegisterRequest, AuthResponse } from '../types';

export const authService = {
  async login(payload: LoginRequest): Promise<AuthResponse> {
    const res = await api.post<AuthResponse>('/auth/login', payload);
    api.setToken(res.token);
    return res;
  },

  async register(payload: RegisterRequest): Promise<AuthResponse> {
    const res = await api.post<AuthResponse>('/auth/register', payload);
    api.setToken(res.token);
    return res;
  },

  async logout(): Promise<void> {
    api.setToken(null);
  },

  async getProfile(): Promise<Duser> {
    return await api.get<Duser>('/auth/me');
  },

  async requestPasswordReset(email: string): Promise<{ message: string; debugCode?: string }> {
    return await api.post<{ message: string; debugCode?: string }>('/auth/forgot-password', { email });
  },

  async verifyResetCode(email: string, code: string): Promise<{ message: string; resetToken?: string }> {
    return await api.post<{ message: string; resetToken?: string }>('/auth/verify-reset-code', { email, code });
  },

  async resetPassword(token: string, newPassword: string): Promise<{ message: string }> {
    return await api.post<{ message: string }>('/auth/reset-password', { token, newPassword });
  },
};
