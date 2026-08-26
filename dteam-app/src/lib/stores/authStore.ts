import { writable, derived } from 'svelte/store';
import type { Duser, UserStatus } from '../types';
import { authService } from '../services/authService';
import { api } from '../services/api';

function createAuthStore() {
  const initialToken = api.getToken();

  const { subscribe, set, update } = writable<{
    user: Duser | null;
    token: string | null;
    resetEmail: string | null;
    resetToken: string | null;
    isLoading: boolean;
    error: string | null;
  }>({
    user: null,
    token: initialToken,
    resetEmail: null,
    resetToken: null,
    isLoading: false,
    error: null,
  });

  // Attempt to restore session on initialization if token exists
  if (initialToken) {
    authService.getProfile()
      .then((user) => {
        update((s) => ({ ...s, user, isLoading: false }));
      })
      .catch(() => {
        api.setToken(null);
        update((s) => ({ ...s, user: null, token: null, isLoading: false }));
      });
  }

  return {
    subscribe,
    setUser: (user: Duser | null) => update((s) => ({ ...s, user })),
    setStatus: (status: UserStatus) => {
      update((s) => {
        if (!s.user) return s;
        return { ...s, user: { ...s.user, status } };
      });
    },
    updateBalance: (deltaNanoTon: number | bigint) => {
      update((s) => {
        if (!s.user) return s;
        const current = Number(s.user.balanceInNanoTons);
        const newBalance = Math.max(0, current + Number(deltaNanoTon));
        return { ...s, user: { ...s.user, balanceInNanoTons: newBalance } };
      });
    },
    register: async (email: string, username: string, password: string, walletAddress?: string) => {
      update((s) => ({ ...s, isLoading: true, error: null }));
      try {
        const res = await authService.register({ email, username, password, walletAddress });
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
      set({ user: null, token: null, resetEmail: null, resetToken: null, isLoading: false, error: null });
    },
  };
}

export const authStore = createAuthStore();
export const isAuthenticated = derived(authStore, ($auth) => !!$auth.user);
export const currentUser = derived(authStore, ($auth) => $auth.user);
