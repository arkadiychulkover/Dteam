<script lang="ts">
import { onMount } from 'svelte';
  import { gamesStore } from '../../stores/gamesStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { cartStore } from '../../stores/cartStore';
  import { uiStore } from '../../stores/uiStore';
  import { formatPrice, formatBasePrice } from '../../utils/formatters';
  import type { Game, GameRecommendation } from '../../types';
  import { gamesService } from '../../services/gamesService';
  import GameRecommendationsDropdown from '../ui/GameRecommendationsDropdown.svelte';
  import GameDetailsModal from './GameDetailsModal.svelte';
  import {
    Search,
    SlidersHorizontal,
    LayoutGrid,
    List,
    ChevronDown,
    ChevronUp,
    Star,
    Sparkles,
    RotateCcw,
    X,
    Check,
    Layers,
    Monitor,
    Apple,
    Gamepad2,
    Heart,
  } from 'lucide-svelte';

  let isGenreOpen = $state(true);
  let isPriceOpen = $state(true);
  let isTypeOpen = $state(true);
  let isPlatformOpen = $state(false);
  let isFeatureOpen = $state(false);

  let searchInput = $state($gamesStore.filters.search);
  let tagInput = $state($gamesStore.filters.tagQuery);
  let catalogSearchWrapperEl = $state<HTMLElement | null>(null);
  let catalogDebounceTimer: ReturnType<typeof setTimeout> | null = null;
  let catalogRecommendations = $state<GameRecommendation[]>([]);
  let isCatalogLoadingRecs = $state(false);
  let isCatalogRecsOpen = $state(false);
  let isMobileFiltersOpen = $state(false);

  const genres = [
    'All Games', 'Action', 'RPG', 'Strategy', 'Adventure',
    'Shooter', 'Indie', 'Simulation', 'MMO', 'Horror', 'Sports', 'Racing', 'Cyberpunk', 'SciFi'
  ];

  const priceOptions: { id: 'all' | 'free' | 'under1' | 'under3' | 'under6' | 'under9'; label: string }[] = [
    { id: 'free', label: 'Безкоштовно' },
    { id: 'under1', label: 'До 1 TON' },
    { id: 'under3', label: 'До 3 TON' },
    { id: 'under6', label: 'До 6 TON' },
    { id: 'under9', label: 'До 9 TON' },
    { id: 'all', label: 'Без обмежень' },
  ];

  const sortOptions = [
    { id: 'relevance', label: 'За релевантністю' },
    { id: 'price_asc', label: 'Ціна: від найнижчої' },
    { id: 'price_desc', label: 'Ціна: від найвищої' },
    { id: 'rating', label: 'За рейтингом' },
    { id: 'newest', label: 'Новинки' },
  ];

  function applyFilter(changes: any) {
    gamesStore.setFilters(changes);
    gamesStore.loadCatalogGames();
  }

  function handleSearchSubmit(e?: Event) {
    if (e) e.preventDefault();
    if (catalogDebounceTimer) clearTimeout(catalogDebounceTimer);
    isCatalogRecsOpen = false;
    applyFilter({ search: searchInput });
  }

  function handleCatalogSearchInput(e: Event) {
    const query = (e.target as HTMLInputElement).value;
    searchInput = query;

    if (catalogDebounceTimer) {
      clearTimeout(catalogDebounceTimer);
    }

    if (!query.trim()) {
      catalogRecommendations = [];
      isCatalogRecsOpen = false;
      isCatalogLoadingRecs = false;
      return;
    }

    catalogDebounceTimer = setTimeout(async () => {
      isCatalogLoadingRecs = true;
      isCatalogRecsOpen = true;
      try {
        const res = await gamesService.getRecommendations(query.trim(), 8);
        if (res && res.length > 0) {
          catalogRecommendations = res;
        } else {
          const lower = query.trim().toLowerCase();
          catalogRecommendations = $gamesStore.games
            .filter(g => g.title.toLowerCase().startsWith(lower))
            .slice(0, 8)
            .map(g => ({
              id: g.id,
              title: g.title,
              banner: g.headerImageUrl || g.coverImageUrl || '',
              bannerUrl: g.headerImageUrl || g.coverImageUrl || '',
              headerImageUrl: g.headerImageUrl,
              coverImageUrl: g.coverImageUrl
            }));
        }
      } catch {
        const lower = query.trim().toLowerCase();
        catalogRecommendations = $gamesStore.games
          .filter(g => g.title.toLowerCase().startsWith(lower))
          .slice(0, 8)
          .map(g => ({
            id: g.id,
            title: g.title,
            banner: g.headerImageUrl || g.coverImageUrl || '',
            bannerUrl: g.headerImageUrl || g.coverImageUrl || '',
            headerImageUrl: g.headerImageUrl,
            coverImageUrl: g.coverImageUrl
          }));
      } finally {
        isCatalogLoadingRecs = false;
      }
    }, 500);
  }

  async function handleSelectCatalogRecommendation(rec: GameRecommendation) {
    if (catalogDebounceTimer) clearTimeout(catalogDebounceTimer);
    isCatalogRecsOpen = false;
    searchInput = rec.title;
    try {
      const fullGame = await gamesService.getGameById(rec.id);
      openGame(fullGame);
    } catch {
      const cached = $gamesStore.games.find(g => g.id === rec.id);
      if (cached) {
        openGame(cached);
      } else {
        applyFilter({ search: rec.title });
      }
    }
  }

  function handleCatalogClickOutside(e: MouseEvent) {
    if (catalogSearchWrapperEl && !catalogSearchWrapperEl.contains(e.target as Node)) {
      isCatalogRecsOpen = false;
    }
  }

  function handleTagSubmit(e?: Event) {
    if (e) e.preventDefault();
    applyFilter({ tagQuery: tagInput });
  }

  function handleReset() {
    searchInput = '';
    tagInput = '';
    isCatalogRecsOpen = false;
    gamesStore.resetFilters();
    gamesStore.loadCatalogGames();
  }

  $effect(() => {
    searchInput = $gamesStore.filters.search;
  });

  function calculateOriginalPrice(game: Game): string {
    return formatBasePrice(game.priceInNanoTons);
  }

  function openGame(game: Game) {
    gamesStore.selectGame(game);
    uiStore.setTab('game');
    if (typeof window !== 'undefined') {
      window.scrollTo({ top: 0, behavior: 'instant' });
    }
  }

  onMount(() => {
    gamesStore.loadCatalogGames();
  });
