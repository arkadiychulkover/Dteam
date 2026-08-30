<script lang="ts">
  import { gamesStore } from '../../stores/gamesStore';
  import { uiStore } from '../../stores/uiStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { cartStore } from '../../stores/cartStore';
  import { formatPrice, formatBasePrice } from '../../utils/formatters';
  import { ChevronLeft, ChevronRight, Heart, ShoppingBag, ShoppingCart } from 'lucide-svelte';

  let currentIndex = $state(0);

  const allGames = $derived($gamesStore.games);
  const activeGame = $derived(allGames[currentIndex] || allGames[0]);
  const isHeroWishlisted = $derived(activeGame ? $wishlistStore.wishlistGameIds.has(activeGame.id) : false);

  function nextSlide() {
    if (allGames.length === 0) return;
    currentIndex = (currentIndex + 1) % allGames.length;
  }

  function prevSlide() {
    if (allGames.length === 0) return;
    currentIndex = (currentIndex - 1 + allGames.length) % allGames.length;
  }

  function handleHeroWishlistToggle(e: MouseEvent) {
    e.stopPropagation();
    if (activeGame) {
      wishlistStore.toggleWishlist(activeGame);
    }
  }
</script>

{#if activeGame}
  <div class="space-y-4">
    <div class="flex items-center justify-end gap-3">
      <div class="flex items-center gap-4 text-xs font-semibold text-slate-300">
        <button
          onclick={() => uiStore.setTab('catalog')}
          class="hover:text-cyan-300 transition-colors cursor-pointer"
        >
          Каталог
        </button>
        <button
          onclick={() => uiStore.addToast({ title: 'Новини', message: 'Розділ оновлень та новин платформи.', type: 'info' })}
          class="hover:text-cyan-300 transition-colors cursor-pointer"
        >
          Новини
        </button>

        <div class="flex items-center gap-2 border-l border-cyan-900/60 pl-3">
          <button
            onclick={() => uiStore.setTab('wishlist')}
            class="relative p-2 rounded-xl bg-[#061820] hover:bg-cyan-950/70 border border-cyan-500/20 text-slate-300 hover:text-rose-400 transition-colors cursor-pointer"
            title="Список бажань"
          >
            <Heart class="w-4 h-4 {$wishlistStore.items.length > 0 ? 'fill-rose-500 text-rose-500' : ''}" />
            {#if $wishlistStore.items.length > 0}
              <span class="absolute -top-1.5 -right-1.5 min-w-[16px] h-[16px] px-0.5 rounded-full bg-rose-600 text-white text-[9px] font-bold flex items-center justify-center">
                {$wishlistStore.items.length}
              </span>
            {/if}
          </button>
          <button
            onclick={() => uiStore.addToast({ title: 'Кошик', message: 'Кошик порожній.', type: 'info' })}
            class="p-2 rounded-xl bg-[#061820] hover:bg-cyan-950/70 border border-cyan-500/20 text-slate-300 hover:text-cyan-300 transition-colors cursor-pointer"
            title="Кошик"
          >
            <ShoppingBag class="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>

    <div
      role="button"
      tabindex="0"
      onclick={() => { gamesStore.selectGame(activeGame); uiStore.setTab('game'); }}
      onkeydown={(e) => { if (e.key === 'Enter') { gamesStore.selectGame(activeGame); uiStore.setTab('game'); } }}
      class="group relative w-full aspect-[16/7] min-h-[300px] rounded-3xl overflow-hidden border border-cyan-500/30 shadow-2xl shadow-cyan-950/50 cursor-pointer text-left bg-slate-950"
    >
      <img
        src={activeGame.headerImageUrl || activeGame.coverImageUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1600&auto=format&fit=crop&q=80'}
        alt={activeGame.title}
        class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700"
      />

      <div class="absolute inset-0 bg-gradient-to-t from-[#020b0f] via-[#020b0f]/30 to-transparent"></div>
      <div class="absolute inset-0 bg-gradient-to-r from-[#020b0f]/80 via-transparent to-[#020b0f]/80"></div>

      {#if allGames.length > 1}
        <button
          type="button"
          onclick={(e) => {
            e.stopPropagation();
            prevSlide();
          }}
          class="absolute left-3 sm:left-4 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-black/60 hover:bg-cyan-500 text-white hover:text-black border border-white/20 flex items-center justify-center backdrop-blur-md transition-all z-20 cursor-pointer shadow-lg"
          aria-label="Previous game"
        >
          <ChevronLeft class="w-5 h-5" />
        </button>

        <button
          type="button"
          onclick={(e) => {
            e.stopPropagation();
            nextSlide();
          }}
          class="absolute right-3 sm:right-4 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-black/60 hover:bg-cyan-500 text-white hover:text-black border border-white/20 flex items-center justify-center backdrop-blur-md transition-all z-20 cursor-pointer shadow-lg"
          aria-label="Next game"
        >
          <ChevronRight class="w-5 h-5" />
        </button>
      {/if}

      <div class="absolute bottom-4 left-4 right-4 sm:bottom-6 sm:left-6 sm:right-6 flex flex-col sm:flex-row sm:items-end justify-between gap-4 z-10">
        <div>
          <div class="flex items-center gap-2.5">
            {#if (activeGame.discountPercentage || 0) > 0}
              <span class="px-2 py-0.5 rounded bg-rose-600 text-white font-extrabold text-xs">
                -{activeGame.discountPercentage}%
              </span>
            {/if}

            <span class="text-xl sm:text-2xl font-black text-white font-mono tracking-tight">
              {formatPrice(activeGame.priceInNanoTons, activeGame.discountPercentage)}
            </span>

            {#if (activeGame.discountPercentage || 0) > 0}
              <span class="text-xs sm:text-sm text-slate-400 line-through font-mono">
                {formatBasePrice(activeGame.priceInNanoTons)}
              </span>
            {/if}

            <button
              type="button"
              onclick={handleHeroWishlistToggle}
              class="p-2 rounded-xl bg-black/60 hover:bg-rose-950/80 border transition-all cursor-pointer backdrop-blur-md
                {isHeroWishlisted ? 'border-rose-500/80 text-rose-500' : 'border-white/20 text-slate-300 hover:text-rose-400'}"
              title={isHeroWishlisted ? 'Видалити зі списку бажань' : 'Додати до списку бажань'}
            >
              <Heart class="w-4 h-4 {isHeroWishlisted ? 'fill-rose-500' : ''}" />
            </button>
          </div>

          <span class="text-[10px] text-slate-400 mt-1 block">
            {(activeGame.discountPercentage || 0) > 0 ? 'Знижка діє обмежений час' : 'Офіційний реліз у Dteam'}
          </span>
        </div>

        <div class="sm:text-right max-w-md">
          <h2 class="text-lg sm:text-2xl font-extrabold text-white group-hover:text-cyan-300 transition-colors drop-shadow-md">
            {activeGame.title}
          </h2>
          <p class="text-xs text-slate-300 line-clamp-2 mt-1 leading-relaxed drop-shadow">
            {activeGame.shortDescription || activeGame.description}
          </p>
        </div>
      </div>
    </div>

    {#if allGames.length > 1}
      <div class="flex items-center gap-2.5 overflow-x-auto py-1 scrollbar-none justify-start sm:justify-center">
        {#each allGames.slice(0, 10) as game, idx}
          <button
            type="button"
            onclick={() => currentIndex = idx}
            class="relative w-16 sm:w-20 aspect-[16/9] rounded-xl overflow-hidden border-2 transition-all cursor-pointer shrink-0
              {currentIndex === idx ? 'border-cyan-400 scale-105 shadow-lg shadow-cyan-500/30' : 'border-transparent opacity-60 hover:opacity-100'}"
          >
            <img
              src={game.headerImageUrl || game.coverImageUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=200'}
              alt={game.title}
              class="w-full h-full object-cover"
            />
          </button>
        {/each}
      </div>
    {/if}
  </div>
{/if}

