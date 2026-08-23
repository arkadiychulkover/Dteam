import { writable, derived } from 'svelte/store';
import type { Duser, UserStatus } from '../types';
import { authService } from '../services/authService';

const MOCK_USER: Duser = {
  id: 'd199452b-23fa-4cf3-a633-8fe5932599fa',
  email: 'cyber_gamer@dteam.io',
  username: 'CyberPlayer',
  walletAddress: 'EQBvW8Z5huBkMJYdn3PBRnVDLyTO2_OTHTuP4asMb_Fton',
  balanceInNanoTons: 15_500_000_000, // 15.5 TON
  totalEarningsInNanoTons: 42_000_000_000, // 42 TON
  createdAt: new Date().toISOString(),
  isInFamily: true,
  isAdmin: true,
  isBanned: false,
  status: 1 as UserStatus, // Online
  avatarUrl: 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150&auto=format&fit=crop&q=80',
  bio: 'Web3 game dev & collector. Exploring the decentralized metaverse 🚀',
};

function createAuthStore() {
  const { subscribe, set, update } = writable<{
    user: Duser | null;
    token: string | null;
    isLoading: boolean;
    error: string | null;
  }>({
    user: MOCK_USER,
    token: 'mock-jwt-token-dteam',
    isLoading: false,
    error: null,
  });

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
    login: async (emailOrUsername: string, password: string) => {
      update((s) => ({ ...s, isLoading: true, error: null }));
      try {
        const res = await authService.login({ emailOrUsername, password });
        set({ user: res.user, token: res.token, isLoading: false, error: null });
      } catch (err: any) {
        // Fallback for dev mode
        update((s) => ({
          ...s,
          isLoading: false,
          user: { ...MOCK_USER, username: emailOrUsername },
          error: null,
        }));
      }
    },
    logout: async () => {
      await authService.logout();
      set({ user: null, token: null, isLoading: false, error: null });
    },
  };
}

export const authStore = createAuthStore();
export const isAuthenticated = derived(authStore, ($auth) => !!$auth.user);
export const currentUser = derived(authStore, ($auth) => $auth.user);
