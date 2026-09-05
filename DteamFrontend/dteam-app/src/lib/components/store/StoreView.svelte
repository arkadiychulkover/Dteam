<script lang="ts">
  import { onMount } from 'svelte';
  import { gamesStore } from '../../stores/gamesStore';
  import { uiStore } from '../../stores/uiStore';
  import { formatPrice, formatBasePrice, getEffectivePrice } from '../../utils/formatters';
  import type { Game } from '../../types';
  import FeaturedCarousel from './FeaturedCarousel.svelte';
  import BackendImage from '../ui/BackendImage.svelte';
  import { ChevronRight, ChevronLeft, Gift, Loader2, RefreshCw } from 'lucide-svelte';
  import { recommendationService } from '../../services/recommendationService';

  const allGames = $derived($gamesStore.games);
  const isLoading = $derived($gamesStore.isLoading);

  let specialOffersIndex = $state(0);
  const discountedGames = $derived(allGames.filter(g => (g.discountPercentage || 0) > 0));
  const specialOffersList = $derived(discountedGames.length > 0 ? discountedGames : allGames);
  const visibleSpecialOffers = $derived(specialOffersList.slice(specialOffersIndex, specialOffersIndex + 3));

  let recommendedGames = $state<Game[]>([]);
  let recommendedIndex = $state(0);
  let recommendedLoading = $state(true);
  const visibleRecommended = $derived(
    (recommendedGames.length > 0 ? recommendedGames : allGames).slice(recommendedIndex, recommendedIndex + 4)
  );

  let budgetIndex = $state(0);
  const budgetGames = $derived(allGames.filter(g => getEffectivePrice(g.priceInNanoTons, g.discountPercentage) <= 1.0));
  const visibleBudget = $derived((budgetGames.length > 0 ? budgetGames : allGames).slice(budgetIndex, budgetIndex + 4));

  const topSellers = $derived([...allGames].sort((a, b) => b.downloadCount - a.downloadCount).slice(0, 3));
  const newReleases = $derived([...allGames].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()).slice(0, 3));
  const freeGames = $derived(allGames.filter(g => Number(g.priceInNanoTons) === 0 || g.discountPercentage === 100).slice(0, 3));

  function goToCatalog(filters?: any) {
    if (filters) {
      gamesStore.setFilters(filters);
    }
    uiStore.setTab('catalog');
  }

  function openGame(game: Game) {
    recommendationService.track(game.id, 'ViewGame');
    gamesStore.selectGame(game);
    uiStore.setTab('game');
  }

  function calculateOriginalPrice(game: Game): string {
    return formatBasePrice(game.priceInNanoTons);
  }

  async function loadRecommended() {
    try {
      recommendedGames = await recommendationService.getRecommended(24, 0);
    } catch (e) {
      console.warn('Не вдалося завантажити рекомендації:', e);
      recommendedGames = [];
    } finally {
      recommendedLoading = false;
    }
  }

  onMount(() => {
    gamesStore.loadGames();
    loadRecommended();
  });
</script>

