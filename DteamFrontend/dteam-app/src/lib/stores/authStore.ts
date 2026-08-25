import { writable, derived } from 'svelte/store';
import type { Duser, UserStatus } from '../types';
import { authService } from '../services/authService';
import { api } from '../services/api';

function getStoredUser(): Duser | null {
  if (typeof window !== 'undefined' && window.localStorage) {
    try {
      const data = localStorage.getItem('dteam_user');
      return data ? JSON.parse(data) : null;
    } catch (e) {
      console.warn('[authStore] Failed to parse stored user:', e);
    }
  }
  return null;
}

function saveStoredUser(user: Duser | null) {
  if (typeof window !== 'undefined' && window.localStorage) {
    try {
      if (user) {
        localStorage.setItem('dteam_user', JSON.stringify(user));
      } else {
        localStorage.removeItem('dteam_user');
      }
    } catch (e) {
      console.warn('[authStore] Failed to save stored user:', e);
    }
  }
}

function createAuthStore() {
  const initialToken = api.getToken();
  const initialUser = initialToken ? getStoredUser() : null;

  const { subscribe, set, update } = writable<{
    user: Duser | null;
    token: string | null;
    resetEmail: string | null;
    resetToken: string | null;
    isLoading: boolean;
    error: string | null;
  }>({
    user: initialUser,
    token: initialToken,
    resetEmail: null,
    resetToken: null,
    isLoading: false,
    error: null,
  });

  // If token exists, validate session in background with backend
  if (initialToken) {
    authService.getProfile()
      .then((freshUser) => {
        saveStoredUser(freshUser);
        update((s) => ({ ...s, user: freshUser, token: initialToken, isLoading: false }));
      })
      .catch((err: any) => {
        // Only wipe credentials if explicitly 401 Unauthorized (token invalid / expired)
        if (err.status === 401) {
          api.setToken(null);
          saveStoredUser(null);
          update((s) => ({ ...s, user: null, token: null, isLoading: false }));
        } else {
          console.warn('[authStore] Background profile check failed, keeping cached session:', err.message);
        }
      });
  }

  return {
    subscribe,
    setUser: (user: Duser | null) => {
      saveStoredUser(user);
      update((s) => ({ ...s, user }));
    },
    setStatus: (status: UserStatus) => {
      update((s) => {
        if (!s.user) return s;
        const updated = { ...s.user, status };
        saveStoredUser(updated);
        return { ...s, user: updated };
      });
    },
    updateBalance: (deltaNanoTon: number | bigint) => {
      update((s) => {
        if (!s.user) return s;
        const current = Number(s.user.balanceInNanoTons);
        const newBalance = Math.max(0, current + Number(deltaNanoTon));
        const updated = { ...s.user, balanceInNanoTons: newBalance };
        saveStoredUser(updated);
        return { ...s, user: updated };
      });
    },
    register: async (email: string, username: string, password: string, walletAddress?: string) => {
      update((s) => ({ ...s, isLoading: true, error: null }));
      try {
        const res = await authService.register({ email, username, password, walletAddress });
        api.setToken(res.token);
        saveStoredUser(res.user);
        set({ user: res.user, token: res.token, resetEmail: null, resetToken: null, isLoading: false, error: null });
        return res;
      } catch (err: any) {
        const message = err.message || 'Ошибка регистрации';
        update((s) => ({ ...s, isLoading: false, error: message }));
        throw new Error(message);
      }
    },
    login: async (emailOrUsername: string, password: string) => {
      update((s) => ({ ...s, isLoading: true, error: null }));
      try {
        const res = await authService.login({ emailOrUsername, password });
        api.setToken(res.token);
        saveStoredUser(res.user);
        set({ user: res.user, token: res.token, resetEmail: null, resetToken: null, isLoading: false, error: null });
        return res;
      } catch (err: any) {
        const message = err.message || 'Ошибка входа';
        update((s) => ({ ...s, isLoading: false, error: message }));
        throw new Error(message);
      }
    },
    requestPasswordReset: async (email: string) => {
      update((s) => ({ ...s, isLoading: true, error: null }));
      try {
        const res = await authService.requestPasswordReset(email);
        update((s) => ({ ...s, resetEmail: email, isLoading: false, error: null }));
        return res;
      } catch (err: any) {
        const message = err.message || 'Ошибка запроса сброса пароля';
        update((s) => ({ ...s, isLoading: false, error: message }));
        throw new Error(message);
      }
    },
    verifyResetCode: async (code: string) => {
      update((s) => ({ ...s, isLoading: true, error: null }));
      let currentEmail = '';
      const unsubscribe = subscribe((s) => { currentEmail = s.resetEmail || ''; });
      unsubscribe();

      try {
        const res = await authService.verifyResetCode(currentEmail, code);
        const token = res.resetToken || code;
        update((s) => ({ ...s, resetToken: token, isLoading: false, error: null }));
        return res;
      } catch (err: any) {
        const message = err.message || 'Неверный код подтверждения';
        update((s) => ({ ...s, isLoading: false, error: message }));
        throw new Error(message);
      }
    },
    resetPassword: async (newPassword: string) => {
      update((s) => ({ ...s, isLoading: true, error: null }));
      let currentToken = '';
      const unsubscribe = subscribe((s) => { currentToken = s.resetToken || ''; });
      unsubscribe();

      try {
        const res = await authService.resetPassword(currentToken, newPassword);
        update((s) => ({ ...s, resetEmail: null, resetToken: null, isLoading: false, error: null }));
        return res;
      } catch (err: any) {
        const message = err.message || 'Ошибка обновления пароля';
        update((s) => ({ ...s, isLoading: false, error: message }));
        throw new Error(message);
      }
    },
    logout: async () => {
      await authService.logout();
      api.setToken(null);
      saveStoredUser(null);
      set({ user: null, token: null, resetEmail: null, resetToken: null, isLoading: false, error: null });
    },
  };
}

export const authStore = createAuthStore();
export const isAuthenticated = derived(authStore, ($auth) => !!$auth.user && !!$auth.token);
export const currentUser = derived(authStore, ($auth) => $auth.user);
export const isUserAdmin = derived(authStore, ($auth) => !!$auth.user?.isAdmin);
