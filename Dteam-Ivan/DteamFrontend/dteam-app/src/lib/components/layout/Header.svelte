<script lang="ts">
import { uiStore, type MainTab } from '../../stores/uiStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { cartStore } from '../../stores/cartStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { authStore, currentUser, isUserAdmin } from '../../stores/authStore';
  import { myProfileStore } from '../../stores/myProfileStore';
  import {
    Gamepad2,
    Shield,
    Compass,
    Heart,
    ShoppingCart,
    LogIn,
    UserPlus,
    LogOut,
    User,
    ChevronDown,
    Search,
    Newspaper,
    Coins,
    Plus,
    Wallet,
    Library,
    Users
  } from 'lucide-svelte';
  import { formatTon, nanoTonToTon } from '../../utils/formatters';
  import { friendsStore } from '../../stores/friendsStore';
  import { gamesService } from '../../services/gamesService';
  import type { GameRecommendation } from '../../types';
  import type { CatalogFilterState } from '../../stores/gamesStore';
  import GameRecommendationsDropdown from '../ui/GameRecommendationsDropdown.svelte';
  import SearchCategoriesModal from '../ui/SearchCategoriesModal.svelte';

  let isUserDropdownOpen = $state(false);
  let headerSearchQuery = $state('');
  let searchWrapperEl = $state<HTMLElement | null>(null);
  let categoriesModalEl = $state<HTMLElement | null>(null);
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;
  let recommendations = $state<GameRecommendation[]>([]);
  let isLoadingRecommendations = $state(false);
  let isRecommendationsOpen = $state(false);
  let isCategoriesModalOpen = $state(false);

  const baseTabs: { id: MainTab; label: string; icon: any; adminOnly?: boolean }[] = [
    { id: 'store', label: 'Крамниця', icon: Gamepad2 },
    { id: 'library', label: 'Бібліотека', icon: Library },
    { id: 'community', label: 'Спільнота', icon: Newspaper },
    { id: 'friends', label: 'Друзі', icon: Users },
    { id: 'catalog', label: 'Каталог', icon: Compass },
    { id: 'admin', label: 'Адмінка', icon: Shield, adminOnly: true },
  ];

  const visibleTabs = $derived(
    baseTabs.filter(tab => !tab.adminOnly || $isUserAdmin)
  );

  function handleLogout() {
    authStore.logout();
    isUserDropdownOpen = false;
    uiStore.setTab('store');
  }

  function handleSearchSubmit(e?: SubmitEvent) {
    if (e) e.preventDefault();
    if (debounceTimer) clearTimeout(debounceTimer);
    isRecommendationsOpen = false;
    isCategoriesModalOpen = false;
    if (headerSearchQuery.trim()) {
      gamesStore.setFilters({ search: headerSearchQuery.trim() });
      uiStore.setTab('catalog');
    }
  }

  async function fetchRecommendations(query: string) {
    if (!query.trim()) {
      recommendations = [];
      isRecommendationsOpen = false;
      isLoadingRecommendations = false;
      return;
    }

    isLoadingRecommendations = true;
    isRecommendationsOpen = true;
    try {
      const res = await gamesService.getRecommendations(query.trim(), 8);
      if (res && res.length > 0) {
        recommendations = res;
      } else {
        const lower = query.trim().toLowerCase();
        recommendations = $gamesStore.games
          .filter(g =>
            g.title.toLowerCase().startsWith(lower) ||
            g.title.toLowerCase().includes(lower) ||
            (g.tags && g.tags.some(t => t.toLowerCase().includes(lower)))
          )
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
      recommendations = $gamesStore.games
        .filter(g =>
          g.title.toLowerCase().startsWith(lower) ||
          g.title.toLowerCase().includes(lower) ||
          (g.tags && g.tags.some(t => t.toLowerCase().includes(lower)))
        )
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
      isLoadingRecommendations = false;
    }
  }

  function handleSearchInput(e: Event) {
    const query = (e.target as HTMLInputElement).value;
    headerSearchQuery = query;

    if (query.trim().length > 0) {
      isCategoriesModalOpen = false;
    } else {
      isCategoriesModalOpen = true;
      recommendations = [];
      isRecommendationsOpen = false;
      if (debounceTimer) clearTimeout(debounceTimer);
      return;
    }

    if (debounceTimer) {
      clearTimeout(debounceTimer);
    }

    debounceTimer = setTimeout(() => {
      fetchRecommendations(query);
    }, 500);
  }

  function handleSearchFocus() {
    if (!headerSearchQuery.trim()) {
      isCategoriesModalOpen = true;
      isRecommendationsOpen = false;
    } else {
      isCategoriesModalOpen = false;
      if (recommendations.length > 0) {
        isRecommendationsOpen = true;
      } else {
        fetchRecommendations(headerSearchQuery);
      }
    }
  }

  function handleSelectCategoryFilter(filter: Partial<CatalogFilterState>) {
    gamesStore.resetFilters();
    gamesStore.setFilters(filter);
    gamesStore.loadCatalogGames();

    isCategoriesModalOpen = false;
    isRecommendationsOpen = false;
    headerSearchQuery = '';

    uiStore.setTab('catalog');
    if (typeof window !== 'undefined') {
      window.scrollTo({ top: 0, behavior: 'instant' });
    }
  }

  async function handleSelectRecommendation(rec: GameRecommendation) {
    if (debounceTimer) clearTimeout(debounceTimer);
    isRecommendationsOpen = false;
    isCategoriesModalOpen = false;
    headerSearchQuery = rec.title;
    try {
      const fullGame = await gamesService.getGameById(rec.id);
      gamesStore.selectGame(fullGame);
      uiStore.setTab('game');
    } catch {
      const cached = $gamesStore.games.find(g => g.id === rec.id);
      if (cached) {
        gamesStore.selectGame(cached);
        uiStore.setTab('game');
      } else {
        gamesStore.setFilters({ search: rec.title });
        uiStore.setTab('catalog');
      }
    }
  }

  function handleClickOutside(e: MouseEvent) {
    const target = e.target as Node;
    if (searchWrapperEl && !searchWrapperEl.contains(target) && (!categoriesModalEl || !categoriesModalEl.contains(target))) {
      isRecommendationsOpen = false;
      isCategoriesModalOpen = false;
    }
  }

  function handleKeydown(e: KeyboardEvent) {
    if (e.key === 'Escape') {
      isRecommendationsOpen = false;
      isCategoriesModalOpen = false;
    }
  }

  function handleLogoClick() {
    gamesStore.resetFilters();
    headerSearchQuery = '';
    isRecommendationsOpen = false;
    isCategoriesModalOpen = false;
    uiStore.setTab('store');
  }
</script>

<svelte:window onclick={handleClickOutside} onkeydown={handleKeydown} />

<header class="sticky top-0 z-40 bg-[#030d12]/90 backdrop-blur-xl border-b border-cyan-500/20 px-4 lg:px-8 py-3 transition-all">
  <div class="max-w-7xl mx-auto flex items-center justify-between gap-4">

    <div class="flex items-center gap-6">
      <button
        onclick={handleLogoClick}
        class="flex items-center gap-2.5 group cursor-pointer text-left"
      >
        <div class="w-9 h-9 rounded-2xl bg-gradient-to-br from-cyan-400 via-teal-500 to-emerald-400 flex items-center justify-center text-black font-black shadow-lg shadow-cyan-500/30 group-hover:scale-105 transition-transform">
          <Gamepad2 class="w-5 h-5" />
        </div>
        <div>
          <span class="font-black text-xl tracking-tighter text-white font-display flex items-center gap-1 leading-none">
            DTEAM<span class="text-cyan-400">.</span>
          </span>
          <span class="block text-[8px] font-bold text-cyan-400/90 tracking-widest uppercase mt-0.5">GAMING HUB</span>
        </div>
      </button>

      <nav class="flex items-center gap-1 bg-[#061820]/90 p-1 rounded-2xl border border-cyan-500/20 shadow-inner">
        {#each visibleTabs as tab}
          {@const Icon = tab.icon}
          <button
            onclick={() => uiStore.setTab(tab.id)}
            class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold tracking-wide transition-all cursor-pointer relative
              {$uiStore.activeTab === tab.id
                ? 'bg-gradient-to-r from-cyan-500 to-emerald-500 text-black shadow-lg shadow-cyan-500/25 font-black'
                : 'text-slate-400 hover:text-white hover:bg-slate-800/40'}"
          >
            <Icon class="w-3.5 h-3.5 {$uiStore.activeTab === tab.id ? 'text-black' : tab.id === 'admin' ? 'text-cyan-400' : 'text-slate-400'}" />
            <span>{tab.label}</span>
            {#if tab.id === 'admin'}
              <span class="w-1.5 h-1.5 rounded-full bg-cyan-400 animate-ping absolute top-1.5 right-1.5"></span>
            {/if}
          </button>
        {/each}
      </nav>
    </div>

    <div bind:this={searchWrapperEl} class="relative flex-1 max-w-md min-w-[200px] mx-1 sm:mx-2">
      <form onsubmit={handleSearchSubmit} class="relative w-full">
        <input
          type="text"
          placeholder="Пошук у Крамниці..."
          bind:value={headerSearchQuery}
          oninput={handleSearchInput}
          onfocus={handleSearchFocus}
          class="w-full pl-4 pr-10 py-2 rounded-2xl bg-[#061820]/90 hover:bg-[#07212b] border border-cyan-500/30 focus:border-cyan-400 focus:shadow-[0_0_15px_rgba(13,242,201,0.25)] focus:outline-none text-xs text-white placeholder-slate-400 transition-all shadow-inner"
        />
        <button
          type="submit"
          class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-cyan-400 p-1 cursor-pointer transition-colors"
          title="Пошук"
        >
          <Search class="w-4 h-4" />
        </button>
      </form>

      <GameRecommendationsDropdown
        {recommendations}
        isOpen={isRecommendationsOpen}
        isLoading={isLoadingRecommendations}
        searchQuery={headerSearchQuery}
        onSelect={handleSelectRecommendation}
        onViewAll={handleSearchSubmit}
      />
    </div>

    <div class="flex items-center gap-2 shrink-0">
      <button
        onclick={() => uiStore.setTab('wishlist')}
        class="relative p-2 rounded-xl border transition-all cursor-pointer group
          {$uiStore.activeTab === 'wishlist'
            ? 'bg-cyan-500/20 border-cyan-400 text-rose-400 shadow-md shadow-cyan-500/20'
            : 'bg-[#061820] hover:bg-cyan-950/60 border-cyan-500/20 hover:border-cyan-400 text-slate-300 hover:text-rose-400'}"
        title="Список бажань"
      >
        <Heart class="w-4 h-4 {$wishlistStore.items.length > 0 || $uiStore.activeTab === 'wishlist' ? 'fill-rose-500 text-rose-500' : 'group-hover:scale-110'}" />
        {#if $wishlistStore.items.length > 0}
          <span class="absolute -top-1.5 -right-1.5 min-w-[18px] h-[18px] px-1 rounded-full bg-rose-600 text-white text-[10px] font-bold flex items-center justify-center shadow-md shadow-rose-600/40 animate-in zoom-in">
            {$wishlistStore.items.length}
          </span>
        {/if}
      </button>

      <button
        onclick={() => uiStore.setTab('cart')}
        class="relative p-2 rounded-xl border transition-all cursor-pointer group
          {$uiStore.activeTab === 'cart'
            ? 'bg-cyan-500/20 border-cyan-400 text-cyan-300 shadow-md shadow-cyan-500/20'
            : 'bg-[#061820] hover:bg-cyan-950/60 border-cyan-500/20 hover:border-cyan-400 text-slate-300 hover:text-cyan-300'}"
        title="Кошик"
      >
        <ShoppingCart class="w-4 h-4 {$cartStore.items.length > 0 || $uiStore.activeTab === 'cart' ? 'text-cyan-400' : 'group-hover:scale-110'}" />
        {#if $cartStore.items.length > 0}
          <span class="absolute -top-1.5 -right-1.5 min-w-[18px] h-[18px] px-1 rounded-full bg-cyan-500 text-black text-[10px] font-black flex items-center justify-center shadow-md shadow-cyan-500/40 animate-in zoom-in">
            {$cartStore.items.length}
          </span>
        {/if}
      </button>

      {#if $currentUser}

        <button
          onclick={() => uiStore.setDepositModal(true)}
          class="hidden sm:flex items-center gap-1.5 px-2.5 py-1.5 rounded-xl bg-gradient-to-r from-[#07212b] to-[#061820] hover:from-cyan-950/80 hover:to-[#072836] border border-cyan-500/30 hover:border-cyan-400/80 text-xs transition-all cursor-pointer shadow-inner group"
          title="Поповнити баланс (The Open Network)"
        >
          <span class="text-cyan-400 font-bold text-xs group-hover:scale-110 transition-transform">💎</span>
          <span class="font-bold text-white font-mono text-xs tracking-tight">
            {formatTon(nanoTonToTon($currentUser.balanceInNanoTons))}
          </span>
          <span class="w-4 h-4 rounded-md bg-cyan-500/20 text-cyan-300 group-hover:bg-cyan-400 group-hover:text-black flex items-center justify-center text-[11px] font-black transition-all ml-0.5 shadow-sm">
            +
          </span>
        </button>

        <div class="relative">
          <button
            onclick={() => isUserDropdownOpen = !isUserDropdownOpen}
            class="flex items-center gap-2 p-1.5 pl-2.5 rounded-xl bg-[#061820] hover:bg-cyan-950/60 border border-cyan-500/30 transition-all cursor-pointer"
          >
            <div class="w-7 h-7 rounded-lg bg-gradient-to-br from-cyan-400 to-emerald-500 flex items-center justify-center text-black font-black text-xs">
              {#if $currentUser.avatarUrl}
                <img src={$currentUser.avatarUrl} alt={$currentUser.username} class="w-full h-full rounded-lg object-cover" />
              {:else}
                {$currentUser.username.charAt(0).toUpperCase()}
              {/if}
            </div>
            <span class="hidden sm:block text-xs font-bold text-slate-200">
              {$currentUser.username}
            </span>
            <ChevronDown class="w-3.5 h-3.5 text-slate-400" />
          </button>

          {#if isUserDropdownOpen}
            <div class="absolute right-0 mt-2 w-56 bg-[#09151e] border border-cyan-500/30 rounded-2xl shadow-2xl p-1.5 z-50 animate-in fade-in slide-in-from-top-2">
              <div class="px-3 py-2 border-b border-cyan-950/80 text-[11px] text-slate-400">
                <p class="font-bold text-white truncate">{$currentUser.username}</p>
                <p class="text-[10px] text-cyan-400/80 truncate">{$currentUser.email}</p>
                <div class="flex items-center justify-between mt-2 pt-2 border-t border-slate-800/80">
                  <span class="text-slate-400 text-[10px]">Баланс:</span>
                  <span class="text-cyan-300 font-bold font-mono text-[11px]">
                    💎 {formatTon(nanoTonToTon($currentUser.balanceInNanoTons))}
                  </span>
                </div>
              </div>

              <button
                onclick={() => { myProfileStore.viewMyProfile(); isUserDropdownOpen = false; }}
                class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-slate-200 cursor-pointer font-bold mt-1"
              >
                <User class="w-3.5 h-3.5 text-cyan-400" /> Мій профіль
              </button>

              <button
                onclick={() => { uiStore.setTab('library'); isUserDropdownOpen = false; }}
                class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-slate-200 cursor-pointer font-bold mt-1"
              >
                <Library class="w-3.5 h-3.5 text-cyan-400" /> Бібліотека
              </button>

              <button
                onclick={() => { uiStore.setTab('community'); isUserDropdownOpen = false; }}
                class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-slate-200 cursor-pointer font-bold"
              >
                <Users class="w-3.5 h-3.5 text-cyan-400" /> Спільнота
              </button>

              <button
                onclick={() => { uiStore.setDepositModal(true); isUserDropdownOpen = false; }}
                class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-cyan-300 cursor-pointer font-bold mt-1"
              >
                <Coins class="w-3.5 h-3.5 text-cyan-400" /> Поповнити баланс (TON)
              </button>

              <button
                onclick={() => { uiStore.setTab('friends'); isUserDropdownOpen = false; }}
                class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-slate-200 hover:text-white cursor-pointer font-bold mt-1"
              >
                <Users class="w-3.5 h-3.5 text-cyan-400" /> Друзі
                {#if $friendsStore.requests.length > 0}
                  <span class="ml-auto px-1.5 py-0.5 rounded-md bg-[#0df2c9] text-black text-[10px] font-black">
                    +{$friendsStore.requests.length}
                  </span>
                {/if}
              </button>

              {#if $currentUser.isAdmin}
                <button
                  onclick={() => { uiStore.setTab('admin'); isUserDropdownOpen = false; }}
                  class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-cyan-400 cursor-pointer font-bold mt-1"
                >
                  <Shield class="w-3.5 h-3.5" /> Панель Адміністратора
                </button>
              {/if}

              <button
                onclick={handleLogout}
                class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-red-500/10 text-red-400 cursor-pointer font-bold mt-1"
              >
                <LogOut class="w-3.5 h-3.5" /> Вийти з акаунту
              </button>
            </div>
          {/if}
        </div>
      {:else}
        <div class="flex items-center gap-1.5">
          <button
            onclick={() => uiStore.setLoginModal(true)}
            class="px-3 py-1.5 rounded-xl bg-[#061820] hover:bg-cyan-950/60 text-xs font-bold text-slate-200 border border-cyan-500/30 hover:border-cyan-400 transition-all cursor-pointer flex items-center gap-1.5"
          >
            <LogIn class="w-3.5 h-3.5 text-cyan-400" />
            <span class="hidden sm:inline">Увійти</span>
          </button>
          <button
            onclick={() => uiStore.setTab('register')}
            class="px-3 py-1.5 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-xs font-black text-black shadow-md shadow-cyan-500/20 transition-all cursor-pointer flex items-center gap-1.5"
          >
            <UserPlus class="w-3.5 h-3.5 text-black" />
            <span class="hidden sm:inline">Реєстрація</span>
          </button>
        </div>
      {/if}
    </div>
  </div>

  <div bind:this={categoriesModalEl}>
    <SearchCategoriesModal
      isOpen={isCategoriesModalOpen}
      onSelectFilter={handleSelectCategoryFilter}
      onClose={() => isCategoriesModalOpen = false}
    />
  </div>
</header>

