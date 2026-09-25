import { api } from './api';
import { tokenStorage } from './tokenStorage';
import type { AuthResponseDto, UserDto } from '../types';

export const authService = {
  async login(emailOrUsername: string, password: string): Promise<AuthResponseDto> {
    const data = await api.post<AuthResponseDto>('/auth/login', {
      emailOrUsername: emailOrUsername.trim(),
      password,
    });
    if (data.accessToken && data.refreshToken) {
      await tokenStorage.setTokens(data.accessToken, data.refreshToken);
    }
    return data;
  },


  async forgotPassword(email: string): Promise<{ message: string; debugCode?: string; userEmail?: string }> {
    return api.post<{ message: string; debugCode?: string; userEmail?: string }>('/auth/forgot-password', {
      email: email.trim(),
    });
  },

  async verifyResetCode(email: string, code: string): Promise<{ message: string; resetToken: string }> {
    return api.post<{ message: string; resetToken: string }>('/auth/verify-reset-code', {
      email: email.trim(),
      code: code.trim(),
    });
  },

  async resetPassword(token: string, newPassword: string): Promise<{ message: string }> {
    return api.post<{ message: string }>('/auth/reset-password', {
      token: token.trim(),
      newPassword,
    });
  },

  async getMe(): Promise<UserDto> {
    return api.get<UserDto>('/auth/me');
  },

  async logout(): Promise<void> {
    try {
      await api.post('/auth/logout');
    } catch {
    } finally {
      await tokenStorage.clearTokens();
    }
  },
};
