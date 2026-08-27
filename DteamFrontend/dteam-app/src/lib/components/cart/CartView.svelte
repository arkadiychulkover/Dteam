<script lang="ts">
  import { cartStore, cartTotals } from '../../stores/cartStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { uiStore } from '../../stores/uiStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { currentUser } from '../../stores/authStore';
  import { formatPrice, formatBasePrice, formatTon, nanoTonToTon } from '../../utils/formatters';
  import type { Game, CartItem } from '../../types';
  import { 
    ShoppingCart, 
    Trash2, 
    X, 
    ArrowRight, 
    ArrowLeft, 
    Heart, 
    Sparkles, 
    ShieldCheck,
    CreditCard,
    CheckCircle2,
    Coins,
    Wallet,
    Loader2
  } from 'lucide-svelte';

  const items = $derived($cartStore.items);
  const totals = $derived($cartTotals);

  let isCheckingOut = $state(false);

  function openGame(game: Game) {
    gamesStore.selectGame(game);
    uiStore.setTab('game');
  }

  async function handleCheckout() {
    if (!$currentUser) {
      uiStore.addToast({
        title: 'Увійдіть в акаунт',
        message: 'Для оформлення покупки необхідно увійти в акаунт.',
        type: 'warning',
      });
      uiStore.setLoginModal(true);
      return;
    }

    if (items.length === 0) return;

    const currentBalance = Number($currentUser.balanceInNanoTons) || 0;
    const requiredTotal = totals.totalEffectivePriceInNanoTons;

    if (currentBalance < requiredTotal) {
      uiStore.addToast({
        title: 'Недостатньо TON на балансі',
        message: `Для покупки потрібно ${formatPrice(requiredTotal)}, а ваш баланс становить ${formatPrice(currentBalance)}. Будь ласка, поповніть рахунок.`,
        type: 'warning',
      });
      uiStore.setDepositModal(true);
      return;
    }

    isCheckingOut = true;
    try {
      await cartStore.checkout();
    } catch (e) {
      console.warn('Checkout failed:', e);
    } finally {
      isCheckingOut = false;
    }
  }
</script>

