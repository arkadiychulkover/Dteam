import { writable, derived, get } from 'svelte/store';
import type { Game, CartItem } from '../types';
import { cartService } from '../services/cartService';
import { authStore, currentUser } from './authStore';
import { wishlistStore } from './wishlistStore';
import { uiStore } from './uiStore';

function createCartStore() {
  const { subscribe, set, update } = writable<{
    items: CartItem[];
    cartGameIds: Set<string>;
    isLoading: boolean;
  }>({
    items: [],
    cartGameIds: new Set<string>(),
    isLoading: false,
  });

  return {
    subscribe,

    loadCart: async () => {
      const user = get(currentUser);
      if (!user?.id) {
        update((s) => ({ ...s, items: [], cartGameIds: new Set(), isLoading: false }));
        return;
      }

      update((s) => ({ ...s, isLoading: true }));
      try {
        const summary = await cartService.getCart();
        const items = summary.items || [];
        const gameIds = new Set(items.map((i) => i.gameId));
        update((s) => ({
          ...s,
          items,
          cartGameIds: gameIds,
          isLoading: false,
        }));
      } catch (err) {
        console.warn('[cartStore] Failed to load cart:', err);
        update((s) => ({ ...s, isLoading: false }));
      }
    },

    addToCart: async (game: Game, redirectToCart: boolean = false) => {
      const user = get(currentUser);
      if (!user?.id) {
        uiStore.addToast({
          title: 'Увійдіть в акаунт',
          message: 'Для додавання товарів до кошика потрібна авторизація.',
          type: 'warning',
        });
        uiStore.setLoginModal(true);
        return false;
      }

      const state = get({ subscribe });
      const isAlreadyIn = state.cartGameIds.has(game.id);

      if (isAlreadyIn) {
        uiStore.addToast({
          title: 'Вже в кошику',
          message: `Гру '${game.title}' вже додано до вашого кошика.`,
          type: 'info',
        });
        if (redirectToCart) {
          uiStore.setTab('cart');
        }
        return true;
      }

      // Optimistic Add
      const dummyItem: CartItem = {
        userId: user.id,
        gameId: game.id,
        game,
        addedAt: new Date().toISOString(),
      };

      update((s) => {
        const nextGameIds = new Set(s.cartGameIds);
        nextGameIds.add(game.id);
        return {
          ...s,
          items: [dummyItem, ...s.items],
          cartGameIds: nextGameIds,
        };
      });

      try {
        const newItem = await cartService.addToCart(game.id);
        update((s) => ({
          ...s,
          items: [newItem, ...s.items.filter((i) => i.gameId !== game.id)],
        }));

        uiStore.addToast({
          title: 'Додано в кошик 🛒',
          message: `Гру '${game.title}' успішно додано до вашого кошика!`,
          type: 'success',
        });

        if (redirectToCart) {
          uiStore.setTab('cart');
        }
        return true;
      } catch (err: any) {
        uiStore.addToast({ title: 'Помилка', message: err.message || 'Не вдалося додати до кошика', type: 'error' });
        update((s) => {
          const nextGameIds = new Set(s.cartGameIds);
          nextGameIds.delete(game.id);
          return {
            ...s,
            items: s.items.filter((i) => i.gameId !== game.id),
            cartGameIds: nextGameIds,
          };
        });
        return false;
      }
    },

    removeFromCart: async (gameId: string, gameTitle?: string) => {
      const user = get(currentUser);
      if (!user?.id) return;

      const prevItems = get({ subscribe }).items;

      update((s) => {
        const nextGameIds = new Set(s.cartGameIds);
        nextGameIds.delete(gameId);
        return {
          ...s,
          items: s.items.filter((i) => i.gameId !== gameId),
          cartGameIds: nextGameIds,
        };
      });

      try {
        await cartService.removeFromCart(gameId);
        uiStore.addToast({
          title: 'Видалено з кошика',
          message: gameTitle ? `Товар '${gameTitle}' видалено з кошика.` : 'Товар видалено з кошика.',
          type: 'info',
        });
      } catch (err: any) {
        uiStore.addToast({ title: 'Помилка видалення', message: err.message, type: 'error' });
        update((s) => ({
          ...s,
          items: prevItems,
          cartGameIds: new Set(prevItems.map((i) => i.gameId)),
        }));
      }
    },

    clearCart: async () => {
      const user = get(currentUser);
      if (!user?.id) return;

      try {
        await cartService.clearCart();
        update((s) => ({ ...s, items: [], cartGameIds: new Set() }));
        uiStore.addToast({
          title: 'Кошик очищено',
          message: 'Усі товари успішно видалено з кошика.',
          type: 'info',
        });
      } catch (err: any) {
        uiStore.addToast({ title: 'Помилка', message: err.message, type: 'error' });
      }
    },

    moveToWishlist: async (game: Game) => {
      const user = get(currentUser);
      if (!user?.id) return;

      // Optimistic update
      update((s) => {
        const nextGameIds = new Set(s.cartGameIds);
        nextGameIds.delete(game.id);
        return {
          ...s,
          items: s.items.filter((i) => i.gameId !== game.id),
          cartGameIds: nextGameIds,
        };
      });

      try {
        await cartService.moveToWishlist(game.id);
        wishlistStore.loadWishlist();
        uiStore.addToast({
          title: 'Переміщено до бажаного ♥',
          message: `Гру '${game.title}' переміщено зі списку кошика до списку бажань!`,
          type: 'success',
        });
      } catch (err: any) {
        uiStore.addToast({ title: 'Помилка', message: err.message, type: 'error' });
        const summary = await cartService.getCart();
        update((s) => ({
          ...s,
          items: summary.items || [],
          cartGameIds: new Set((summary.items || []).map((i) => i.gameId)),
        }));
      }
    },

    checkout: async () => {
      const user = get(currentUser);
      if (!user?.id) {
        uiStore.addToast({
          title: 'Увійдіть в акаунт',
          message: 'Для здійснення покупки необхідно авторизуватися.',
          type: 'warning',
        });
        uiStore.setLoginModal(true);
        return null;
      }

      update((s) => ({ ...s, isLoading: true }));
      try {
        const res = await cartService.checkout();
        update((s) => ({ ...s, items: [], cartGameIds: new Set(), isLoading: false }));
        authStore.updateBalance(-res.totalSpentInNanoTons);
        uiStore.addToast({
          title: 'Успішна покупка! 🎉',
          message: res.message || 'Ігри додано до вашої бібліотеки!',
          type: 'success',
        });
        return res;
      } catch (err: any) {
        update((s) => ({ ...s, isLoading: false }));
        const errorMessage = err.message || 'Помилка при оформленні замовлення.';
        
        if (errorMessage.toLowerCase().includes('недостатньо коштів') || errorMessage.toLowerCase().includes('balance')) {
          uiStore.addToast({
            title: 'Недостатньо коштів',
            message: 'На вашому балансі недостатньо TON. Будь ласка, поповніть рахунок.',
            type: 'warning',
          });
          uiStore.setDepositModal(true);
        } else {
          uiStore.addToast({
            title: 'Помилка покупки',
            message: errorMessage,
            type: 'error',
          });
        }
        throw err;
      }
    },
  };
}

export const cartStore = createCartStore();

export const isGameInCart = (gameId: string) => {
  return derived(cartStore, ($s) => $s.cartGameIds.has(gameId));
};

export const cartTotals = derived(cartStore, ($s) => {
  const items = $s.items;
  const totalBasePriceInNanoTons = items.reduce((acc, item) => acc + (Number(item.game.priceInNanoTons) || 0), 0);
  const totalEffectivePriceInNanoTons = items.reduce((acc, item) => {
    const price = Number(item.game.priceInNanoTons) || 0;
    const discount = Number(item.game.discountPercentage) || 0;
    const effective = discount > 0 ? Math.round(price * (100 - discount) / 100) : price;
    return acc + effective;
  }, 0);
  const totalSavingsInNanoTons = Math.max(0, totalBasePriceInNanoTons - totalEffectivePriceInNanoTons);

  return {
    totalBasePriceInNanoTons,
    totalEffectivePriceInNanoTons,
    totalSavingsInNanoTons,
    itemsCount: items.length,
  };
});
