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

  async requestPasswordReset(email: string): Promise<void> {
    await api.post('/auth/forgot-password', { email });
  },
};
