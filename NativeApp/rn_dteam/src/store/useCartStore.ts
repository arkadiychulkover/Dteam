import { create } from 'zustand';
import type { CartItem, CheckoutResult } from '../types';
import { cartService } from '../services/cartService';
import { useAuthStore } from './useAuthStore';

interface CartStoreState {
  items: CartItem[];
  cartGameIds: Set<string>;
  isLoading: boolean;
  isCheckingOut: boolean;
  totalBasePriceInNanoTons: number;
  totalEffectivePriceInNanoTons: number;
  totalSavingsInNanoTons: number;

  isInCart: (gameId: string) => boolean;
  loadCart: () => Promise<void>;
  addToCart: (gameId: string) => Promise<boolean>;
  removeFromCart: (gameId: string) => Promise<void>;
  clearCart: () => Promise<void>;
  checkout: () => Promise<CheckoutResult>;
}

function calculateTotals(items: CartItem[]) {
  const totalBasePriceInNanoTons = items.reduce(
    (acc, i) => acc + (Number(i.game?.priceInNanoTons) || 0),
    0
  );
  const totalEffectivePriceInNanoTons = items.reduce((acc, i) => {
    const price = Number(i.game?.priceInNanoTons) || 0;
    const discount = Number(i.game?.discountPercentage) || 0;
    const effective = discount > 0 ? Math.round((price * (100 - discount)) / 100) : price;
    return acc + effective;
  }, 0);
  const totalSavingsInNanoTons = Math.max(
    0,
    totalBasePriceInNanoTons - totalEffectivePriceInNanoTons
  );

  return {
    totalBasePriceInNanoTons,
    totalEffectivePriceInNanoTons,
    totalSavingsInNanoTons,
  };
}

export const useCartStore = create<CartStoreState>((set, get) => ({
  items: [],
  cartGameIds: new Set<string>(),
  isLoading: false,
  isCheckingOut: false,
  totalBasePriceInNanoTons: 0,
  totalEffectivePriceInNanoTons: 0,
  totalSavingsInNanoTons: 0,

  isInCart: (gameId: string) => {
    return get().cartGameIds.has(gameId);
  },

  loadCart: async () => {
    const { isAuthenticated } = useAuthStore.getState();
    if (!isAuthenticated) {
      set({
        items: [],
        cartGameIds: new Set(),
        totalBasePriceInNanoTons: 0,
        totalEffectivePriceInNanoTons: 0,
        totalSavingsInNanoTons: 0,
        isLoading: false,
      });
      return;
    }

    set({ isLoading: true });
    try {
      const summary = await cartService.getCart();
      const items = summary.items || [];
      const gameIds = new Set(items.map((i) => i.gameId || i.game?.id));
      const totals = calculateTotals(items);

      set({
        items,
        cartGameIds: gameIds,
        ...totals,
        isLoading: false,
      });
    } catch (err) {
      console.warn('[useCartStore] Error loading cart:', err);
      set({ isLoading: false });
    }
  },

  addToCart: async (gameId: string) => {
    const { isAuthenticated } = useAuthStore.getState();
    if (!isAuthenticated) {
      return false;
    }

    if (get().isInCart(gameId)) {
      return true;
    }

    try {
      const addedItem = await cartService.addToCart(gameId);
      const updatedItems = [
        addedItem,
        ...get().items.filter((i) => (i.gameId || i.game?.id) !== gameId),
      ];
      const updatedGameIds = new Set(get().cartGameIds);
      updatedGameIds.add(gameId);
      const totals = calculateTotals(updatedItems);

      set({
        items: updatedItems,
        cartGameIds: updatedGameIds,
        ...totals,
      });
      return true;
    } catch (err: any) {
      console.warn('[useCartStore] Failed to add item to cart:', err);
      throw err;
    }
  },

  removeFromCart: async (gameId: string) => {
    const prevItems = get().items;
    const nextItems = prevItems.filter((i) => (i.gameId || i.game?.id) !== gameId);
    const nextGameIds = new Set(get().cartGameIds);
    nextGameIds.delete(gameId);
    const totals = calculateTotals(nextItems);

    set({
      items: nextItems,
      cartGameIds: nextGameIds,
      ...totals,
    });

    try {
      await cartService.removeFromCart(gameId);
    } catch (err) {
      console.warn('[useCartStore] Failed to remove from cart on server:', err);
      // rollback if failed
      const prevTotals = calculateTotals(prevItems);
      set({
        items: prevItems,
        cartGameIds: new Set(prevItems.map((i) => i.gameId || i.game?.id)),
        ...prevTotals,
      });
      throw err;
    }
  },

  clearCart: async () => {
    set({
      items: [],
      cartGameIds: new Set(),
      totalBasePriceInNanoTons: 0,
      totalEffectivePriceInNanoTons: 0,
      totalSavingsInNanoTons: 0,
    });

    try {
      await cartService.clearCart();
    } catch (err) {
      console.warn('[useCartStore] Failed to clear cart:', err);
      await get().loadCart();
    }
  },

  checkout: async () => {
    set({ isCheckingOut: true });
    try {
      const res = await cartService.checkout();
      set({
        items: [],
        cartGameIds: new Set(),
        totalBasePriceInNanoTons: 0,
        totalEffectivePriceInNanoTons: 0,
        totalSavingsInNanoTons: 0,
        isCheckingOut: false,
      });

      // Оновлюємо профіль користувача, щоб відобразити новий баланс у useAuthStore
      await useAuthStore.getState().checkAuth().catch(() => {});

      return res;
    } catch (err: any) {
      set({ isCheckingOut: false });
      throw err;
    }
  },
}));
