<script lang="ts">
  import { onMount } from 'svelte';
  import {
    Search,
    SlidersHorizontal,
    ChevronDown,
    Star,
    ArrowDown,
    ArrowLeft,
    Loader2,
    ShoppingCart,
    Check,
    Sparkles,
    PackageOpen
  } from 'lucide-svelte';
  import { gamesStore } from '../../stores/gamesStore';
  import { cartStore } from '../../stores/cartStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { uiStore } from '../../stores/uiStore';
  import { gamesService } from '../../services/gamesService';
  import { router } from '../../services/router';
  import { formatPrice, formatBasePrice } from '../../utils/formatters';
  import type { Game } from '../../types';

  const game = $derived($gamesStore.selectedGame || $gamesStore.games[0]);

  let dlcs = $state<Game[]>([]);
  let isLoading = $state(false);
  let searchQuery = $state('');
  let sortBy = $state<'relevance' | 'price_asc' | 'price_desc' | 'rating' | 'name'>('relevance');
  let isSortDropdownOpen = $state(false);
  let filterType = $state<'all' | 'free' | 'paid' | 'discounted'>('all');
  let isFilterBarOpen = $state(false);

  $effect(() => {
    if (game?.id) {
      loadDlcs(game.id);
    }
  });

  async function loadDlcs(gameId: string) {
    isLoading = true;
    try {
      dlcs = await gamesService.getDlcs(gameId);
    } catch (e) {
      console.warn('[AllDlcsView] Failed to load DLCs:', e);
      dlcs = [];
    } finally {
      isLoading = false;
    }
  }

  const sortLabels = {
    relevance: 'За релевантністю',
    price_asc: 'Спочатку дешевші',
    price_desc: 'Спочатку дорожчі',
    rating: 'За рейтингом',
    name: 'За назвою'
  };

  let filteredDlcs = $derived.by(() => {
    let result = [...dlcs];

    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase().trim();
      result = result.filter(d =>
        d.title.toLowerCase().includes(q) ||
        (d.shortDescription && d.shortDescription.toLowerCase().includes(q)) ||
        (d.description && d.description.toLowerCase().includes(q))
      );
    }

    if (filterType === 'free') {
      result = result.filter(d => Number(d.priceInNanoTons) === 0);
    } else if (filterType === 'paid') {
      result = result.filter(d => Number(d.priceInNanoTons) > 0);
    } else if (filterType === 'discounted') {
      result = result.filter(d => (d.discountPercentage ?? 0) > 0);
    }

    if (sortBy === 'price_asc') {
      result.sort((a, b) => Number(a.priceInNanoTons) - Number(b.priceInNanoTons));
    } else if (sortBy === 'price_desc') {
      result.sort((a, b) => Number(b.priceInNanoTons) - Number(a.priceInNanoTons));
    } else if (sortBy === 'rating') {
      result.sort((a, b) => (b.averageRating ?? 0) - (a.averageRating ?? 0));
    } else if (sortBy === 'name') {
      result.sort((a, b) => a.title.localeCompare(b.title));
    }

    return result;
  });

  function openDlcDetails(dlc: Game) {
    router.navigateToGame(dlc);
  }

  async function handleAddToCart(dlc: Game, e: MouseEvent) {
    e.stopPropagation();
    await cartStore.addToCart(dlc, false);
    uiStore.addToast({
      title: 'Додано в кошик',
      message: `DLC "${dlc.title}" успішно додано до вашого кошика.`,
      type: 'success'
    });
  }

  const paidDlcs = $derived(dlcs.filter((d) => Number(d.priceInNanoTons) > 0));
  const totalDlcsPrice = $derived(
    paidDlcs.reduce((acc, d) => acc + (Number(d.priceInNanoTons) || 0), 0)
  );
  const bundleDiscountedPrice = $derived(Math.round(totalDlcsPrice * 0.8));
  const bundleSavings = $derived(totalDlcsPrice - bundleDiscountedPrice);

  let isAddingAll = $state(false);

  async function handleBuyAllDlcs() {
    if (dlcs.length === 0) return;
    isAddingAll = true;
    try {
      await cartStore.addMultipleToCart(
        dlcs,
        false,
        `Усі ${dlcs.length} DLC додано до кошика зі знижкою 20%!`
      );
    } finally {
      isAddingAll = false;
    }
  }