<div class="max-w-7xl mx-auto px-4 lg:px-8 py-6 space-y-12 animate-in fade-in">
  {#if isLoading && allGames.length === 0}
    <div class="flex flex-col items-center justify-center py-24 space-y-4">
      <Loader2 class="w-10 h-10 text-cyan-400 animate-spin" />
      <p class="text-sm font-bold text-slate-300">Завантаження крамниці Dteam...</p>
    </div>
  {:else if allGames.length === 0}
    <div class="flex flex-col items-center justify-center py-20 bg-[#061820]/60 rounded-3xl border border-cyan-500/20 text-center p-8 space-y-4 shadow-xl">
      <Gift class="w-12 h-12 text-cyan-500/40" />
      <h3 class="text-lg font-black text-white">Ігор поки що немає або сервер недоступний</h3>
      <p class="text-xs text-slate-400 max-w-md">Перевірте з'єднання з бэкендом або натисніть кнопку нижче для повторного завантаження.</p>
      <button
        onclick={() => gamesStore.loadGames()}
        class="inline-flex items-center gap-2 px-5 py-2.5 rounded-2xl bg-gradient-to-r from-cyan-500 to-emerald-500 text-black font-black text-xs shadow-lg shadow-cyan-500/20 hover:from-cyan-400 hover:to-emerald-400 transition-all cursor-pointer"
      >
        <RefreshCw class="w-4 h-4" />
        <span>Оновити крамницю</span>
      </button>
    </div>
  {:else}
    <FeaturedCarousel />

    {#if specialOffersList.length > 0}
      <section class="space-y-4">
        <div class="flex items-center justify-between">
          <button
          onclick={() => goToCatalog({ isDiscounted: true })}
          class="group flex items-center gap-2 text-lg sm:text-xl font-display font-extrabold text-white hover:text-cyan-300 transition-colors cursor-pointer"
        >
          <span>Особливі пропозиції</span>
          <ChevronRight class="w-4 h-4 text-cyan-400 group-hover:translate-x-1 transition-transform" />
        </button>
      </div>

      <div class="relative">
        {#if specialOffersList.length > 3}
          <button
            onclick={() => specialOffersIndex = Math.max(0, specialOffersIndex - 1)}
            disabled={specialOffersIndex === 0}
            class="hidden md:flex absolute -left-5 top-1/2 -translate-y-1/2 w-8 h-8 rounded-full bg-black/70 hover:bg-cyan-500 hover:text-black text-white border border-white/20 items-center justify-center transition-all disabled:opacity-30 disabled:pointer-events-none z-10 cursor-pointer shadow-lg"
            aria-label="Previous offers"
          >
            <ChevronLeft class="w-4 h-4" />
          </button>
          <button
            onclick={() => specialOffersIndex = Math.min(specialOffersList.length - 3, specialOffersIndex + 1)}
            disabled={specialOffersIndex >= specialOffersList.length - 3}
            class="hidden md:flex absolute -right-5 top-1/2 -translate-y-1/2 w-8 h-8 rounded-full bg-black/70 hover:bg-cyan-500 hover:text-black text-white border border-white/20 items-center justify-center transition-all disabled:opacity-30 disabled:pointer-events-none z-10 cursor-pointer shadow-lg"
            aria-label="Next offers"
          >
            <ChevronRight class="w-4 h-4" />
          </button>
        {/if}

        <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
          {#each visibleSpecialOffers as game (game.id)}
            <div
              role="button"
              tabindex="0"
              onclick={() => openGame(game)}
              onkeydown={(e) => e.key === 'Enter' && openGame(game)}
              class="group flex flex-col rounded-2xl bg-[#061d26] hover:bg-[#082733] border border-[#0d3b4b] hover:border-cyan-400/80 overflow-hidden shadow-xl transition-all duration-300 cursor-pointer transform hover:-translate-y-1"
            >
              <div class="relative aspect-[16/9] w-full overflow-hidden bg-slate-950">
                <img
                  src={game.headerImageUrl || game.coverImageUrl}
                  alt={game.title}
                  class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                />
              </div>

              <div class="p-3.5 space-y-1.5 flex-1 flex flex-col justify-between">
                <h3 class="font-bold text-sm text-white group-hover:text-cyan-300 transition-colors truncate">
                  {game.title}
                </h3>

                <div class="flex items-center gap-2">
                  {#if (game.discountPercentage || 0) > 0}
                    <span class="px-1.5 py-0.5 rounded bg-rose-600 text-white font-extrabold text-[10px]">
                      -{game.discountPercentage}%
                    </span>
                  {/if}

                  <span class="text-xs font-bold text-slate-200 font-mono">
                    {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                  </span>

                  {#if (game.discountPercentage || 0) > 0}
                    <span class="text-[10px] text-slate-500 line-through font-mono">
                      {calculateOriginalPrice(game)}
                    </span>
                  {/if}
                </div>
              </div>
            </div>
          {/each}
        </div>
      </div>
    </section>
  {/if}

  {#if recommendedGames.length > 0 || allGames.length > 0}
    {@const listForRecommended = recommendedGames.length > 0 ? recommendedGames : allGames}
    <section class="space-y-4">
      <div class="flex items-center justify-between">
        <button
          onclick={() => goToCatalog()}
          class="group flex items-center gap-2 text-lg sm:text-xl font-display font-extrabold text-white hover:text-cyan-300 transition-colors cursor-pointer"
        >
          <span>Рекомендовані вам</span>
          <ChevronRight class="w-4 h-4 text-cyan-400 group-hover:translate-x-1 transition-transform" />
        </button>
      </div>

      <div class="relative">
        {#if listForRecommended.length > 4}
          <button
            onclick={() => recommendedIndex = Math.max(0, recommendedIndex - 1)}
            disabled={recommendedIndex === 0}
            class="hidden md:flex absolute -left-5 top-1/2 -translate-y-1/2 w-8 h-8 rounded-full bg-black/70 hover:bg-cyan-500 hover:text-black text-white border border-white/20 items-center justify-center transition-all disabled:opacity-30 disabled:pointer-events-none z-10 cursor-pointer shadow-lg"
            aria-label="Previous recommended"
          >
            <ChevronLeft class="w-4 h-4" />
          </button>
          <button
            onclick={() => recommendedIndex = Math.min(listForRecommended.length - 4, recommendedIndex + 1)}
            disabled={recommendedIndex >= listForRecommended.length - 4}
            class="hidden md:flex absolute -right-5 top-1/2 -translate-y-1/2 w-8 h-8 rounded-full bg-black/70 hover:bg-cyan-500 hover:text-black text-white border border-white/20 items-center justify-center transition-all disabled:opacity-30 disabled:pointer-events-none z-10 cursor-pointer shadow-lg"
            aria-label="Next recommended"
          >
            <ChevronRight class="w-4 h-4" />
          </button>
        {/if}

        <div class="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-4 gap-5">
          {#each visibleRecommended as game (game.id)}
            <div
              role="button"
              tabindex="0"
              onclick={() => openGame(game)}
              onkeydown={(e) => e.key === 'Enter' && openGame(game)}
              class="group flex flex-col rounded-2xl bg-[#061d26] hover:bg-[#082733] border border-[#0d3b4b] hover:border-cyan-400/80 overflow-hidden shadow-xl transition-all duration-300 cursor-pointer transform hover:-translate-y-1"
            >
              <div class="relative aspect-[3/4] w-full overflow-hidden bg-slate-950">
                <BackendImage
                  src={game.coverImageUrl || game.headerImageUrl}
                  alt={game.title}
                  class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                />
              </div>

              <div class="p-3.5 space-y-1 flex-1 flex flex-col justify-between">
                <h3 class="font-bold text-xs sm:text-sm text-white group-hover:text-cyan-300 transition-colors truncate">
                  {game.title}
                </h3>

                <div class="flex items-center gap-2">
                  {#if (game.discountPercentage || 0) > 0}
                    <span class="px-1.5 py-0.5 rounded bg-rose-600 text-white font-extrabold text-[10px]">
                      -{game.discountPercentage}%
                    </span>
                  {/if}

                  <span class="text-xs font-bold text-slate-200 font-mono">
                    {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                  </span>

                  {#if (game.discountPercentage || 0) > 0}
                    <span class="text-[10px] text-slate-500 line-through font-mono">
                      {calculateOriginalPrice(game)}
                    </span>
                  {/if}
                </div>
              </div>
            </div>
          {/each}
        </div>
      </div>
    </section>
  {/if}

  {#if visibleBudget.length > 0}
    <section class="space-y-4">
      <div class="flex items-center justify-between">
        <button
          onclick={() => goToCatalog({ priceFilter: 'under1' })}
          class="group flex items-center gap-2 text-lg sm:text-xl font-display font-extrabold text-white hover:text-cyan-300 transition-colors cursor-pointer"
        >
          <span>До 1 TON</span>
          <ChevronRight class="w-4 h-4 text-cyan-400 group-hover:translate-x-1 transition-transform" />
        </button>
      </div>

      <div class="relative">
        {#if budgetGames.length > 4}
          <button
            onclick={() => budgetIndex = Math.max(0, budgetIndex - 1)}
            disabled={budgetIndex === 0}
            class="hidden md:flex absolute -left-5 top-1/2 -translate-y-1/2 w-8 h-8 rounded-full bg-black/70 hover:bg-cyan-500 hover:text-black text-white border border-white/20 items-center justify-center transition-all disabled:opacity-30 disabled:pointer-events-none z-10 cursor-pointer shadow-lg"
            aria-label="Previous budget games"
          >
            <ChevronLeft class="w-4 h-4" />
          </button>
          <button
            onclick={() => budgetIndex = Math.min(budgetGames.length - 4, budgetIndex + 1)}
            disabled={budgetIndex >= budgetGames.length - 4}
            class="hidden md:flex absolute -right-5 top-1/2 -translate-y-1/2 w-8 h-8 rounded-full bg-black/70 hover:bg-cyan-500 hover:text-black text-white border border-white/20 items-center justify-center transition-all disabled:opacity-30 disabled:pointer-events-none z-10 cursor-pointer shadow-lg"
            aria-label="Next budget games"
          >
            <ChevronRight class="w-4 h-4" />
          </button>
        {/if}

        <div class="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-4 gap-5">
          {#each visibleBudget as game (game.id)}
            <div
              role="button"
              tabindex="0"
              onclick={() => openGame(game)}
              onkeydown={(e) => e.key === 'Enter' && openGame(game)}
              class="group flex flex-col rounded-2xl bg-[#061d26] hover:bg-[#082733] border border-[#0d3b4b] hover:border-cyan-400/80 overflow-hidden shadow-xl transition-all duration-300 cursor-pointer transform hover:-translate-y-1"
            >
              <div class="relative aspect-[3/4] w-full overflow-hidden bg-slate-950">
                <img
                  src={game.coverImageUrl || game.headerImageUrl}
                  alt={game.title}
                  class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                />
              </div>

              <div class="p-3.5 space-y-1 flex-1 flex flex-col justify-between">
                <h3 class="font-bold text-xs sm:text-sm text-white group-hover:text-cyan-300 transition-colors truncate">
                  {game.title}
                </h3>

                <div class="flex items-center gap-2">
                  {#if (game.discountPercentage || 0) > 0}
                    <span class="px-1.5 py-0.5 rounded bg-rose-600 text-white font-extrabold text-[10px]">
                      -{game.discountPercentage}%
                    </span>
                  {/if}

                  <span class="text-xs font-bold text-slate-200 font-mono">
                    {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                  </span>

                  {#if (game.discountPercentage || 0) > 0}
                    <span class="text-[10px] text-slate-500 line-through font-mono">
                      {calculateOriginalPrice(game)}
                    </span>
                  {/if}
                </div>
              </div>
            </div>
          {/each}
        </div>
      </div>
    </section>
  {/if}

  {#if allGames.length > 0}
    <section class="space-y-4">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
        <div class="space-y-4">
          <button
            onclick={() => goToCatalog({ sortBy: 'relevance' })}
            class="group flex items-center gap-2 text-base font-display font-extrabold text-white hover:text-cyan-300 transition-colors cursor-pointer"
          >
            <span>Хіти продажу</span>
            <ChevronRight class="w-4 h-4 text-cyan-400 group-hover:translate-x-1 transition-transform" />
          </button>

          <div class="space-y-4">
            {#each topSellers as game (game.id)}
              <div
                role="button"
                tabindex="0"
                onclick={() => openGame(game)}
                onkeydown={(e) => e.key === 'Enter' && openGame(game)}
                class="group flex flex-col rounded-2xl bg-[#061d26] hover:bg-[#082733] border border-[#0d3b4b] hover:border-cyan-400/80 overflow-hidden shadow-lg transition-all duration-300 cursor-pointer transform hover:-translate-y-1"
              >
                <div class="relative aspect-[16/9] w-full overflow-hidden bg-slate-950">
                  <img src={game.headerImageUrl || game.coverImageUrl} alt={game.title} class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
                </div>
                <div class="p-3 space-y-1">
                  <h4 class="font-bold text-xs sm:text-sm text-white group-hover:text-cyan-300 transition-colors truncate">
                    {game.title}
                  </h4>
                  <div class="flex items-center gap-2">
                    {#if (game.discountPercentage || 0) > 0}
                      <span class="px-1.5 py-0.5 rounded bg-rose-600 text-white font-extrabold text-[10px]">
                        -{game.discountPercentage}%
                      </span>
                    {/if}
                    <span class="text-xs font-bold text-slate-200 font-mono">
                      {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                    </span>
                    {#if (game.discountPercentage || 0) > 0}
                      <span class="text-[10px] text-slate-500 line-through font-mono">
                        {calculateOriginalPrice(game)}
                      </span>
                    {/if}
                  </div>
                </div>
              </div>
            {/each}
          </div>
        </div>

        <div class="space-y-4">
          <button
            onclick={() => goToCatalog({ sortBy: 'newest' })}
            class="group flex items-center gap-2 text-base font-display font-extrabold text-white hover:text-cyan-300 transition-colors cursor-pointer"
          >
            <span>Нові релізи</span>
            <ChevronRight class="w-4 h-4 text-cyan-400 group-hover:translate-x-1 transition-transform" />
          </button>

          <div class="space-y-4">
            {#each newReleases as game (game.id)}
              <div
                role="button"
                tabindex="0"
                onclick={() => openGame(game)}
                onkeydown={(e) => e.key === 'Enter' && openGame(game)}
                class="group flex flex-col rounded-2xl bg-[#061d26] hover:bg-[#082733] border border-[#0d3b4b] hover:border-cyan-400/80 overflow-hidden shadow-lg transition-all duration-300 cursor-pointer transform hover:-translate-y-1"
              >
                <div class="relative aspect-[16/9] w-full overflow-hidden bg-slate-950">
                  <img src={game.headerImageUrl || game.coverImageUrl} alt={game.title} class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
                </div>
                <div class="p-3 space-y-1">
                  <h4 class="font-bold text-xs sm:text-sm text-white group-hover:text-cyan-300 transition-colors truncate">
                    {game.title}
                  </h4>
                  <div class="flex items-center gap-2">
                    {#if (game.discountPercentage || 0) > 0}
                      <span class="px-1.5 py-0.5 rounded bg-rose-600 text-white font-extrabold text-[10px]">
                        -{game.discountPercentage}%
                      </span>
                    {/if}
                    <span class="text-xs font-bold text-slate-200 font-mono">
                      {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                    </span>
                    {#if (game.discountPercentage || 0) > 0}
                      <span class="text-[10px] text-slate-500 line-through font-mono">
                        {calculateOriginalPrice(game)}
                      </span>
                    {/if}
                  </div>
                </div>
              </div>
            {/each}
          </div>
        </div>

        <div class="space-y-4">
          <button
            onclick={() => goToCatalog({ priceFilter: 'free' })}
            class="group flex items-center gap-2 text-base font-display font-extrabold text-white hover:text-cyan-300 transition-colors cursor-pointer"
          >
            <span>Безкоштовні</span>
            <ChevronRight class="w-4 h-4 text-cyan-400 group-hover:translate-x-1 transition-transform" />
          </button>

          {#if freeGames.length > 0}
            <div class="space-y-4">
              {#each freeGames as game (game.id)}
                <div
                  role="button"
                  tabindex="0"
                  onclick={() => openGame(game)}
                  onkeydown={(e) => e.key === 'Enter' && openGame(game)}
                  class="group flex flex-col rounded-2xl bg-[#061d26] hover:bg-[#082733] border border-[#0d3b4b] hover:border-cyan-400/80 overflow-hidden shadow-lg transition-all duration-300 cursor-pointer transform hover:-translate-y-1"
                >
                  <div class="relative aspect-[16/9] w-full overflow-hidden bg-slate-950">
                    <img src={game.headerImageUrl || game.coverImageUrl} alt={game.title} class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
                  </div>
                  <div class="p-3 space-y-1">
                    <h4 class="font-bold text-xs sm:text-sm text-white group-hover:text-cyan-300 transition-colors truncate">
                      {game.title}
                    </h4>
                    <div class="flex items-center gap-2">
                      {#if (game.discountPercentage || 0) === 100}
                        <span class="px-1.5 py-0.5 rounded bg-rose-600 text-white font-extrabold text-[10px]">
                          -100%
                        </span>
                      {/if}
                      <span class="text-xs font-bold text-cyan-400 font-mono">
                        Безкоштовно
                      </span>
                      {#if (game.discountPercentage || 0) === 100}
                        <span class="text-[10px] text-slate-500 line-through font-mono">
                          {calculateOriginalPrice(game)}
                        </span>
                      {/if}
                    </div>
                  </div>
                </div>
              {/each}
            </div>
          {:else}
            <div class="p-6 rounded-2xl bg-[#061d26]/80 border border-[#0d3b4b] text-center space-y-2">
              <Gift class="w-8 h-8 text-cyan-400/40 mx-auto" />
              <h5 class="text-xs font-bold text-white">Немає безкоштовних ігор</h5>
              <p class="text-[11px] text-slate-400">
                Наразі всі ігри платні. Ви можете додати безкоштовну гру через адмін-панель зі ціною 0 TON.
              </p>
            </div>
          {/if}
        </div>
      </div>
    </section>
  {/if}
{/if}
</div>
