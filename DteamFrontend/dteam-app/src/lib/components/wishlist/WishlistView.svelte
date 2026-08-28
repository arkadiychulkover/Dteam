<script lang="ts">
  import { onMount } from 'svelte';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { cartStore } from '../../stores/cartStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { uiStore } from '../../stores/uiStore';
  import { formatPrice, formatBasePrice } from '../../utils/formatters';
  import type { WishlistItem, Game } from '../../types';
  import GameDetailsModal from '../store/GameDetailsModal.svelte';
  import {
    Search,
    SlidersHorizontal,
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
    ShoppingCart,
    ShoppingBag,
    Trash2,
    Grid,
    List,
  } from 'lucide-svelte';

  let storeSearchQuery = $state('');
  let wishlistSearchQuery = $state('');
  let tagInput = $state('');

  type SortOption = 'discounts' | 'price_asc' | 'price_desc' | 'title_asc' | 'title_desc' | 'date_new';
  let sortBy = $state<SortOption>('discounts');
  let isSortDropdownOpen = $state(false);

  let isGenreOpen = $state(false);
  let isPriceOpen = $state(true);
  let isTypeOpen = $state(false);
  let isFeaturesOpen = $state(false);
  let isPlatformOpen = $state(false);
  let isEventsOpen = $state(false);

  let selectedGenre = $state<string>('All');
  let selectedPriceRange = $state<'all' | 'free' | 'under1' | 'under3' | 'under6' | 'under9'>('all');
  let isDiscountOnly = $state(false);
  let selectedType = $state<'all' | 'base' | 'dlc'>('all');
  let selectedPlatforms = $state<string[]>([]);
  let selectedFeatures = $state<string[]>([]);

  const genres = [
    'All', 'Action', 'RPG', 'Strategy', 'Adventure', 
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

  const platformOptions = ['Windows', 'MacOS', 'Linux'];
  const featureOptions = ['SinglePlayer', 'MultiPlayer', 'CoOp', 'VR'];

  const sortLabels: Record<SortOption, string> = {
    discounts: 'Спочатку знижки',
    price_asc: 'Від дешевих до дорогих',
    price_desc: 'Від дорогих до дешевих',
    title_asc: 'А – Я',
    title_desc: 'Я – А',
    date_new: 'Спочатку нові'
  };

  onMount(() => {
    wishlistStore.loadWishlist();
  });

  function calculateOriginalPrice(g: Game): string {
    return formatBasePrice(g.priceInNanoTons);
  }

  function handleResetFilters() {
    wishlistSearchQuery = '';
    tagInput = '';
    selectedGenre = 'All';
    selectedPriceRange = 'all';
    isDiscountOnly = false;
    selectedType = 'all';
    selectedPlatforms = [];
    selectedFeatures = [];
  }

  function handleStoreSearch(e: Event) {
    e.preventDefault();
    if (storeSearchQuery.trim()) {
      gamesStore.setFilters({ search: storeSearchQuery.trim() });
      uiStore.setTab('catalog');
    }
  }

  function handleDecorativeBuy(e: MouseEvent, game: Game) {
    e.stopPropagation();
    uiStore.addToast({
      title: 'Кошик',
      message: `Гру '${game.title}' додано до кошика.`,
      type: 'success',
    });
  }

  const filteredItems = $derived(
    $wishlistStore.items
      .filter((item) => {
        const g = item.game;
        if (wishlistSearchQuery.trim()) {
          const q = wishlistSearchQuery.toLowerCase();
          const matches =
            g.title.toLowerCase().includes(q) ||
            (g.shortDescription && g.shortDescription.toLowerCase().includes(q)) ||
            (g.tags && g.tags.some((t) => t.toLowerCase().includes(q))) ||
            (g.genres && g.genres.some((gen) => gen.toLowerCase().includes(q)));
          if (!matches) return false;
        }

        if (tagInput.trim()) {
          const tq = tagInput.toLowerCase();
          const matchTag =
            (g.tags && g.tags.some((t) => t.toLowerCase().includes(tq))) ||
            (g.genres && g.genres.some((gen) => gen.toLowerCase().includes(tq)));
          if (!matchTag) return false;
        }

        if (selectedGenre !== 'All') {
          const genMatch = g.genres && g.genres.some((gen) => gen.toLowerCase() === selectedGenre.toLowerCase());
          if (!genMatch) return false;
        }

        const priceTon = Number(g.priceInNanoTons) / 1_000_000_000;
        if (selectedPriceRange === 'free' && priceTon > 0) return false;
        if (selectedPriceRange === 'under1' && priceTon > 1) return false;
        if (selectedPriceRange === 'under3' && priceTon > 3) return false;
        if (selectedPriceRange === 'under6' && priceTon > 6) return false;
        if (selectedPriceRange === 'under9' && priceTon > 9) return false;

        if (isDiscountOnly && (g.discountPercentage || 0) <= 0) return false;

        if (selectedType === 'base' && g.isDlc) return false;
        if (selectedType === 'dlc' && !g.isDlc) return false;

        if (selectedPlatforms.length > 0) {
          const hasPlat = selectedPlatforms.some((p) => g.platforms?.includes(p));
          if (!hasPlat) return false;
        }

        if (selectedFeatures.length > 0) {
          const hasFeat = selectedFeatures.some((f) => g.features?.includes(f));
          if (!hasFeat) return false;
        }

        return true;
      })
      .sort((a, b) => {
        if (sortBy === 'discounts') {
          return (b.game.discountPercentage || 0) - (a.game.discountPercentage || 0);
        }
        if (sortBy === 'price_asc') {
          return Number(a.game.priceInNanoTons) - Number(b.game.priceInNanoTons);
        }
        if (sortBy === 'price_desc') {
          return Number(b.game.priceInNanoTons) - Number(a.game.priceInNanoTons);
        }
        if (sortBy === 'title_asc') {
          return a.game.title.localeCompare(b.game.title);
        }
        if (sortBy === 'title_desc') {
          return b.game.title.localeCompare(a.game.title);
        }
        if (sortBy === 'date_new') {
          return new Date(b.addedAt).getTime() - new Date(a.addedAt).getTime();
        }
        return 0;
      })
  );
</script>

<div class="max-w-7xl mx-auto px-4 lg:px-8 py-6 space-y-6">
  <div class="flex flex-col sm:flex-row items-center justify-between gap-4">
    <form onsubmit={handleStoreSearch} class="relative w-full sm:w-80">
      <input
        type="text"
        placeholder="Пошук у Крамниці..."
        bind:value={storeSearchQuery}
        class="w-full pl-4 pr-10 py-2.5 rounded-2xl bg-[#061820]/90 border border-cyan-500/30 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-400 transition-all shadow-inner"
      />
      <button type="submit" class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-cyan-400 p-1 cursor-pointer">
        <Search class="w-4 h-4" />
      </button>
    </form>

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
          class="relative p-2 rounded-xl bg-cyan-500/20 border border-cyan-400 text-rose-400 shadow-md shadow-cyan-500/20 transition-all cursor-pointer"
          title="Список бажань"
        >
          <Heart class="w-4 h-4 fill-rose-500 text-rose-500" />
          {#if $wishlistStore.items.length > 0}
            <span class="absolute -top-1.5 -right-1.5 min-w-[16px] h-[16px] px-1 rounded-full bg-rose-600 text-white text-[9px] font-bold flex items-center justify-center">
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

  <div>
    <h1 class="text-2xl sm:text-3xl font-black text-white tracking-tight flex items-center gap-3">
      <span>Мій список бажаного</span>
      <span class="text-xs px-2.5 py-0.5 rounded-full bg-[#08232e] text-cyan-300 border border-cyan-500/30 font-mono font-bold">
        {$wishlistStore.items.length} { $wishlistStore.items.length === 1 ? 'гра' : 'ігор' }
      </span>
    </h1>
  </div>

  <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
    <aside class="lg:col-span-3 space-y-4">
      <div class="p-4 rounded-2xl bg-[#061820]/90 border border-cyan-500/20 shadow-lg space-y-4">
        <div class="flex items-center justify-between">
          <h3 class="text-sm font-extrabold text-white tracking-wide flex items-center gap-2">
            <SlidersHorizontal class="w-4 h-4 text-cyan-400" />
            <span>Фільтри</span>
          </h3>
          <button
            onclick={handleResetFilters}
            class="text-xs text-cyan-400 hover:text-cyan-300 font-semibold flex items-center gap-1 transition-colors cursor-pointer"
          >
            <RotateCcw class="w-3 h-3" />
            <span>Скинути</span>
          </button>
        </div>

        <div class="relative">
          <input
            type="text"
            placeholder="Пошук тегів..."
            bind:value={tagInput}
            class="w-full pl-3 pr-8 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-500"
          />
          {#if tagInput}
            <button
              type="button"
              onclick={() => tagInput = ''}
              class="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white cursor-pointer"
            >
              <X class="w-3.5 h-3.5" />
            </button>
          {/if}
        </div>

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
                  onclick={() => selectedGenre = g}
                  class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs transition-colors cursor-pointer flex items-center justify-between
                    {selectedGenre === g
                      ? 'bg-cyan-500/20 text-cyan-300 font-bold border border-cyan-500/40'
                      : 'text-slate-400 hover:text-white hover:bg-slate-800/40'}"
                >
                  <span>{g}</span>
                  {#if selectedGenre === g}
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
            <div class="mt-2.5 space-y-2">
              {#each priceOptions as opt}
                <label class="flex items-center gap-2 text-xs text-slate-300 cursor-pointer hover:text-cyan-300">
                  <input
                    type="radio"
                    name="priceFilter"
                    value={opt.id}
                    checked={selectedPriceRange === opt.id}
                    onchange={() => selectedPriceRange = opt.id}
                    class="accent-cyan-400 cursor-pointer"
                  />
                  <span>{opt.label}</span>
                </label>
              {/each}

              <div class="pt-2 border-t border-cyan-950/60">
                <label class="flex items-center gap-2 text-xs font-semibold text-rose-300 cursor-pointer hover:text-rose-200">
                  <input
                    type="checkbox"
                    bind:checked={isDiscountOnly}
                    class="accent-rose-500 cursor-pointer"
                  />
                  <span>Знижки (Discounts)</span>
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
            <div class="mt-2.5 space-y-1.5 text-xs text-slate-300">
              <label class="flex items-center gap-2 cursor-pointer hover:text-white">
                <input type="radio" name="typeFilter" value="all" checked={selectedType === 'all'} onchange={() => selectedType = 'all'} class="accent-cyan-400" />
                <span>Усі типи</span>
              </label>
              <label class="flex items-center gap-2 cursor-pointer hover:text-white">
                <input type="radio" name="typeFilter" value="base" checked={selectedType === 'base'} onchange={() => selectedType = 'base'} class="accent-cyan-400" />
                <span>Базова гра</span>
              </label>
              <label class="flex items-center gap-2 cursor-pointer hover:text-white">
                <input type="radio" name="typeFilter" value="dlc" checked={selectedType === 'dlc'} onchange={() => selectedType = 'dlc'} class="accent-cyan-400" />
                <span>DLC / Доповнення</span>
              </label>
            </div>
          {/if}
        </div>

        <div class="border-t border-cyan-950/80 pt-3">
          <button
            type="button"
            onclick={() => isFeaturesOpen = !isFeaturesOpen}
            class="w-full flex items-center justify-between text-xs font-bold text-slate-200 hover:text-cyan-300 transition-colors cursor-pointer"
          >
            <span>Особливості</span>
            {#if isFeaturesOpen}
              <ChevronUp class="w-4 h-4 text-cyan-400" />
            {:else}
              <ChevronDown class="w-4 h-4 text-slate-500" />
            {/if}
          </button>

          {#if isFeaturesOpen}
            <div class="mt-2.5 space-y-1.5">
              {#each featureOptions as feat}
                <label class="flex items-center gap-2 text-xs text-slate-300 cursor-pointer hover:text-white">
                  <input
                    type="checkbox"
                    checked={selectedFeatures.includes(feat)}
                    onchange={() => {
                      if (selectedFeatures.includes(feat)) {
                        selectedFeatures = selectedFeatures.filter(f => f !== feat);
                      } else {
                        selectedFeatures = [...selectedFeatures, feat];
                      }
                    }}
                    class="accent-cyan-400 cursor-pointer"
                  />
                  <span>{feat}</span>
                </label>
              {/each}
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
              {#each platformOptions as plat}
                <label class="flex items-center gap-2 text-xs text-slate-300 cursor-pointer hover:text-white">
                  <input
                    type="checkbox"
                    checked={selectedPlatforms.includes(plat)}
                    onchange={() => {
                      if (selectedPlatforms.includes(plat)) {
                        selectedPlatforms = selectedPlatforms.filter(p => p !== plat);
                      } else {
                        selectedPlatforms = [...selectedPlatforms, plat];
                      }
                    }}
                    class="accent-cyan-400 cursor-pointer"
                  />
                  <span>{plat}</span>
                </label>
              {/each}
            </div>
          {/if}
        </div>

        <div class="border-t border-cyan-950/80 pt-3">
          <button
            type="button"
            onclick={() => isEventsOpen = !isEventsOpen}
            class="w-full flex items-center justify-between text-xs font-bold text-slate-200 hover:text-cyan-300 transition-colors cursor-pointer"
          >
            <span>Івенти</span>
            {#if isEventsOpen}
              <ChevronUp class="w-4 h-4 text-cyan-400" />
            {:else}
              <ChevronDown class="w-4 h-4 text-slate-500" />
            {/if}
          </button>
        </div>
      </div>
    </aside>

    <main class="lg:col-span-9 space-y-4">
      <div class="flex flex-col sm:flex-row items-center justify-between gap-3 p-2.5 rounded-2xl bg-[#061820]/90 border border-cyan-500/20 shadow-md">
        <div class="relative flex-1 w-full">
          <input
            type="text"
            placeholder="Пошук у Бажаному..."
            bind:value={wishlistSearchQuery}
            class="w-full pl-4 pr-10 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-400 shadow-inner"
          />
          {#if wishlistSearchQuery}
            <button
              type="button"
              onclick={() => wishlistSearchQuery = ''}
              class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white cursor-pointer"
            >
              <X class="w-3.5 h-3.5" />
            </button>
          {/if}
        </div>

        <div class="relative shrink-0">
          <div class="flex items-center gap-2">
            <span class="text-xs text-slate-400 font-medium">Сортування:</span>
            <button
              type="button"
              onclick={() => isSortDropdownOpen = !isSortDropdownOpen}
              class="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-[#0a232c] border border-cyan-500/30 hover:border-cyan-400 text-xs font-bold text-cyan-300 transition-all cursor-pointer shadow-sm"
            >
              <span>{sortLabels[sortBy]}</span>
              <ChevronDown class="w-3.5 h-3.5 text-cyan-400 transition-transform {isSortDropdownOpen ? 'rotate-180' : ''}" />
            </button>
          </div>

          {#if isSortDropdownOpen}
            <div class="absolute right-0 top-full mt-1.5 w-56 rounded-xl bg-[#08202b] border border-cyan-500/30 shadow-2xl shadow-cyan-950/80 p-1.5 z-30 space-y-1 animate-in fade-in zoom-in-95">
              {#each (Object.keys(sortLabels) as SortOption[]) as opt}
                <button
                  type="button"
                  onclick={() => {
                    sortBy = opt;
                    isSortDropdownOpen = false;
                  }}
                  class="w-full text-left px-3 py-2 rounded-lg text-xs transition-colors cursor-pointer flex items-center justify-between
                    {sortBy === opt
                      ? 'bg-cyan-500/20 text-cyan-300 font-bold'
                      : 'text-slate-300 hover:text-white hover:bg-slate-800/60'}"
                >
                  <span>{sortLabels[opt]}</span>
                  {#if sortBy === opt}
                    <Check class="w-3.5 h-3.5 text-cyan-400" />
                  {/if}
                </button>
              {/each}
            </div>
          {/if}
        </div>
      </div>

      {#if $wishlistStore.isLoading}
        <div class="text-center py-20 text-slate-400 text-xs flex items-center justify-center gap-2">
          <span class="animate-spin text-cyan-400 text-lg">⟳</span>
          <span>Завантаження списку бажаного...</span>
        </div>
      {:else if filteredItems.length > 0}
        <div class="space-y-3">
          {#each filteredItems as item (item.gameId)}
            {@const game = item.game}
            <div
              role="button"
              tabindex="0"
              onclick={() => openGame(game)}
              onkeydown={(e) => e.key === 'Enter' && openGame(game)}
              class="group relative flex flex-col sm:flex-row items-start sm:items-center justify-between p-3 sm:p-3.5 rounded-2xl bg-[#061820]/95 hover:bg-[#092530] border border-cyan-500/20 hover:border-cyan-400/60 shadow-lg hover:shadow-cyan-500/10 transition-all duration-300 cursor-pointer gap-4"
            >
              <div class="flex items-center gap-4 flex-1 min-w-0">
                <div class="relative w-28 sm:w-44 aspect-[16/9] rounded-xl overflow-hidden bg-slate-950 shrink-0 border border-cyan-500/30 shadow-md">
                  <img
                    src={game.headerImageUrl || game.coverImageUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600'}
                    alt={game.title}
                    class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                  />
                  {#if game.isDlc}
                    <span class="absolute top-1.5 left-1.5 px-1.5 py-0.2 rounded bg-purple-950 text-purple-300 text-[8px] font-bold uppercase tracking-wider border border-purple-500/40">
                      DLC
                    </span>
                  {/if}
                </div>

                <div class="min-w-0 space-y-1.5">
                  <h3 class="text-sm sm:text-base font-extrabold text-white group-hover:text-cyan-300 transition-colors truncate">
                    {game.title}
                  </h3>

                  <div class="flex flex-wrap gap-1.5">
                    {#if game.genres && game.genres.length > 0}
                      {#each game.genres.slice(0, 3) as gen}
                        <span class="px-2 py-0.5 rounded-md bg-[#0a2733] border border-cyan-500/20 text-[10px] text-cyan-300 font-medium">
                          {gen.toLowerCase()}
                        </span>
                      {/each}
                    {/if}
                    {#if game.tags && game.tags.length > 0}
                      {#each game.tags.slice(0, 2) as tag}
                        <span class="px-2 py-0.5 rounded-md bg-[#0a2733] border border-cyan-500/20 text-[10px] text-slate-300 font-medium">
                          {tag.toLowerCase()}
                        </span>
                      {/each}
                    {/if}
                    {#if (game.genres?.length || 0) + (game.tags?.length || 0) > 5}
                      <span class="px-1.5 py-0.5 rounded-md bg-[#0a2733] border border-cyan-500/20 text-[9px] text-slate-400 font-mono">
                        +{((game.genres?.length || 0) + (game.tags?.length || 0)) - 5}
                      </span>
                    {/if}
                  </div>

                  <div class="flex items-center gap-1.5 text-xs text-slate-300">
                    <span class="font-bold text-slate-200">{game.averageRating > 0 ? game.averageRating.toFixed(1) : '4.5'}</span>
                    <Star class="w-3.5 h-3.5 fill-rose-500 text-rose-500" />
                  </div>
                </div>
              </div>

              <div class="flex sm:flex-col items-end justify-between sm:justify-center w-full sm:w-auto shrink-0 gap-3">
                <button
                  type="button"
                  onclick={(e) => {
                    e.stopPropagation();
                    wishlistStore.toggleWishlist(game);
                  }}
                  class="absolute top-2.5 right-2.5 p-1 rounded-lg text-slate-500 hover:text-rose-400 hover:bg-rose-950/40 transition-colors cursor-pointer"
                  title="Видалити зі списку бажаного"
                >
                  <X class="w-4 h-4" />
                </button>

                <div class="sm:text-right space-y-1 w-full flex flex-row sm:flex-col items-center sm:items-end justify-between sm:justify-start gap-2 sm:gap-1 mt-1 sm:mt-0">
                  <div class="flex items-center gap-2">
                    {#if (game.discountPercentage || 0) > 0}
                      <span class="px-1.5 py-0.5 rounded bg-rose-600 text-white font-extrabold text-[10px]">
                        -{game.discountPercentage}%
                      </span>
                    {/if}

                    <span class="text-sm sm:text-base font-black text-cyan-300 font-mono">
                      {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                    </span>

                    {#if (game.discountPercentage || 0) > 0}
                      <span class="text-[10px] text-slate-500 line-through font-mono">
                        {calculateOriginalPrice(game)}
                      </span>
                    {/if}
                  </div>

                  {#if (game.discountPercentage || 0) > 0}
                    <div class="text-[9px] text-slate-400 font-medium">
                      Знижка діє до 30.02.2026 10:00
                    </div>
                  {/if}

                  <button
                    type="button"
                    onclick={(e) => handleAddToCart(e, game)}
                    class="px-4 py-1.5 sm:py-2 rounded-xl bg-[#06b6d4] hover:bg-[#22d3ee] text-black font-extrabold text-xs shadow-md shadow-cyan-500/20 transition-all cursor-pointer shrink-0 mt-1"
                  >
                    У кошик
                  </button>
                </div>
              </div>
            </div>
          {/each}
        </div>
      {:else}
        <div class="text-center py-24 bg-[#061820]/60 rounded-3xl border border-cyan-500/20 shadow-xl space-y-3">
          <div class="w-16 h-16 rounded-2xl bg-rose-500/10 border border-rose-500/20 flex items-center justify-center text-rose-400 mx-auto">
            <Heart class="w-8 h-8 text-rose-400/70" />
          </div>
          <h4 class="text-lg font-bold text-white">Список бажаного порожній</h4>
          <p class="text-xs text-slate-400 max-w-sm mx-auto">
            Зберігайте ігри за допомогою кнопки з сердечком ♥ у крамниці, щоб стежити за релізами та знижками.
          </p>
          <button
            onclick={() => uiStore.setTab('catalog')}
            class="mt-3 px-5 py-2.5 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 text-black font-bold text-xs shadow-lg shadow-cyan-500/25 transition-all cursor-pointer inline-flex items-center gap-2"
          >
            <Gamepad2 class="w-4 h-4" />
            <span>Перейти до каталогу ігор</span>
          </button>
        </div>
      {/if}
    </main>
  </div>

  <GameDetailsModal />
</div>