</script>

<div class="min-h-screen bg-[#030d12] text-slate-100 pb-20">

  {#if game}

    <div class="relative w-full h-[320px] md:h-[400px] overflow-hidden bg-black select-none">

      <img
        src={game.headerImageUrl || game.coverImageUrl || '/placeholder.png'}
        alt={game.title}
        class="w-full h-full object-cover object-center filter brightness-90 transform scale-105"
      />

      <div class="absolute inset-0 bg-gradient-to-t from-[#030d12] via-[#030d12]/40 to-black/60"></div>
      <div class="absolute inset-0 bg-gradient-to-r from-[#030d12]/90 via-[#030d12]/40 to-transparent"></div>

      <div class="absolute inset-0 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex flex-col justify-between py-6">

        <div>
          <button
            type="button"
            onclick={() => router.navigateToGame(game)}
            class="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-black/60 hover:bg-cyan-950/80 border border-cyan-500/30 text-xs font-semibold text-cyan-300 hover:text-cyan-200 transition-all cursor-pointer backdrop-blur-md"
          >
            <ArrowLeft class="w-3.5 h-3.5" />
            <span>Назад до гри {game.title}</span>
          </button>
        </div>

        <div class="space-y-1 mb-4">
          <p class="text-xs sm:text-sm font-bold text-cyan-300 tracking-wider drop-shadow-md">
            Завантажуваний контент для
          </p>
          <h1 class="text-3xl sm:text-5xl font-black text-white tracking-tight drop-shadow-lg font-display">
            {game.title}
          </h1>
        </div>

      </div>
    </div>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 -mt-6 relative z-10 space-y-8">

      {#if dlcs.length > 1}
        <div class="p-6 rounded-3xl bg-gradient-to-r from-[#072433] via-[#041924] to-[#071d27] border-2 border-cyan-400/40 shadow-2xl shadow-cyan-500/10 flex flex-col lg:flex-row lg:items-center justify-between gap-6 relative overflow-hidden">
          <div class="absolute -right-10 -bottom-10 w-64 h-64 bg-cyan-500/10 rounded-full blur-3xl pointer-events-none"></div>

          <div class="space-y-3 z-10 flex-1">
            <div class="flex items-center gap-2">
              <span class="px-2.5 py-1 rounded-lg bg-cyan-400 text-black font-black text-xs uppercase tracking-wider shadow-md">
                -20% Знижка на комплект
              </span>
              <span class="text-xs font-semibold text-cyan-300 flex items-center gap-1">
                <Sparkles class="w-3.5 h-3.5" />
                Вигідна пропозиція
              </span>
            </div>

            <h2 class="text-xl sm:text-2xl font-black text-white font-display">
              Купити всі {dlcs.length} DLC зі знижкою 20%
            </h2>
            <p class="text-xs sm:text-sm text-slate-300 max-w-2xl leading-relaxed">
              Отримайте повний набір контенту для {game.title}! Придбайте всі доповнення разом одним кліком і заощаджуйте 20% на кожному DLC.
            </p>

            <div class="flex items-center gap-2 pt-1 overflow-x-auto">
              {#each dlcs.slice(0, 6) as dlc}
                <img
                  src={dlc.coverImageUrl || dlc.headerImageUrl || '/placeholder.png'}
                  alt={dlc.title}
                  title={dlc.title}
                  class="w-10 h-10 rounded-lg object-cover border border-cyan-500/30 shrink-0"
                />
              {/each}
              {#if dlcs.length > 6}
                <div class="w-10 h-10 rounded-lg bg-cyan-950/80 border border-cyan-500/30 flex items-center justify-center text-xs font-bold text-cyan-300 shrink-0">
                  +{dlcs.length - 6}
                </div>
              {/if}
            </div>
          </div>

          <div class="z-10 flex flex-col sm:flex-row lg:flex-col items-start lg:items-end justify-between gap-4 p-5 rounded-2xl bg-black/40 border border-cyan-500/20 shrink-0">
            <div>
              <div class="text-[11px] text-slate-400">Ціна комплекту всіх DLC:</div>
              <div class="flex items-baseline gap-2">
                {#if totalDlcsPrice > 0}
                  <span class="text-xs text-slate-400 line-through font-mono">
                    {formatPrice(totalDlcsPrice)}
                  </span>
                  <span class="text-xl font-black text-cyan-300 font-mono">
                    {formatPrice(bundleDiscountedPrice)}
                  </span>
                {:else}
                  <span class="text-xl font-black text-emerald-400">Безкоштовно</span>
                {/if}
              </div>
              {#if bundleSavings > 0}
                <div class="text-[10px] font-bold text-emerald-400 mt-0.5">
                  Ви заощаджуєте {formatPrice(bundleSavings)} (-20%)
                </div>
              {/if}
            </div>

            <button
              type="button"
              onclick={handleBuyAllDlcs}
              disabled={isAddingAll}
              class="w-full sm:w-auto px-6 py-3 rounded-xl bg-gradient-to-r from-cyan-400 to-cyan-500 hover:from-cyan-300 hover:to-cyan-400 text-black font-extrabold text-xs transition-all duration-200 shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50"
            >
              <ShoppingCart class="w-4 h-4" />
              <span>{isAddingAll ? 'Додавання...' : `Додати в кошик усі DLC (${dlcs.length})`}</span>
            </button>
          </div>
        </div>
      {/if}

      <div class="bg-[#051720]/90 border border-cyan-500/25 rounded-2xl p-3.5 shadow-xl backdrop-blur-md flex flex-wrap items-center justify-between gap-3">

        <div class="flex items-center gap-3 flex-1 min-w-0 sm:min-w-[280px] w-full sm:w-auto">
          <div class="relative flex-1">
            <Search class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Пошук..."
              bind:value={searchQuery}
              class="w-full pl-10 pr-4 py-2 rounded-xl bg-[#020b10] border border-cyan-900/60 focus:border-cyan-400 text-xs text-white placeholder-slate-500 outline-none transition-all"
            />
          </div>

          <button
            type="button"
            onclick={() => isFilterBarOpen = !isFilterBarOpen}
            class="flex items-center gap-1.5 px-3.5 py-2 rounded-xl border text-xs font-semibold transition-all cursor-pointer {isFilterBarOpen || filterType !== 'all' ? 'bg-cyan-500/20 border-cyan-400 text-cyan-300' : 'bg-[#020b10] border-cyan-900/60 text-slate-400 hover:text-white'}"
          >
            <SlidersHorizontal class="w-3.5 h-3.5" />
            <span>Фільтри</span>
          </button>
        </div>

        <div class="relative">
          <button
            type="button"
            onclick={() => isSortDropdownOpen = !isSortDropdownOpen}
            class="flex items-center gap-2 px-3.5 py-2 rounded-xl bg-[#020b10] border border-cyan-900/60 hover:border-cyan-500/40 text-xs font-medium text-slate-300 cursor-pointer transition-all"
          >
            <span class="text-slate-400">сортування:</span>
            <span class="text-cyan-400 font-bold">{sortLabels[sortBy]}</span>
            <ChevronDown class="w-3.5 h-3.5 text-cyan-400" />
          </button>

          {#if isSortDropdownOpen}
            <div class="absolute right-0 top-11 z-30 bg-[#071d27] border border-cyan-500/30 rounded-xl py-1.5 shadow-2xl min-w-[170px] backdrop-blur-lg">
              {#each Object.entries(sortLabels) as [key, label]}
                <button
                  type="button"
                  onclick={() => { sortBy = key as any; isSortDropdownOpen = false; }}
                  class="w-full text-left px-3.5 py-2 text-xs text-slate-300 hover:bg-cyan-950/60 hover:text-cyan-300 cursor-pointer transition-colors {sortBy === key ? 'text-cyan-400 font-bold bg-cyan-950/40' : ''}"
                >
                  {label}
                </button>
              {/each}
            </div>
          {/if}
        </div>

      </div>

      {#if isFilterBarOpen}
        <div class="flex items-center gap-2 pt-1 pb-2 overflow-x-auto">
          <button
            type="button"
            onclick={() => filterType = 'all'}
            class="px-3 py-1.5 rounded-full text-xs font-bold transition-all cursor-pointer {filterType === 'all' ? 'bg-cyan-400 text-black shadow-md' : 'bg-[#061820] text-slate-400 border border-cyan-900/40 hover:text-white'}"
          >
            Усі DLC ({dlcs.length})
          </button>
          <button
            type="button"
            onclick={() => filterType = 'free'}
            class="px-3 py-1.5 rounded-full text-xs font-bold transition-all cursor-pointer {filterType === 'free' ? 'bg-cyan-400 text-black shadow-md' : 'bg-[#061820] text-slate-400 border border-cyan-900/40 hover:text-white'}"
          >
            Безкоштовні ({dlcs.filter(d => Number(d.priceInNanoTons) === 0).length})
          </button>
          <button
            type="button"
            onclick={() => filterType = 'discounted'}
            class="px-3 py-1.5 rounded-full text-xs font-bold transition-all cursor-pointer {filterType === 'discounted' ? 'bg-cyan-400 text-black shadow-md' : 'bg-[#061820] text-slate-400 border border-cyan-900/40 hover:text-white'}"
          >
            Зі знижкою ({dlcs.filter(d => (d.discountPercentage ?? 0) > 0).length})
          </button>
          <button
            type="button"
            onclick={() => filterType = 'paid'}
            class="px-3 py-1.5 rounded-full text-xs font-bold transition-all cursor-pointer {filterType === 'paid' ? 'bg-cyan-400 text-black shadow-md' : 'bg-[#061820] text-slate-400 border border-cyan-900/40 hover:text-white'}"
          >
            Платні ({dlcs.filter(d => Number(d.priceInNanoTons) > 0).length})
          </button>
        </div>
      {/if}

      {#if isLoading}
        <div class="flex items-center justify-center py-20">
          <Loader2 class="w-10 h-10 text-cyan-400 animate-spin" />
        </div>
      {:else if filteredDlcs.length > 0}
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          {#each filteredDlcs as dlc (dlc.id)}
            <div
              role="button"
              tabindex="0"
              onclick={() => openDlcDetails(dlc)}
              onkeydown={(e) => e.key === 'Enter' && openDlcDetails(dlc)}
              class="bg-[#051720]/90 border border-cyan-500/25 hover:border-cyan-400/70 rounded-3xl overflow-hidden shadow-xl hover:shadow-cyan-950/40 transition-all group cursor-pointer flex flex-col justify-between"
            >
              <div>

                <div class="relative w-full h-48 sm:h-56 bg-black overflow-hidden">
                  <img
                    src={dlc.headerImageUrl || dlc.coverImageUrl || '/placeholder.png'}
                    alt={dlc.title}
                    class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                  />
                  <div class="absolute inset-0 bg-gradient-to-t from-[#051720] via-transparent to-transparent opacity-80"></div>

                  <div class="absolute top-3 left-3 w-8 h-8 rounded-full bg-black/70 border border-cyan-500/40 backdrop-blur-md flex items-center justify-center text-cyan-400 shadow-md">
                    <ArrowDown class="w-4 h-4 stroke-[2.5]" />
                  </div>

                  {#if (dlc.discountPercentage ?? 0) > 0}
                    <div class="absolute top-3 right-3 bg-emerald-500 text-black font-black text-xs px-2.5 py-1 rounded-full shadow-lg">
                      -{dlc.discountPercentage}%
                    </div>
                  {/if}
                </div>

                <div class="p-5 space-y-3">
                  <div class="flex items-baseline justify-between gap-2">
                    <h3 class="text-lg font-bold text-white group-hover:text-cyan-300 transition-colors leading-tight">
                      {dlc.title}
                    </h3>
                    <div class="flex items-center gap-1 text-xs font-bold text-amber-400 shrink-0">
                      <span>{dlc.averageRating ? dlc.averageRating.toFixed(1) : '4.5'}</span>
                      <Star class="w-3.5 h-3.5 fill-amber-400 text-amber-400" />
                    </div>
                  </div>

                  <p class="text-xs text-slate-400 leading-relaxed line-clamp-3">
                    {dlc.shortDescription || dlc.description || 'Офіційний додатковий вміст для гри.'}
                  </p>
                </div>
              </div>

              <div class="px-5 pb-5 pt-2 flex items-center justify-between gap-4 border-t border-cyan-950/60">
                <div>
                  {#if Number(dlc.priceInNanoTons) === 0}
                    <span class="text-base font-bold text-emerald-400 tracking-wide">
                      Безкоштовно
                    </span>
                  {:else}
                    <div class="flex items-baseline gap-2">
                      <span class="text-base font-extrabold text-white font-mono">
                        {formatPrice(dlc.priceInNanoTons, dlc.discountPercentage)}
                      </span>
                      {#if (dlc.discountPercentage ?? 0) > 0}
                        <span class="text-xs text-slate-500 line-through font-mono">
                          {formatBasePrice(dlc.priceInNanoTons)}
                        </span>
                      {/if}
                    </div>
                  {/if}
                </div>

                <button
                  type="button"
                  onclick={(e) => handleAddToCart(dlc, e)}
                  class="px-5 py-2 rounded-full font-extrabold text-xs transition-all cursor-pointer shadow-md flex items-center gap-1.5 {$cartStore.cartGameIds?.has(dlc.id) ? 'bg-emerald-500 text-black hover:bg-emerald-400' : 'bg-[#22d3ee] text-black hover:bg-cyan-300'}"
                >
                  {#if $cartStore.cartGameIds?.has(dlc.id)}
                    <Check class="w-3.5 h-3.5 stroke-[3]" />
                    <span>У кошику</span>
                  {:else}
                    <span>У кошик</span>
                  {/if}
                </button>
              </div>

            </div>
          {/each}
        </div>
      {:else}
        <div class="text-center py-20 bg-[#051720]/60 rounded-3xl border border-cyan-500/20 shadow-xl space-y-3 max-w-lg mx-auto">
          <PackageOpen class="w-12 h-12 text-cyan-500/40 mx-auto" />
          <h3 class="text-lg font-bold text-white">DLC не знайдено</h3>
          <p class="text-xs text-slate-400">
            {searchQuery ? `За запитом "${searchQuery}" нічого не знайдено.` : 'Для цієї гри поки немає доступних доповнень.'}
          </p>
          {#if searchQuery}
            <button
              type="button"
              onclick={() => { searchQuery = ''; filterType = 'all'; }}
              class="mt-2 text-xs text-cyan-400 hover:underline cursor-pointer"
            >
              Скинути фільтри
            </button>
          {/if}
        </div>
      {/if}

    </div>
  {:else}
    <div class="text-center py-24 text-slate-400 space-y-3">
      <PackageOpen class="w-12 h-12 text-slate-600 mx-auto" />
      <p class="text-base font-bold text-white">Гру не обрано</p>
      <button
        type="button"
        onclick={() => uiStore.setTab('store')}
        class="text-xs text-cyan-400 hover:underline cursor-pointer"
      >
        Перейти до крамниці
      </button>
    </div>
  {/if}

</div>

