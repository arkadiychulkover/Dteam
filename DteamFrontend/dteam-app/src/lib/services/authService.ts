import { api } from './api';
import type { Duser, LoginRequest, RegisterRequest, AuthResponse } from '../types';
import { onlineHubService } from './onlineHubService';

export const authService = {
  async login(payload: LoginRequest): Promise<AuthResponse> {
    const res = await api.post<AuthResponse>('/auth/login', payload);
    api.setTokens(res.accessToken, res.refreshToken);
    
    // Перепідключаємось до хабу з новим токеном, щоб з'єднання стало іменованим
    // (інакше SignalR лишиться на старому анонімному conn і рахуватиме юзера як гостя)
    await onlineHubService.restartConnection();
    
    return res;
  },

  async register(payload: RegisterRequest): Promise<AuthResponse> {
    const res = await api.post<AuthResponse>('/auth/register', payload);
    api.setTokens(res.accessToken, res.refreshToken);
    
    // Перепідключаємось до хабу з новим токеном
    await onlineHubService.restartConnection();
    
    return res;
  },

  async logout(): Promise<void> {
    try {
      await api.post('/auth/logout');
    } catch {
      // Игнорируем ошибки сетевого logout запроса
    } finally {
      api.setTokens(null, null);
      
      // Перепідключаємось анонімно, щоб не лишити "привида" від старого юзера
      await onlineHubService.restartConnection();
    }
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