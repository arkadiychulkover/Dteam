import { writable, get } from 'svelte/store';
import { settingsService } from '../services/settingsService';
import { mediaService } from '../services/mediaService';
import { authStore } from './authStore';
import { uiStore } from './uiStore';
import type {
  UserProfileSettings,
  NotificationPreferences,
  WalletSummary,
  ChangePasswordRequest,
  WalletTransactionItem,
  DeleteAccountRequest,
  SettingsTabId,
} from '../types/settings';

interface SettingsServerState {
  profile: UserProfileSettings | null;
  preferences: NotificationPreferences | null;
  walletSummary: WalletSummary | null;
}

export interface GeneralDraftState {
  username: string;
  email: string;
  bio: string;
  avatarUrl: string;
  bannerUrl: string;
  preferredLanguage: string;
  isDirty: boolean;
}

interface SettingsUiState {
  activeTab: SettingsTabId;
  isLoading: boolean;
  isSaving: boolean;
  isUploadingAvatar: boolean;
  isUploadingBanner: boolean;
  error: string | null;
  searchQuery: string;
  transactions: WalletTransactionItem[];
  txTotalCount: number;
  txPage: number;
  txPageSize: number;
  txTotalPages: number;
  txLoading: boolean;
}

function createSettingsStore() {
  const serverState = writable<SettingsServerState>({
    profile: null,
    preferences: null,
    walletSummary: null,
  });

  const draftState = writable<GeneralDraftState>({
    username: '',
    email: '',
    bio: '',
    avatarUrl: '',
    bannerUrl: '',
    preferredLanguage: 'uk',
    isDirty: false,
  });

  const uiState = writable<SettingsUiState>({
    activeTab: 'general',
    isLoading: false,
    isSaving: false,
    isUploadingAvatar: false,
    isUploadingBanner: false,
    error: null,
    searchQuery: '',
    transactions: [],
    txTotalCount: 0,
    txPage: 1,
    txPageSize: 20,
    txTotalPages: 1,
    txLoading: false,
  });

  async function loadSettings() {
    uiState.update((s) => ({ ...s, isLoading: true, error: null }));
    try {
      const res = await settingsService.getSettings();
      serverState.set({
        profile: res.profile,
        preferences: res.preferences,
        walletSummary: res.walletSummary,
      });

      draftState.set({
        username: res.profile.username || '',
        email: res.profile.email || '',
        bio: res.profile.bio || '',
        avatarUrl: res.profile.avatarUrl || '',
        bannerUrl: res.profile.bannerUrl || '',
        preferredLanguage: res.profile.preferredLanguage || 'uk',
        isDirty: false,
      });

      uiState.update((s) => ({ ...s, isLoading: false }));

      loadTransactions(1);
    } catch (err: any) {
      const msg = err.message || 'Помилка завантаження налаштувань';
      uiState.update((s) => ({ ...s, isLoading: false, error: msg }));
      uiStore.addToast({
        title: 'Помилка',
        message: msg,
        type: 'error',
      });
    }
  }

  function setTab(tab: SettingsTabId) {
    uiState.update((s) => ({ ...s, activeTab: tab }));
  }

  function setSearchQuery(q: string) {
    uiState.update((s) => ({ ...s, searchQuery: q }));
  }

  function updateGeneralDraft(partial: Partial<GeneralDraftState>) {
    draftState.update((s) => ({
      ...s,
      ...partial,
      isDirty: true,
    }));
  }

  function resetGeneralDraft() {
    const server = get(serverState);
    if (server.profile) {
      draftState.set({
        username: server.profile.username || '',
        email: server.profile.email || '',
        bio: server.profile.bio || '',
        avatarUrl: server.profile.avatarUrl || '',
        bannerUrl: server.profile.bannerUrl || '',
        preferredLanguage: server.profile.preferredLanguage || 'uk',
        isDirty: false,
      });
    }
  }

  async function saveGeneralSettings(): Promise<boolean> {
    const draft = get(draftState);
    uiState.update((s) => ({ ...s, isSaving: true, error: null }));

    try {
      const updatedProfile = await settingsService.updateGeneralSettings({
        username: draft.username.trim(),
        email: draft.email.trim(),
        bio: draft.bio.trim(),
        avatarUrl: draft.avatarUrl,
        bannerUrl: draft.bannerUrl,
        preferredLanguage: draft.preferredLanguage,
      });

      serverState.update((s) => ({ ...s, profile: updatedProfile }));
      draftState.update((d) => ({ ...d, isDirty: false }));

      authStore.patchUser({
        username: updatedProfile.username,
        email: updatedProfile.email,
        bio: updatedProfile.bio,
        avatarUrl: updatedProfile.avatarUrl,
        bannerUrl: updatedProfile.bannerUrl,
      });

      uiState.update((s) => ({ ...s, isSaving: false }));
      uiStore.addToast({
        title: 'Успішно',
        message: 'Налаштування профілю збережено!',
        type: 'success',
      });
      return true;
    } catch (err: any) {
      const msg = err.message || 'Не вдалося зберегти зміни';
      uiState.update((s) => ({ ...s, isSaving: false, error: msg }));
      uiStore.addToast({
        title: 'Помилка збереження',
        message: msg,
        type: 'error',
      });
      return false;
    }
  }

  async function uploadAvatar(file: File): Promise<string | null> {
    uiState.update((s) => ({ ...s, isUploadingAvatar: true }));
    try {
      const res = await mediaService.upload(file);
      updateGeneralDraft({ avatarUrl: res.url });
      uiState.update((s) => ({ ...s, isUploadingAvatar: false }));
      uiStore.addToast({
        title: 'Аватар завантажено',
        message: 'Зображення готове. Натисніть "Зберегти" для підтвердження.',
        type: 'info',
      });
      return res.url;
    } catch (err: any) {
      uiState.update((s) => ({ ...s, isUploadingAvatar: false }));
      uiStore.addToast({
        title: 'Помилка завантаження',
        message: err.message || 'Не вдалося завантажити аватар',
        type: 'error',
      });
      return null;
    }
  }

  async function uploadBanner(file: File): Promise<string | null> {
    uiState.update((s) => ({ ...s, isUploadingBanner: true }));
    try {
      const res = await mediaService.upload(file);
      updateGeneralDraft({ bannerUrl: res.url });
      uiState.update((s) => ({ ...s, isUploadingBanner: false }));
      uiStore.addToast({
        title: 'Баннер завантажено',
        message: 'Зображення готове. Натисніть "Зберегти" для підтвердження.',
        type: 'info',
      });
      return res.url;
    } catch (err: any) {
      uiState.update((s) => ({ ...s, isUploadingBanner: false }));
      uiStore.addToast({
        title: 'Помилка завантаження',
        message: err.message || 'Не вдалося завантажити баннер',
        type: 'error',
      });
      return null;
    }
  }

  async function saveNotificationPreferences(prefs: NotificationPreferences): Promise<boolean> {
    try {
      serverState.update((s) => ({ ...s, preferences: prefs }));
      const updated = await settingsService.updateNotificationPreferences(prefs);
      serverState.update((s) => ({ ...s, preferences: updated }));
      return true;
    } catch (err: any) {
      uiStore.addToast({
        title: 'Помилка оновлення',
        message: err.message || 'Не вдалося оновити сповіщення',
        type: 'error',
      });
      return false;
    }
  }

  async function changePassword(dto: ChangePasswordRequest): Promise<boolean> {
    uiState.update((s) => ({ ...s, isSaving: true, error: null }));
    try {
      await settingsService.changePassword(dto);
      uiState.update((s) => ({ ...s, isSaving: false }));
      uiStore.addToast({
        title: 'Пароль змінено',
        message: 'Ваш пароль успішно оновлено! Усі сесії синхронізовано.',
        type: 'success',
      });
      return true;
    } catch (err: any) {
      const msg = err.message || 'Помилка зміни паролю';
      uiState.update((s) => ({ ...s, isSaving: false, error: msg }));
      uiStore.addToast({
        title: 'Помилка',
        message: msg,
        type: 'error',
      });
      return false;
    }
  }

  async function loadTransactions(page = 1, pageSize = 20) {
    uiState.update((s) => ({ ...s, txLoading: true }));
    try {
      const res = await settingsService.getTransactionHistory(page, pageSize);
      uiState.update((s) => ({
        ...s,
        transactions: res.items,
        txTotalCount: res.totalCount,
        txPage: res.page,
        txPageSize: res.pageSize,
        txTotalPages: res.totalPages,
        txLoading: false,
      }));
    } catch (err: any) {
      uiState.update((s) => ({ ...s, txLoading: false }));
      console.warn('[settingsStore] Could not load transactions:', err);
    }
  }

  async function deleteAccount(dto: DeleteAccountRequest): Promise<boolean> {
    uiState.update((s) => ({ ...s, isSaving: true, error: null }));
    try {
      await settingsService.deleteAccount(dto);
      uiState.update((s) => ({ ...s, isSaving: false }));
      uiStore.addToast({
        title: 'Акаунт видалено',
        message: 'Ваш акаунт було успішно видалено. До побачення!',
        type: 'info',
      });
      await authStore.logout();
      uiStore.setTab('store');
      return true;
    } catch (err: any) {
      const msg = err.message || 'Помилка видалення акаунта';
      uiState.update((s) => ({ ...s, isSaving: false, error: msg }));
      uiStore.addToast({
        title: 'Помилка',
        message: msg,
        type: 'error',
      });
      return false;
    }
  }

  return {
    serverState,
    draftState,
    uiState,
    loadSettings,
    setTab,
    setSearchQuery,
    updateGeneralDraft,
    resetGeneralDraft,
    saveGeneralSettings,
    uploadAvatar,
    uploadBanner,
    saveNotificationPreferences,
    changePassword,
    loadTransactions,
    deleteAccount,
  };
}

export const settingsStore = createSettingsStore();
