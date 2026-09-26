import { create } from 'zustand';
import { authService } from '../services/authService';
import { tokenStorage } from '../services/tokenStorage';
import type { UserDto } from '../types';

interface AuthStoreState {
  user: UserDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;

  checkAuth: () => Promise<void>;
  setUser: (user: UserDto | null) => void;
  loginSuccess: (user: UserDto) => void;
  logout: () => Promise<void>;
}

let checkAuthPromise: Promise<void> | null = null;

export const useAuthStore = create<AuthStoreState>((set) => ({
  user: null,
  isAuthenticated: false,
  isLoading: true,

  checkAuth: async () => {
    if (checkAuthPromise) {
      return checkAuthPromise;
    }

    checkAuthPromise = (async () => {
      set({ isLoading: true });
      try {
        await tokenStorage.init();
        const token = await tokenStorage.getToken();
        if (!token || tokenStorage.isTokenExpired(token)) {
          set({ user: null, isAuthenticated: false, isLoading: false });
          return;
        }

        const user = await authService.getMe();
        set({ user, isAuthenticated: true, isLoading: false });
      } catch (err: any) {
        console.warn('[AuthStore] Failed to check current user session:', err);
        // Only clear user credentials if the token is rejected (401 Unauthorized)
        if (err?.status === 401) {
          await tokenStorage.clearTokens().catch(() => {});
          set({ user: null, isAuthenticated: false, isLoading: false });
        } else {
          // For 429 Too Many Requests or network hiccups, retain current auth state
          set({ isLoading: false });
        }
      } finally {
        checkAuthPromise = null;
      }
    })();

    return checkAuthPromise;
  },

  setUser: (user) => {
    set({ user, isAuthenticated: !!user });
  },

  loginSuccess: (user) => {
    set({ user, isAuthenticated: true, isLoading: false });
  },

  logout: async () => {
    try {
      await authService.logout();
    } catch (e) {
      console.warn('[AuthStore] Logout error:', e);
    } finally {
      set({ user: null, isAuthenticated: false, isLoading: false });
    }
  },
}));
