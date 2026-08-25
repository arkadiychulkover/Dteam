<script lang="ts">
  import { cartStore, cartTotals } from '../../stores/cartStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { uiStore } from '../../stores/uiStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { currentUser } from '../../stores/authStore';
  import { formatPrice, formatBasePrice } from '../../utils/formatters';
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
    CheckCircle2
  } from 'lucide-svelte';

  const items = $derived($cartStore.items);
  const totals = $derived($cartTotals);

  function openGame(game: Game) {
    gamesStore.selectGame(game);
    uiStore.setTab('game');
  }

  function handleCheckout() {
    uiStore.addToast({
      title: 'Оформлення замовлення',
      message: `Сума до сплати: ${formatPrice(totals.totalEffectivePriceInNanoTons)}. Інтеграція TON Connect увімкнена в тестовому режимі.`,
      type: 'success',
    });
  }
</script>

<div class="max-w-7xl mx-auto px-4 lg:px-8 py-8 space-y-8 animate-in fade-in duration-300">
  
  <!-- Breadcrumbs & Navigation -->
  <div class="flex items-center justify-between border-b border-cyan-950/60 pb-3">
    <div class="flex items-center gap-2 text-xs text-slate-400">
      <button onclick={() => uiStore.setTab('store')} class="hover:text-cyan-400 transition-colors cursor-pointer">
        Крамниця
      </button>
      <span>/</span>
      <span class="text-white font-bold">Кошик</span>
    </div>

    <button
      onclick={() => uiStore.setTab('store')}
      class="inline-flex items-center gap-1.5 text-xs font-bold text-slate-400 hover:text-cyan-400 transition-colors cursor-pointer"
    >
      <ArrowLeft class="w-3.5 h-3.5" />
      <span>Продовжити покупки</span>
    </button>
  </div>

  <!-- Main Section Title -->
  <div class="flex items-center justify-between">
    <h1 class="text-3xl sm:text-4xl font-black text-white font-display tracking-wide flex items-center gap-3">
      <span>Мій кошик</span>
      {#if items.length > 0}
        <span class="text-base font-bold px-3 py-0.5 rounded-full bg-cyan-500/20 border border-cyan-500/40 text-cyan-300 font-mono">
          {items.length}
        </span>
      {/if}
    </h1>
  </div>

  {#if items.length > 0}
    <!-- Two Column Layout (Matching Reference Screenshot) -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
      
      <!-- Left Column: Cart Items List -->
      <div class="lg:col-span-8 space-y-4">
        {#each items as item (item.gameId)}
          <div class="relative bg-[#061820]/90 backdrop-blur-xl border border-cyan-500/20 hover:border-cyan-500/40 rounded-3xl p-4 sm:p-5 transition-all shadow-xl shadow-cyan-950/30 flex flex-col sm:flex-row items-center gap-4 sm:gap-6 group">
            
            <!-- Game Image Banner -->
            <button
              type="button"
              onclick={() => openGame(item.game)}
              class="w-full sm:w-48 aspect-[16/9] sm:aspect-[16/8] rounded-2xl overflow-hidden bg-slate-950 border border-cyan-500/20 shrink-0 group-hover:scale-[1.02] transition-transform cursor-pointer relative shadow-inner"
            >
              <img
                src={item.game.headerImageUrl || item.game.coverImageUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600&auto=format&fit=crop&q=80'}
                alt={item.game.title}
                class="w-full h-full object-cover"
              />
            </button>

            <!-- Game Details -->
            <div class="flex-1 min-w-0 flex flex-col justify-between self-stretch py-1 space-y-2">
              <div>
                <!-- Title -->
                <button
                  type="button"
                  onclick={() => openGame(item.game)}
                  class="text-left font-black text-lg sm:text-xl text-white hover:text-cyan-300 transition-colors cursor-pointer truncate max-w-full block"
                >
                  {item.game.title}
                </button>

                <!-- Move to Wishlist Link -->
                <button
                  type="button"
                  onclick={() => cartStore.moveToWishlist(item.game)}
                  class="text-xs font-bold text-cyan-400/80 hover:text-cyan-300 hover:underline transition-all cursor-pointer inline-flex items-center gap-1 mt-1"
                >
                  <span>Перемістити до Бажаного</span>
                </button>
              </div>

              <!-- Platforms / Genre tags -->
              {#if item.game.genres && item.game.genres.length > 0}
                <div class="flex flex-wrap gap-1.5 pt-1">
                  {#each item.game.genres.slice(0, 2) as genre}
                    <span class="px-2 py-0.5 rounded-md bg-[#08232e] text-[10px] font-semibold text-slate-300 border border-cyan-500/10">
                      {genre}
                    </span>
                  {/each}
                </div>
              {/if}
            </div>

            <!-- Price and Actions -->
            <div class="flex sm:flex-col items-center sm:items-end justify-between w-full sm:w-auto gap-3 shrink-0 self-stretch sm:self-auto pt-2 sm:pt-0 border-t sm:border-t-0 border-cyan-950/60">
              
              <!-- Remove '✕' Button (Top right on desktop) -->
              <button
                type="button"
                onclick={() => cartStore.removeFromCart(item.gameId, item.game.title)}
                class="text-slate-400 hover:text-rose-400 p-1.5 rounded-xl hover:bg-rose-950/40 transition-colors cursor-pointer"
                title="Видалити з кошика"
                aria-label="Видалити {item.game.title}"
              >
                <X class="w-4 h-4" />
              </button>

              <!-- Prices Display -->
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

      <!-- Right Column: Order Summary (Sticky matching reference) -->
      <div class="lg:col-span-4 sticky top-20 space-y-4">
        <div class="bg-[#061820]/90 backdrop-blur-xl border border-cyan-500/25 rounded-3xl p-6 shadow-2xl shadow-cyan-950/60 space-y-5">
          
          <!-- Summary Lines -->
          <div class="space-y-3 text-sm">
            <!-- Savings Row -->
            <div class="flex items-center justify-between text-slate-300">
              <span>Ви заощадите</span>
              <span class="font-bold text-cyan-300 font-mono">
                {formatPrice(totals.totalSavingsInNanoTons)}
              </span>
            </div>

            <!-- Total Row -->
            <div class="flex items-center justify-between text-base pt-2 border-t border-cyan-950/80">
              <span class="font-bold text-white">Усього</span>
              <span class="text-2xl font-black text-white font-mono tracking-tight">
                {formatPrice(totals.totalEffectivePriceInNanoTons)}
              </span>
            </div>
          </div>

          <!-- Tax / Legal Notice -->
          <p class="text-[11px] text-slate-400 leading-relaxed">
            Якщо застосовно, податок із продажу буде розраховано в процесі оплати.
          </p>

          <!-- Primary Button (Перейти до оплати) -->
          <button
            type="button"
            onclick={handleCheckout}
            class="w-full py-3.5 rounded-2xl bg-gradient-to-r from-emerald-400 via-teal-400 to-cyan-400 hover:from-emerald-300 hover:to-cyan-300 text-black font-black text-sm tracking-wide shadow-lg shadow-emerald-500/25 hover:shadow-cyan-400/40 transition-all cursor-pointer flex items-center justify-center gap-2"
          >
            <span>Перейти до оплати</span>
          </button>

          <!-- Secondary Button (Продовжити покупки) -->
          <button
            type="button"
            onclick={() => uiStore.setTab('catalog')}
            class="w-full py-3 rounded-2xl bg-[#0b2834] hover:bg-[#0f3444] border border-cyan-500/30 text-cyan-300 hover:text-white font-bold text-xs transition-all cursor-pointer text-center"
          >
            <span>Продовжити покупки</span>
          </button>

          <!-- Clear Cart Action -->
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

        <!-- Security / Guarantee Badge -->
        <div class="p-4 rounded-2xl bg-[#041219]/80 border border-cyan-500/15 flex items-center gap-3 text-xs text-slate-400">
          <ShieldCheck class="w-5 h-5 text-cyan-400 shrink-0" />
          <span>Безпечна оплата через TON Blockchain та миттєва активація у вашій бібліотеці.</span>
        </div>
      </div>

    </div>
  {:else}
    <!-- Empty Cart State -->
    <div class="text-center py-20 bg-[#061820]/60 rounded-3xl border border-cyan-500/20 shadow-2xl max-w-2xl mx-auto space-y-5 p-8">
      <div class="w-20 h-20 rounded-full bg-cyan-500/10 border border-cyan-500/30 flex items-center justify-center mx-auto text-cyan-400">
        <ShoppingCart class="w-10 h-10" />
      </div>

      <div class="space-y-2">
        <h2 class="text-2xl font-black text-white font-display">Ваш кошик порожній</h2>
        <p class="text-sm text-slate-400 max-w-md mx-auto">
          У вашому кошику поки немає обраних ігор. Ознайомтеся з каталогом та знайдіть свої улюблені проекти!
        </p>
      </div>

      <div class="pt-3 flex flex-wrap items-center justify-center gap-4">
        <button
          onclick={() => uiStore.setTab('catalog')}
          class="px-6 py-3 rounded-2xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center gap-2"
        >
          <span>Перейти до каталогу</span>
          <ArrowRight class="w-4 h-4" />
        </button>

        <button
          onclick={() => uiStore.setTab('wishlist')}
          class="px-6 py-3 rounded-2xl bg-[#0b2834] hover:bg-[#0f3444] border border-cyan-500/30 text-cyan-300 hover:text-white font-bold text-xs transition-all cursor-pointer flex items-center gap-2"
        >
          <Heart class="w-4 h-4 text-rose-400" />
          <span>Список бажань</span>
        </button>
      </div>
    </div>
  {/if}

</div>