</script>

<svelte:window onclick={handleCatalogClickOutside} />

<div class="max-w-7xl mx-auto px-4 lg:px-8 py-6 animate-in fade-in">
  <div class="mb-6">
    <form onsubmit={handleSearchSubmit} class="relative flex items-center gap-3">
      <div bind:this={catalogSearchWrapperEl} class="relative flex-1">
        <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-cyan-400" />
        <input
          type="text"
          placeholder="Пошук у Крамниці Dteam..."
          bind:value={searchInput}
          oninput={handleCatalogSearchInput}
          onfocus={() => {
            if (searchInput.trim() && catalogRecommendations.length > 0) {
              isCatalogRecsOpen = true;
            }
          }}
          class="w-full pl-11 pr-10 py-3 rounded-2xl bg-[#061820]/95 border border-cyan-500/30 focus:border-cyan-400 focus:shadow-[0_0_20px_rgba(13,242,201,0.25)] focus:outline-none text-xs text-white placeholder-slate-400 shadow-inner transition-all"
        />
        {#if searchInput}
          <button
            type="button"
            onclick={() => { searchInput = ''; isCatalogRecsOpen = false; handleSearchSubmit(); }}
            class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white p-1 cursor-pointer"
          >
            <X class="w-3.5 h-3.5" />
          </button>
        {/if}

        <GameRecommendationsDropdown
          recommendations={catalogRecommendations}
          isOpen={isCatalogRecsOpen}
          isLoading={isCatalogLoadingRecs}
          searchQuery={searchInput}
          onSelect={handleSelectCatalogRecommendation}
          onViewAll={handleSearchSubmit}
        />
      </div>

      <button
        type="submit"
        class="px-5 py-3 rounded-2xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-bold text-xs shadow-lg shadow-cyan-500/25 transition-all cursor-pointer"
      >
        Знайти
      </button>
    </form>
  </div>

  <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-6 pb-4 border-b border-cyan-950/80">
    <div class="flex flex-wrap items-center gap-3">
      <!-- Mobile Filter Toggle Button -->
      <button
        type="button"
        onclick={() => isMobileFiltersOpen = !isMobileFiltersOpen}
        class="lg:hidden px-3 py-1.5 rounded-xl bg-[#061820] hover:bg-cyan-950/80 border border-cyan-500/30 text-cyan-300 font-bold text-xs flex items-center gap-1.5 transition-all cursor-pointer shadow-sm
          {isMobileFiltersOpen ? 'bg-cyan-500 text-black border-cyan-400' : ''}"
      >
        <SlidersHorizontal class="w-3.5 h-3.5" />
        <span>{isMobileFiltersOpen ? 'Сховати фільтри' : 'Фільтри'}</span>
      </button>

      <div class="flex items-center gap-2 text-xs">
        <span class="text-slate-400 font-medium">Сортування:</span>
        <select
          value={$gamesStore.filters.sortBy}
          onchange={(e) => applyFilter({ sortBy: (e.target as HTMLSelectElement).value })}
          class="bg-[#061820] text-cyan-300 font-bold border border-cyan-500/30 hover:border-cyan-400 rounded-xl px-3 py-1.5 focus:outline-none cursor-pointer"
        >
          {#each sortOptions as opt}
            <option value={opt.id}>{opt.label}</option>
          {/each}
        </select>
      </div>
    </div>

    <div class="flex items-center gap-2">
      <span class="text-xs text-slate-400">Вид:</span>
      <div class="flex items-center bg-[#061820] p-1 rounded-xl border border-cyan-500/20">
        <button
          onclick={() => applyFilter({ viewMode: 'grid' })}
          class="p-1.5 rounded-lg transition-colors cursor-pointer {$gamesStore.filters.viewMode === 'grid' ? 'bg-cyan-500 text-black font-bold' : 'text-slate-400 hover:text-white'}"
          title="Сетка (Grid)"
        >
          <LayoutGrid class="w-4 h-4" />
        </button>
        <button
          onclick={() => applyFilter({ viewMode: 'list' })}
          class="p-1.5 rounded-lg transition-colors cursor-pointer {$gamesStore.filters.viewMode === 'list' ? 'bg-cyan-500 text-black font-bold' : 'text-slate-400 hover:text-white'}"
          title="Список (List)"
        >
          <List class="w-4 h-4" />
        </button>
      </div>
    </div>
  </div>

  <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 lg:gap-8">
    <aside class="lg:col-span-3 space-y-4 {isMobileFiltersOpen ? 'block' : 'hidden lg:block'}">
      <div class="p-4 rounded-2xl bg-[#061820]/90 border border-cyan-500/20 shadow-lg space-y-4">
        <div class="flex items-center justify-between">
          <h3 class="text-sm font-extrabold text-white tracking-wide flex items-center gap-2">
            <SlidersHorizontal class="w-4 h-4 text-cyan-400" />
            <span>Фільтри</span>
          </h3>
          <button
            onclick={handleReset}
            class="text-xs text-cyan-400 hover:text-cyan-300 font-semibold flex items-center gap-1 transition-colors cursor-pointer"
          >
            <RotateCcw class="w-3 h-3" />
            <span>Скинути</span>
          </button>
        </div>

        <form onsubmit={handleTagSubmit} class="relative">
          <input
            type="text"
            placeholder="Пошук тегів..."
            bind:value={tagInput}
            class="w-full pl-3 pr-8 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-500"
          />
          {#if tagInput}
            <button
              type="button"
              onclick={() => { tagInput = ''; handleTagSubmit(); }}
              class="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white"
            >
              <X class="w-3.5 h-3.5" />
            </button>
          {/if}
        </form>

        <div class="border-t border-cyan-950/80 pt-3">
          <button
            type="button"
            onclick={() => isGenreOpen = !isGenreOpen}
            class="w-full flex items-center justify-between text-xs font-bold text-slate-200 hover:text-cyan-300 transition-colors cursor-pointer"
          >
            <span>Жанр</span>
            {#if isGenreOpen}
              <ChevronUp class="w-4 h-4 text-cyan-400" />
            {:else}
              <ChevronDown class="w-4 h-4 text-slate-500" />
            {/if}
          </button>

          {#if isGenreOpen}
            <div class="mt-2.5 space-y-1 max-h-48 overflow-y-auto pr-1">
              {#each genres as g}
                <button
                  type="button"
                  onclick={() => applyFilter({ genre: g })}
                  class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs transition-colors cursor-pointer flex items-center justify-between
                    {$gamesStore.filters.genre === g
                      ? 'bg-cyan-500/20 text-cyan-300 font-bold border border-cyan-500/40'
                      : 'text-slate-400 hover:text-white hover:bg-slate-800/40'}"
                >
                  <span>{g}</span>
                  {#if $gamesStore.filters.genre === g}
                    <Check class="w-3.5 h-3.5 text-cyan-400" />
                  {/if}
                </button>
              {/each}
            </div>
          {/if}
        </div>

        <div class="border-t border-cyan-950/80 pt-3">
          <button
            type="button"
            onclick={() => isPriceOpen = !isPriceOpen}
            class="w-full flex items-center justify-between text-xs font-bold text-slate-200 hover:text-cyan-300 transition-colors cursor-pointer"
          >
            <span>Ціна</span>
            {#if isPriceOpen}
              <ChevronUp class="w-4 h-4 text-cyan-400" />
            {:else}
              <ChevronDown class="w-4 h-4 text-slate-500" />
            {/if}
          </button>

          {#if isPriceOpen}
            <div class="mt-2.5 space-y-1.5">
              {#each priceOptions as opt}
                <label class="flex items-center gap-2 text-xs text-slate-300 hover:text-white cursor-pointer select-none">
                  <input
                    type="radio"
                    name="price-filter"
                    checked={$gamesStore.filters.priceFilter === opt.id}
                    onchange={() => applyFilter({ priceFilter: opt.id })}
                    class="accent-cyan-400 cursor-pointer"
                  />
                  <span>{opt.label}</span>
                </label>
              {/each}

              <div class="pt-2 border-t border-cyan-950/50 mt-2">
                <label class="flex items-center gap-2 text-xs text-rose-300 font-bold cursor-pointer select-none">
                  <input
                    type="checkbox"
                    checked={$gamesStore.filters.isDiscounted}
                    onchange={(e) => applyFilter({ isDiscounted: (e.target as HTMLInputElement).checked })}
                    class="accent-rose-500 cursor-pointer rounded"
                  />
                  <span>Знижки / Discounts</span>
                </label>
              </div>
            </div>
          {/if}
        </div>

        <div class="border-t border-cyan-950/80 pt-3">
          <button
            type="button"
            onclick={() => isTypeOpen = !isTypeOpen}
            class="w-full flex items-center justify-between text-xs font-bold text-slate-200 hover:text-cyan-300 transition-colors cursor-pointer"
          >
            <span>Тип</span>
            {#if isTypeOpen}
              <ChevronUp class="w-4 h-4 text-cyan-400" />
            {:else}
              <ChevronDown class="w-4 h-4 text-slate-500" />
            {/if}
          </button>

          {#if isTypeOpen}
            <div class="mt-2.5 space-y-1.5">
              <label class="flex items-center gap-2 text-xs text-slate-300 hover:text-white cursor-pointer">
                <input
                  type="radio"
                  name="type-filter"
                  checked={$gamesStore.filters.isDlc === undefined}
                  onchange={() => applyFilter({ isDlc: undefined })}
                  class="accent-cyan-400"
                />
                <span>Всі типи</span>
              </label>
              <label class="flex items-center gap-2 text-xs text-slate-300 hover:text-white cursor-pointer">
                <input
                  type="radio"
                  name="type-filter"
                  checked={$gamesStore.filters.isDlc === false}
                  onchange={() => applyFilter({ isDlc: false })}
                  class="accent-cyan-400"
                />
                <span>Базові ігри</span>
              </label>
              <label class="flex items-center gap-2 text-xs text-purple-300 font-semibold hover:text-white cursor-pointer">
                <input
                  type="radio"
                  name="type-filter"
                  checked={$gamesStore.filters.isDlc === true}
                  onchange={() => applyFilter({ isDlc: true })}
                  class="accent-purple-500"
                />
                <span>DLC / Доповнення</span>
              </label>
            </div>
          {/if}
        </div>

        <div class="border-t border-cyan-950/80 pt-3">
          <button
            type="button"
            onclick={() => isPlatformOpen = !isPlatformOpen}
            class="w-full flex items-center justify-between text-xs font-bold text-slate-200 hover:text-cyan-300 transition-colors cursor-pointer"
          >
            <span>Платформа</span>
            {#if isPlatformOpen}
              <ChevronUp class="w-4 h-4 text-cyan-400" />
            {:else}
              <ChevronDown class="w-4 h-4 text-slate-500" />
            {/if}
          </button>

          {#if isPlatformOpen}
            <div class="mt-2.5 space-y-1.5">
              {#each ['Windows', 'MacOS', 'Linux'] as p}
                <label class="flex items-center gap-2 text-xs text-slate-300 hover:text-white cursor-pointer">
                  <input
                    type="radio"
                    name="platform-filter"
                    checked={$gamesStore.filters.platform === p}
                    onchange={() => applyFilter({ platform: $gamesStore.filters.platform === p ? '' : p })}
                    class="accent-cyan-400"
                  />
                  <span>{p}</span>
                </label>
              {/each}
            </div>
          {/if}
        </div>
      </div>
    </aside>

    <main class="lg:col-span-9">
      {#if $gamesStore.catalogLoading}
        <div class="text-center py-24">
          <span class="animate-spin text-3xl inline-block text-cyan-400">⟳</span>
          <p class="text-xs text-slate-400 mt-2 font-medium">Завантаження ігор з бази даних...</p>
        </div>
      {:else if $gamesStore.catalogGames.length > 0}
        {#if $gamesStore.filters.viewMode === 'grid'}
          <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
            {#each $gamesStore.catalogGames as game (game.id)}
              {@const isWishlisted = $wishlistStore.wishlistGameIds.has(game.id)}
              <div
                role="button"
                tabindex="0"
                onclick={() => openGame(game)}
                onkeydown={(e) => e.key === 'Enter' && openGame(game)}
                class="group relative flex flex-col rounded-2xl bg-[#061820]/90 hover:bg-[#08202b] border border-cyan-500/20 hover:border-cyan-400/80 overflow-hidden shadow-lg hover:shadow-2xl hover:shadow-cyan-500/20 transition-all duration-300 transform hover:-translate-y-1.5 cursor-pointer text-left"
              >
                <div class="relative aspect-[3/4] w-full overflow-hidden bg-slate-950">
                  <img
                    src={game.coverImageUrl || game.headerImageUrl || 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=800'}
                    alt={game.title}
                    class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                  />
                  <div class="absolute inset-0 bg-gradient-to-t from-[#061820] via-transparent to-black/30"></div>

                  <div class="absolute top-3 right-3 flex items-center gap-1 px-2 py-0.5 rounded-lg bg-black/70 backdrop-blur-md border border-cyan-500/30 text-[10px] font-black text-cyan-300 shadow-md">
                    <Star class="w-3 h-3 fill-amber-400 text-amber-400" />
                    <span>{game.averageRating > 0 ? game.averageRating.toFixed(1) : '5.0'}</span>
                  </div>

                  <button
                    type="button"
                    onclick={(e) => {
                      e.stopPropagation();
                      wishlistStore.toggleWishlist(game);
                    }}
                    class="absolute top-3 left-3 p-1.5 rounded-xl bg-black/70 backdrop-blur-md border transition-all cursor-pointer shadow-md
                      {isWishlisted
                        ? 'border-rose-500/60 text-rose-500 hover:bg-rose-950/80'
                        : 'border-white/20 text-slate-300 hover:text-rose-400 hover:border-rose-400/60'}"
                    title={isWishlisted ? 'Видалити зі списку бажань' : 'Додати до списку бажань'}
                  >
                    <Heart class="w-3.5 h-3.5 {isWishlisted ? 'fill-rose-500' : ''}" />
                  </button>

                  {#if game.isDlc}
                    <div class="absolute top-11 left-3 px-2 py-0.5 rounded-lg bg-purple-950/80 backdrop-blur-md border border-purple-500/40 text-[9px] font-extrabold text-purple-300 uppercase tracking-wider">
                      DLC
                    </div>
                  {/if}

                  <div class="absolute bottom-3 left-3 right-3">
                    <h3 class="font-extrabold text-base text-white group-hover:text-cyan-300 transition-colors line-clamp-1">
                      {game.title}
                    </h3>

                    <div class="flex items-center gap-2 mt-1.5">
                      {#if (game.discountPercentage || 0) > 0}
                        <span class="px-1.5 py-0.5 rounded bg-rose-600 text-white font-extrabold text-[10px]">
                          -{game.discountPercentage}%
                        </span>
                      {/if}

                      <span class="text-sm font-black text-cyan-300 font-mono tracking-tight">
                        {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                      </span>

                      {#if (game.discountPercentage || 0) > 0}
                        <span class="text-xs text-slate-500 line-through font-mono">
                          {calculateOriginalPrice(game)}
                        </span>
                      {/if}
                    </div>
                  </div>
                </div>
              </div>
            {/each}
          </div>
        {:else}
          <div class="space-y-3">
            {#each $gamesStore.catalogGames as game (game.id)}
              {@const isWishlisted = $wishlistStore.wishlistGameIds.has(game.id)}
              <div
                role="button"
                tabindex="0"
                onclick={() => openGame(game)}
                onkeydown={(e) => e.key === 'Enter' && openGame(game)}
                class="group flex items-center justify-between p-2.5 sm:p-3 rounded-2xl bg-[#061820]/90 hover:bg-[#08232e] border border-cyan-500/20 hover:border-cyan-400/60 shadow-md hover:shadow-cyan-500/15 transition-all cursor-pointer"
              >
                <div class="flex items-center gap-4 flex-1 min-w-0 pr-4">
                  <div class="w-24 sm:w-36 aspect-[16/7] rounded-xl overflow-hidden bg-slate-950 shrink-0 border border-cyan-500/20">
                    <img
                      src={game.headerImageUrl || game.coverImageUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600'}
                      alt={game.title}
                      class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                    />
                  </div>

                  <div class="min-w-0">
                    <h4 class="text-sm sm:text-base font-extrabold text-white group-hover:text-cyan-300 transition-colors truncate">
                      {game.title}
                    </h4>
                    <div class="flex items-center gap-2 mt-1">
                      {#if game.isDlc}
                        <span class="text-[9px] px-1.5 py-0.2 rounded bg-purple-950 text-purple-300 border border-purple-500/40 uppercase font-bold">
                          DLC
                        </span>
                      {/if}
                      {#if game.genres && game.genres.length > 0}
                        <span class="text-[11px] text-slate-400 truncate">
                          {game.genres.slice(0, 3).join(', ')}
                        </span>
                      {/if}
                    </div>
                  </div>
                </div>

                <div class="flex items-center gap-3 shrink-0">
                  <button
                    type="button"
                    onclick={(e) => {
                      e.stopPropagation();
                      wishlistStore.toggleWishlist(game);
                    }}
                    class="p-2 rounded-xl bg-[#0a232c] hover:bg-rose-950/80 border transition-all cursor-pointer
                      {isWishlisted ? 'border-rose-500/80 text-rose-500' : 'border-cyan-500/20 text-slate-400 hover:text-rose-400'}"
                    title={isWishlisted ? 'Видалити зі списку бажань' : 'Додати до списку бажань'}
                  >
                    <Heart class="w-4 h-4 {isWishlisted ? 'fill-rose-500' : ''}" />
                  </button>

                  {#if (game.discountPercentage || 0) > 0}
                    <span class="px-2 py-0.5 rounded-lg bg-rose-600 text-white font-extrabold text-xs">
                      -{game.discountPercentage}%
                    </span>
                  {/if}

                  <div class="text-right min-w-[70px]">
                    <div class="text-sm sm:text-base font-black text-cyan-300 font-mono">
                      {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                    </div>
                    {#if (game.discountPercentage || 0) > 0}
                      <div class="text-[10px] text-slate-500 line-through font-mono">
                        {calculateOriginalPrice(game)}
                      </div>
                    {/if}
                  </div>
                </div>
              </div>
            {/each}
          </div>
        {/if}
      {:else}
        <div class="text-center py-24 bg-[#061820]/60 rounded-3xl border border-cyan-500/20 shadow-xl">
          <Gamepad2 class="w-16 h-16 text-cyan-500/40 mx-auto mb-4" />
          <h4 class="text-lg font-bold text-white">Ігор за даними фільтрами не знайдено</h4>
          <p class="text-xs text-slate-400 mt-1.5 max-w-sm mx-auto">
            Спробуйте скинути фільтри або змінити критерії пошуку за жанром та ціною.
          </p>
          <button
            onclick={handleReset}
            class="mt-5 px-5 py-2.5 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 text-black font-bold text-xs shadow-lg shadow-cyan-500/25 transition-all cursor-pointer"
          >
            Скинути всі фільтри
          </button>
        </div>
      {/if}
    </main>
  </div>
</div>

