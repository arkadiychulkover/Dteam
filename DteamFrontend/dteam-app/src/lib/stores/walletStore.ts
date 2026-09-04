import { writable, derived } from 'svelte/store';
import { tonService } from '../services/tonService';
import { authStore } from './authStore';
import { formatPrice } from '../utils/formatters';

interface WalletStoreState {
  isConnected: boolean;
  address: string | null;
  isConnecting: boolean;
}

function createWalletStore() {
  const { subscribe, set, update } = writable<WalletStoreState>({
    isConnected: true,
    address: 'EQBvW8Z5huBkMJYdn3PBRnVDLyTO2_OTHTuP4asMb_Fton',
    isConnecting: false,
  });

  return {
    subscribe,
    connect: async () => {
      update((s) => ({ ...s, isConnecting: true }));
      try {
        const address = await tonService.connectWallet();
        set({ isConnected: true, address, isConnecting: false });
      } catch {
        update((s) => ({ ...s, isConnecting: false }));
      }
    },
    disconnect: () => {
      set({ isConnected: false, address: null, isConnecting: false });
    },
    deposit: (tonAmount: number) => {
      const nanoTons = Math.floor(tonAmount * 1_000_000_000);
      authStore.updateBalance(nanoTons);
    },
  };
}

export const walletStore = createWalletStore();

