import { api } from './api';
import type {
  SettingsResponse,
  UserProfileSettings,
  UpdateGeneralSettings,
  NotificationPreferences,
  ChangePasswordRequest,
  WalletTransactionItem,
  PagedResult,
  DeleteAccountRequest,
} from '../types/settings';

class SettingsService {
  public async getSettings(): Promise<SettingsResponse> {
    return api.get<SettingsResponse>('/users/settings');
  }

  public async updateGeneralSettings(dto: UpdateGeneralSettings): Promise<UserProfileSettings> {
    return api.put<UserProfileSettings>('/users/settings/general', dto);
  }

  public async updateNotificationPreferences(dto: NotificationPreferences): Promise<NotificationPreferences> {
    return api.put<NotificationPreferences>('/users/settings/notifications', dto);
  }

  public async changePassword(dto: ChangePasswordRequest): Promise<{ message: string }> {
    return api.post<{ message: string }>('/auth/change-password', dto);
  }

  public async getTransactionHistory(page = 1, pageSize = 20): Promise<PagedResult<WalletTransactionItem>> {
    return api.get<PagedResult<WalletTransactionItem>>(`/payment/transactions-history?page=${page}&pageSize=${pageSize}`);
  }

  public async deleteAccount(dto: DeleteAccountRequest): Promise<{ message: string }> {
    return api.delete<{ message: string }>('/users/me', {
      body: JSON.stringify(dto),
    });
  }
}

export const settingsService = new SettingsService();