<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">
  <div class="flex flex-wrap items-center justify-between gap-4 mb-8">
    <div>
      <div class="flex items-center gap-2 text-xs text-slate-400 mb-2">
        <button 
          onclick={() => uiStore.setTab('store')}
          class="hover:text-cyan-400 transition-colors cursor-pointer"
        >
          Головна
        </button>
        <span>/</span>
        <span class="text-slate-200">Кошик</span>
      </div>
      <h1 class="text-3xl sm:text-4xl font-black text-white font-display tracking-tight flex items-center gap-3">
        Ваш кошик
        {#if items.length > 0}
          <span class="text-xs px-2.5 py-1 rounded-full bg-cyan-500/20 text-cyan-300 font-bold border border-cyan-500/30">
            {items.length} {items.length === 1 ? 'товар' : items.length < 5 ? 'товари' : 'товарів'}
          </span>
        {/if}
      </h1>
    </div>

    {#if items.length > 0}
      <button
        onclick={() => uiStore.setTab('catalog')}
        class="inline-flex items-center gap-2 px-4 py-2 rounded-2xl bg-[#061820] hover:bg-cyan-950/60 border border-cyan-500/30 hover:border-cyan-400 text-xs font-bold text-slate-200 hover:text-white transition-all cursor-pointer shadow-sm"
      >
        <ArrowLeft class="w-3.5 h-3.5" />
        <span>Продовжити покупки</span>
      </button>
    {/if}
  </div>

  {#if items.length === 0}
    <div class="p-12 sm:p-16 rounded-3xl bg-[#061820]/70 border border-cyan-500/20 text-center max-w-xl mx-auto shadow-2xl backdrop-blur-xl">
      <div class="w-20 h-20 rounded-3xl bg-cyan-500/10 border border-cyan-500/30 flex items-center justify-center mx-auto mb-6 text-cyan-400 shadow-lg shadow-cyan-500/10">
        <ShoppingCart class="w-10 h-10" />
      </div>
      <h2 class="text-2xl font-black text-white font-display">
        Ваш кошик порожній
      </h2>
      <p class="text-sm text-slate-400 mt-2 max-w-md mx-auto leading-relaxed">
        Ви ще не додали жодної гри чи DLC. Перегляньте найкращі новинки та хіти в нашому каталозі.
      </p>
      <div class="mt-8 flex flex-wrap items-center justify-center gap-3">
        <button
          onclick={() => uiStore.setTab('catalog')}
          class="px-6 py-3 rounded-2xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center gap-2"
        >
          <span>Перейти до каталогу</span>
          <ArrowRight class="w-4 h-4" />
        </button>
        <button
          onclick={() => uiStore.setTab('wishlist')}
          class="px-5 py-3 rounded-2xl bg-[#0b2834] hover:bg-[#0f3444] border border-cyan-500/30 text-cyan-300 hover:text-white font-bold text-xs transition-all cursor-pointer flex items-center gap-2"
        >
          <Heart class="w-3.5 h-3.5" />
          <span>Список бажань</span>
        </button>
      </div>
    </div>

  {:else}
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
      <div class="lg:col-span-8 space-y-4">
        {#each items as item (item.gameId)}
          <div class="p-4 sm:p-5 rounded-3xl bg-[#061820]/90 backdrop-blur-xl border border-cyan-500/20 hover:border-cyan-500/40 transition-all flex flex-col sm:flex-row items-stretch sm:items-center gap-4 sm:gap-6 group shadow-lg">
            <button
              type="button"
              onclick={() => openGame(item.game)}
              class="w-full sm:w-44 h-28 sm:h-24 rounded-2xl overflow-hidden bg-slate-900 shrink-0 relative cursor-pointer group-hover:scale-[1.02] transition-transform text-left border border-cyan-500/20"
            >
              <img
                src={item.game.capsuleImageUrl || item.game.headerImageUrl || item.game.coverUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600&auto=format&fit=crop&q=80'}
                alt={item.game.title}
                class="w-full h-full object-cover"
              />
              {#if item.game.isDlc}
                <span class="absolute top-2 left-2 px-2 py-0.5 rounded-md bg-purple-600/90 text-white font-black text-[9px] uppercase tracking-wider backdrop-blur-sm shadow-md">
                  DLC
                </span>
              {/if}
            </button>

            <div class="flex-1 min-w-0 flex flex-col justify-between self-stretch">
              <div>
                <div class="flex items-center gap-2">
                  <button
                    type="button"
                    onclick={() => openGame(item.game)}
                    class="text-base sm:text-lg font-black text-white hover:text-cyan-400 transition-colors truncate text-left cursor-pointer font-display"
                  >
                    {item.game.title}
                  </button>
                </div>

                {#if item.game.genres && item.game.genres.length > 0}
                  <div class="flex flex-wrap gap-1.5 mt-1.5">
                    {#each item.game.genres.slice(0, 3) as genre}
                      <span class="text-[10px] px-2 py-0.5 rounded-lg bg-[#041219] text-cyan-300/80 border border-cyan-500/15 font-medium">
                        {genre}
                      </span>
                    {/each}
                  </div>
                {/if}
              </div>

              <div class="flex items-center gap-4 mt-3 pt-2 border-t border-cyan-950/60">
                <button
                  type="button"
                  onclick={() => cartStore.moveToWishlist(item.gameId, item.game.title)}
                  class="text-[11px] font-semibold text-slate-400 hover:text-rose-400 transition-colors flex items-center gap-1.5 cursor-pointer"
                >
                  <Heart class="w-3.5 h-3.5" />
                  <span>Перемістити до бажаного</span>
                </button>
              </div>
            </div>

            <div class="flex sm:flex-col items-center sm:items-end justify-between sm:justify-between shrink-0 self-stretch sm:pl-4 sm:border-l sm:border-cyan-950/80">
              <button
                type="button"
                onclick={() => cartStore.removeFromCart(item.gameId, item.game.title)}
                class="text-slate-400 hover:text-rose-400 p-1.5 rounded-xl hover:bg-rose-950/40 transition-colors cursor-pointer"
                title="Видалити з кошика"
                aria-label="Видалити {item.game.title}"
              >
                <X class="w-4 h-4" />
              </button>

              <div class="flex items-center gap-2.5 mt-auto">
                {#if (item.game.discountPercentage || 0) > 0}
                  <span class="px-2 py-0.5 rounded-lg bg-rose-600 text-white font-extrabold text-xs">
                    -{item.game.discountPercentage}%
                  </span>
                {/if}

                <div class="text-right">
                  <div class="text-base sm:text-lg font-black text-white font-mono">
                    {formatPrice(item.game.priceInNanoTons, item.game.discountPercentage)}
                  </div>
                  {#if (item.game.discountPercentage || 0) > 0}
                    <div class="text-[11px] text-slate-500 line-through font-mono">
                      {formatBasePrice(item.game.priceInNanoTons)}
                    </div>
                  {/if}
                </div>
              </div>

            </div>

          </div>
        {/each}
      </div>

      <div class="lg:col-span-4 sticky top-20 space-y-4">
        <div class="bg-[#061820]/90 backdrop-blur-xl border border-cyan-500/25 rounded-3xl p-6 shadow-2xl shadow-cyan-950/60 space-y-5">
          {#if $currentUser}
            <div class="p-3 rounded-2xl bg-[#041219] border border-cyan-500/20 flex items-center justify-between text-xs">
              <div class="flex items-center gap-1.5 text-slate-300">
                <Wallet class="w-3.5 h-3.5 text-cyan-400" />
                <span>Ваш баланс:</span>
              </div>
              <div class="flex items-center gap-2">
                <span class="font-bold text-white font-mono text-[11px]">
                  💎 {formatTon(nanoTonToTon($currentUser.balanceInNanoTons))}
                </span>
                <button
                  type="button"
                  onclick={() => uiStore.setDepositModal(true)}
                  class="px-2 py-0.5 rounded-lg bg-cyan-500/20 hover:bg-cyan-500/30 text-cyan-300 hover:text-white border border-cyan-500/30 text-[10px] font-bold cursor-pointer transition-colors"
                >
                  + Поповнити
                </button>
              </div>
            </div>
          {/if}

          <div class="space-y-3 text-sm">
            <div class="flex items-center justify-between text-slate-300">
              <span>Ви заощадите</span>
              <span class="font-bold text-cyan-300 font-mono">
                {formatPrice(totals.totalSavingsInNanoTons)}
              </span>
            </div>

            <div class="flex items-center justify-between text-base pt-2 border-t border-cyan-950/80">
              <span class="font-bold text-white">Усього</span>
              <span class="text-2xl font-black text-white font-mono tracking-tight">
                {formatPrice(totals.totalEffectivePriceInNanoTons)}
              </span>
            </div>
          </div>

          <p class="text-[11px] text-slate-400 leading-relaxed">
            Якщо застосовно, податок із продажу буде розраховано в процесі оплати.
          </p>

          <button
            type="button"
            onclick={handleCheckout}
            disabled={isCheckingOut}
            class="w-full py-3.5 rounded-2xl bg-gradient-to-r from-emerald-400 via-teal-400 to-cyan-400 hover:from-emerald-300 hover:to-cyan-300 disabled:opacity-60 disabled:cursor-not-allowed text-black font-black text-sm tracking-wide shadow-lg shadow-emerald-500/25 hover:shadow-cyan-400/40 transition-all cursor-pointer flex items-center justify-center gap-2"
          >
            {#if isCheckingOut}
              <Loader2 class="w-4 h-4 animate-spin text-black" />
              <span>Обробка покупки...</span>
            {:else}
              <span>Купити {totals.itemsCount === 1 ? 'гру' : 'ігри'} ({formatPrice(totals.totalEffectivePriceInNanoTons)})</span>
            {/if}
          </button>

          <button
            type="button"
            onclick={() => uiStore.setTab('catalog')}
            class="w-full py-3 rounded-2xl bg-[#0b2834] hover:bg-[#0f3444] border border-cyan-500/30 text-cyan-300 hover:text-white font-bold text-xs transition-all cursor-pointer text-center"
          >
            <span>Продовжити покупки</span>
          </button>

          <div class="pt-2 text-center border-t border-cyan-950/80">
            <button
              type="button"
              onclick={() => cartStore.clearCart()}
              class="text-xs font-semibold text-slate-400 hover:text-rose-400 transition-colors cursor-pointer"
            >
              Очистити кошик
            </button>
          </div>

        </div>

        <div class="p-4 rounded-2xl bg-[#041219]/80 border border-cyan-500/15 flex items-center gap-3 text-xs text-slate-400">
          <ShieldCheck class="w-5 h-5 text-cyan-400 shrink-0" />
          <span>Безпечна оплата через TON Blockchain та миттєва активація у вашій бібліотеці.</span>
        </div>

      </div>

    </div>
  {/if}
</div>
