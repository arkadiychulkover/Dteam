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
    Users,
    Sparkles,
    Menu,
    X,
    Code2,
    MessageSquare
  } from 'lucide-svelte';
  import TonIcon from '../ui/TonIcon.svelte';
  import { formatAddress, formatTon, nanoTonToTon } from '../../utils/formatters';
  import { friendsStore } from '../../stores/friendsStore';
  import { totalChatUnreadCount } from '../../stores/chatStore';
  import { gamesService } from '../../services/gamesService';
  import type { GameRecommendation } from '../../types';
  import type { CatalogFilterState } from '../../stores/gamesStore';
  import GameRecommendationsDropdown from '../ui/GameRecommendationsDropdown.svelte';
  import SearchCategoriesModal from '../ui/SearchCategoriesModal.svelte';

  let isUserDropdownOpen = $state(false);
  let isMobileMenuOpen = $state(false);
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
    { id: 'chat', label: 'Чат', icon: MessageSquare },
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

<header class="sticky top-0 z-40 bg-[#030d12]/90 backdrop-blur-xl border-b border-cyan-500/20 px-3 sm:px-4 lg:px-8 py-2.5 sm:py-3 transition-all">
  <div class="max-w-7xl mx-auto flex items-center justify-between gap-2 sm:gap-4">

    <!-- Left: Logo & Desktop Navigation -->
    <div class="flex items-center gap-3 sm:gap-6 shrink-0">
      <!-- Mobile Hamburger Button -->
      <button
        onclick={() => isMobileMenuOpen = true}
        class="lg:hidden p-2 rounded-xl bg-[#061820] hover:bg-cyan-950/60 border border-cyan-500/30 text-cyan-400 cursor-pointer transition-all shrink-0"
        title="Меню навігації"
      >
        <Menu class="w-5 h-5" />
      </button>

      <!-- Logo -->
      <button
        onclick={handleLogoClick}
        class="flex items-center gap-2 group cursor-pointer text-left shrink-0"
      >
        <div class="w-8 h-8 sm:w-9 sm:h-9 rounded-2xl bg-gradient-to-br from-cyan-400 via-teal-500 to-emerald-400 flex items-center justify-center text-black font-black shadow-lg shadow-cyan-500/30 group-hover:scale-105 transition-transform shrink-0">
          <Gamepad2 class="w-4 h-4 sm:w-5 sm:h-5" />
        </div>
        <div class="hidden xs:block">
          <span class="font-black text-lg sm:text-xl tracking-tighter text-white font-display flex items-center gap-1 leading-none">
            DTEAM<span class="text-cyan-400">.</span>
          </span>
          <span class="block text-[7px] sm:text-[8px] font-bold text-cyan-400/90 tracking-widest uppercase mt-0.5">GAMING HUB</span>
        </div>
      </button>

      <!-- Desktop Nav -->
      <nav class="hidden lg:flex items-center gap-1 bg-[#061820]/90 p-1 rounded-2xl border border-cyan-500/20 shadow-inner">
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
            {#if tab.id === 'chat' && $totalChatUnreadCount > 0}
              <span class="min-w-[16px] h-4 px-1 rounded-full bg-rose-500 text-white text-[9px] font-black flex items-center justify-center shadow-sm">
                {$totalChatUnreadCount > 99 ? '99+' : $totalChatUnreadCount}
              </span>
            {/if}
            {#if tab.id === 'chat' && $uiStore.activeTab === 'chat'}
              <span class="w-1.5 h-1.5 rounded-full bg-cyan-300 absolute -bottom-1 left-1/2 -translate-x-1/2 shadow-[0_0_8px_#0df2c9]"></span>
            {/if}
          </button>
        {/each}
      </nav>
    </div>

    <!-- Middle: Search Input -->
    <div bind:this={searchWrapperEl} class="relative flex-1 max-w-xs sm:max-w-sm md:max-w-md mx-1 sm:mx-2 transition-all">
      <form onsubmit={handleSearchSubmit} class="relative w-full">
        <input
          type="text"
          placeholder="Пошук у Крамниці..."
          bind:value={headerSearchQuery}
          oninput={handleSearchInput}
          onfocus={handleSearchFocus}
          class="w-full pl-3 sm:pl-4 pr-8 sm:pr-10 py-1.5 sm:py-2 rounded-2xl bg-[#061820]/90 hover:bg-[#07212b] border border-cyan-500/30 focus:border-cyan-400 focus:shadow-[0_0_15px_rgba(13,242,201,0.25)] focus:outline-none text-xs text-white placeholder-slate-400 transition-all shadow-inner"
        />
        <button
          type="submit"
          class="absolute right-2 sm:right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-cyan-400 p-1 cursor-pointer transition-colors"
          title="Пошук"
        >
          <Search class="w-3.5 h-3.5 sm:w-4 sm:h-4" />
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

    <!-- Right: Wishlist, Cart, Balance & Profile -->
    <div class="flex items-center gap-1.5 sm:gap-3 shrink-0 ml-auto">
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
        <!-- Balance Pill: Click to go to Wallet, '+' opens Deposit Modal -->
        <div class="hidden sm:flex items-center rounded-xl bg-gradient-to-r from-[#07212b] to-[#061820] border border-cyan-500/30 hover:border-cyan-400/80 transition-all shadow-inner overflow-hidden">
          <button
            onclick={() => uiStore.setTab('wallet')}
            class="flex items-center gap-1.5 px-2.5 py-1.5 hover:bg-cyan-950/80 text-xs transition-colors cursor-pointer group"
            title="Перейти до гаманця"
          >
            <TonIcon class="w-3.5 h-3.5 text-cyan-400 group-hover:scale-110 transition-transform" />
            <span class="font-bold text-white font-mono text-xs tracking-tight">
              {formatTon(nanoTonToTon($currentUser.balanceInNanoTons))}
            </span>
          </button>
          <button
            onclick={() => uiStore.setDepositModal(true)}
            class="px-2 py-1.5 bg-cyan-500/20 hover:bg-cyan-400 text-cyan-300 hover:text-black text-[11px] font-black transition-all border-l border-cyan-500/30 cursor-pointer"
            title="Швидке поповнення балансу (TON)"
          >
            +
          </button>
        </div>

        <div class="relative">
          <button
            onclick={() => isUserDropdownOpen = !isUserDropdownOpen}
            class="flex items-center gap-1.5 sm:gap-2 p-1 sm:p-1.5 sm:pl-2.5 rounded-xl bg-[#061820] hover:bg-cyan-950/60 border border-cyan-500/30 transition-all cursor-pointer"
          >
            <div class="w-7 h-7 rounded-lg bg-gradient-to-br from-cyan-400 to-emerald-500 flex items-center justify-center text-black font-black text-xs shrink-0">
              {#if $currentUser.avatarUrl}
                <img src={$currentUser.avatarUrl} alt={$currentUser.username} class="w-full h-full rounded-lg object-cover" />
              {:else}
                {$currentUser.username.charAt(0).toUpperCase()}
              {/if}
            </div>
            <span class="hidden md:block text-xs font-bold text-slate-200">
              {$currentUser.username}
            </span>
            <ChevronDown class="w-3.5 h-3.5 text-slate-400" />
          </button>

          {#if isUserDropdownOpen}
            <div class="absolute right-0 mt-2 w-56 bg-[#09151e] border border-cyan-500/30 rounded-2xl shadow-2xl p-1.5 z-50 animate-in fade-in slide-in-from-top-2">
              <div class="px-3 py-2 border-b border-cyan-950/80 text-[11px] text-slate-400">
                <p class="font-bold text-white truncate">{$currentUser.username}</p>
                <p class="text-[10px] text-cyan-400/80 truncate">{$currentUser.email}</p>
                <button
                  onclick={() => { uiStore.setTab('wallet'); isUserDropdownOpen = false; }}
                  class="w-full flex items-center justify-between mt-2 pt-2 border-t border-slate-800/80 hover:text-cyan-300 cursor-pointer transition-colors"
                >
                  <span class="text-slate-400 text-[10px]">Баланс:</span>
                  <span class="text-cyan-300 font-bold font-mono text-[11px] flex items-center gap-1">
                    <TonIcon class="w-3 h-3 text-cyan-400" />
                    <span>{formatTon(nanoTonToTon($currentUser.balanceInNanoTons))}</span>
                  </span>
                </button>
              </div>

              <button
                onclick={async () => { uiStore.setTab('my-profile'); isUserDropdownOpen = false; }}
                class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-slate-200 cursor-pointer font-bold mt-1"
              >
                <User class="w-3.5 h-3.5 text-cyan-400" /> Мій профіль
              </button>

              <button
                onclick={() => { uiStore.setTab('wallet'); isUserDropdownOpen = false; }}
                class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-cyan-300 hover:text-white cursor-pointer font-bold mt-1"
              >
                <Wallet class="w-3.5 h-3.5 text-cyan-400" /> Мій гаманець
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

              <button
                onclick={() => { uiStore.setTab('chat'); isUserDropdownOpen = false; }}
                class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-slate-200 hover:text-white cursor-pointer font-bold mt-1"
              >
                <MessageSquare class="w-3.5 h-3.5 text-cyan-400" /> Чат
                {#if $totalChatUnreadCount > 0}
                  <span class="ml-auto px-1.5 py-0.5 rounded-md bg-rose-500 text-white text-[10px] font-black">
                    {$totalChatUnreadCount}
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
                onclick={() => { uiStore.setTab('developer'); isUserDropdownOpen = false; }}
                class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-emerald-400 hover:text-emerald-300 cursor-pointer font-bold mt-1"
              >
                <Code2 class="w-3.5 h-3.5 text-emerald-400" /> Кабінет розробника
              </button>

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
        <div class="flex items-center gap-1 sm:gap-1.5">
          <button
            onclick={() => uiStore.setLoginModal(true)}
            class="px-2.5 sm:px-3 py-1.5 rounded-xl bg-[#061820] hover:bg-cyan-950/60 text-xs font-bold text-slate-200 border border-cyan-500/30 hover:border-cyan-400 transition-all cursor-pointer flex items-center gap-1.5"
          >
            <LogIn class="w-3.5 h-3.5 text-cyan-400" />
            <span class="hidden sm:inline">Увійти</span>
          </button>
          <button
            onclick={() => uiStore.setTab('register')}
            class="px-2.5 sm:px-3 py-1.5 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-xs font-black text-black shadow-md shadow-cyan-500/20 transition-all cursor-pointer flex items-center gap-1.5"
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

<!-- Mobile Slide-over Drawer Menu -->
{#if isMobileMenuOpen}
  <div class="fixed inset-0 z-50 lg:hidden flex">
    <!-- Backdrop -->
    <button
      type="button"
      onclick={() => isMobileMenuOpen = false}
      class="fixed inset-0 bg-black/80 backdrop-blur-sm animate-in fade-in"
      aria-label="Закрити меню"
    ></button>

    <!-- Drawer Panel -->
    <div class="relative w-4/5 max-w-sm bg-[#05151e] border-r border-cyan-500/30 p-5 flex flex-col justify-between shadow-2xl z-10 animate-in slide-in-from-left duration-200">
      <div class="space-y-6 overflow-y-auto">
        <!-- Top Drawer Header -->
        <div class="flex items-center justify-between border-b border-cyan-500/20 pb-4">
          <div class="flex items-center gap-2.5">
            <div class="w-8 h-8 rounded-xl bg-gradient-to-br from-cyan-400 to-emerald-400 flex items-center justify-center text-black font-black">
              <Gamepad2 class="w-4 h-4" />
            </div>
            <div>
              <span class="font-black text-base text-white tracking-tight font-display">DTEAM</span>
              <span class="block text-[8px] font-bold text-cyan-400/90 tracking-widest uppercase">GAMING HUB</span>
            </div>
          </div>
          <button
            onclick={() => isMobileMenuOpen = false}
            class="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- User Profile summary (if logged in) -->
        {#if $currentUser}
          <div class="p-3.5 rounded-2xl bg-[#09222c] border border-cyan-500/25 space-y-3">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-400 to-emerald-500 flex items-center justify-center text-black font-black text-sm shrink-0">
                {#if $currentUser.avatarUrl}
                  <img src={$currentUser.avatarUrl} alt={$currentUser.username} class="w-full h-full rounded-xl object-cover" />
                {:else}
                  {$currentUser.username.charAt(0).toUpperCase()}
                {/if}
              </div>
              <div class="min-w-0 flex-1">
                <p class="font-bold text-sm text-white truncate">{$currentUser.username}</p>
                <p class="text-[11px] text-cyan-400/80 truncate">{$currentUser.email}</p>
              </div>
            </div>

            <!-- Balance & Deposit Button -->
            <div class="flex items-center justify-between pt-2 border-t border-cyan-950/80">
              <div>
                <span class="text-[10px] text-slate-400">Баланс TON:</span>
                <p class="text-xs font-bold text-white font-mono flex items-center gap-1">
                  <TonIcon class="w-3 h-3 text-cyan-400" />
                  <span>{formatTon(nanoTonToTon($currentUser.balanceInNanoTons))} TON</span>
                </p>
              </div>
              <button
                onclick={() => { uiStore.setDepositModal(true); isMobileMenuOpen = false; }}
                class="px-2.5 py-1 rounded-lg bg-cyan-500 hover:bg-cyan-400 text-black text-[11px] font-black tracking-wider transition-all cursor-pointer"
              >
                + Поповнити
              </button>
            </div>
          </div>
        {/if}

        <!-- Navigation Links List -->
        <div class="space-y-1">
          <p class="text-[10px] font-bold text-slate-500 uppercase tracking-wider px-2 mb-2">Навігація</p>
          {#each visibleTabs as tab}
            {@const Icon = tab.icon}
            <button
              onclick={() => { uiStore.setTab(tab.id); isMobileMenuOpen = false; }}
              class="w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-xs font-bold transition-all cursor-pointer
                {$uiStore.activeTab === tab.id
                  ? 'bg-gradient-to-r from-cyan-500 to-emerald-500 text-black shadow-md shadow-cyan-500/20 font-black'
                  : 'text-slate-300 hover:bg-cyan-500/10 hover:text-white'}"
            >
              <div class="flex items-center gap-3">
                <Icon class="w-4 h-4 {$uiStore.activeTab === tab.id ? 'text-black' : 'text-cyan-400'}" />
                <span>{tab.label}</span>
              </div>
              {#if tab.id === 'chat' && $totalChatUnreadCount > 0}
                <span class="px-1.5 py-0.5 rounded-full bg-rose-500 text-white text-[10px] font-black">
                  {$totalChatUnreadCount}
                </span>
              {/if}
              {#if tab.id === 'friends' && $friendsStore.requests.length > 0}
                <span class="px-1.5 py-0.5 rounded-full bg-[#0df2c9] text-black text-[10px] font-black">
                  +{$friendsStore.requests.length}
                </span>
              {/if}
            </button>
          {/each}

          {#if $currentUser}
            <div class="pt-3 border-t border-cyan-950/80 space-y-1">
              <p class="text-[10px] font-bold text-slate-500 uppercase tracking-wider px-2 mb-2">Акаунт</p>
              <button
                onclick={() => { uiStore.setTab('wallet'); isMobileMenuOpen = false; }}
                class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-bold text-cyan-300 hover:bg-cyan-500/10 cursor-pointer"
              >
                <Wallet class="w-4 h-4 text-cyan-400" />
                <span>Мій гаманець</span>
              </button>
              <button
                onclick={() => { uiStore.setTab('my-profile'); isMobileMenuOpen = false; }}
                class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-bold text-slate-300 hover:bg-cyan-500/10 cursor-pointer"
              >
                <User class="w-4 h-4 text-cyan-400" />
                <span>Мій профіль</span>
              </button>
              <button
                onclick={() => { uiStore.setTab('developer'); isMobileMenuOpen = false; }}
                class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-bold text-emerald-400 hover:bg-cyan-500/10 cursor-pointer"
              >
                <Code2 class="w-4 h-4 text-emerald-400" />
                <span>Кабінет розробника</span>
              </button>
            </div>
          {/if}
        </div>
      </div>

      <!-- Drawer Bottom: Auth Actions -->
      <div class="pt-4 border-t border-cyan-500/20">
        {#if $currentUser}
          <button
            onclick={handleLogout}
            class="w-full flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-red-500/10 hover:bg-red-500/20 text-red-400 text-xs font-bold transition-all cursor-pointer"
          >
            <LogOut class="w-4 h-4" />
            <span>Вийти з акаунту</span>
          </button>
        {:else}
          <div class="grid grid-cols-2 gap-2">
            <button
              onclick={() => { uiStore.setLoginModal(true); isMobileMenuOpen = false; }}
              class="px-3 py-2 rounded-xl bg-[#09222c] text-xs font-bold text-slate-200 border border-cyan-500/30 text-center"
            >
              Увійти
            </button>
            <button
              onclick={() => { uiStore.setTab('register'); isMobileMenuOpen = false; }}
              class="px-3 py-2 rounded-xl bg-cyan-500 text-black text-xs font-bold text-center"
            >
              Реєстрація
            </button>
          </div>
        {/if}
      </div>
    </div>
  </div>
{/if}

<!-- Mobile Bottom Navigation Bar (Quick Switching on Phone screens) -->
<nav class="fixed bottom-0 left-0 right-0 z-40 bg-[#030e14]/95 backdrop-blur-xl border-t border-cyan-500/20 px-2 py-1.5 flex lg:hidden items-center justify-around shadow-2xl safe-area-pb">
  <button
    onclick={() => uiStore.setTab('store')}
    class="flex flex-col items-center gap-0.5 px-3 py-1 rounded-xl text-[10px] font-bold transition-colors cursor-pointer
      {$uiStore.activeTab === 'store' ? 'text-cyan-400' : 'text-slate-400 hover:text-white'}"
  >
    <Gamepad2 class="w-4 h-4" />
    <span>Крамниця</span>
  </button>

  <button
    onclick={() => uiStore.setTab('catalog')}
    class="flex flex-col items-center gap-0.5 px-3 py-1 rounded-xl text-[10px] font-bold transition-colors cursor-pointer
      {$uiStore.activeTab === 'catalog' ? 'text-cyan-400' : 'text-slate-400 hover:text-white'}"
  >
    <Compass class="w-4 h-4" />
    <span>Каталог</span>
  </button>

  <button
    onclick={() => uiStore.setTab('chat')}
    class="flex flex-col items-center gap-0.5 px-3 py-1 rounded-xl text-[10px] font-bold transition-colors cursor-pointer relative
      {$uiStore.activeTab === 'chat' ? 'text-cyan-400' : 'text-slate-400 hover:text-white'}"
  >
    <MessageSquare class="w-4 h-4" />
    <span>Чат</span>
    {#if $totalChatUnreadCount > 0}
      <span class="absolute top-0 right-1 min-w-[14px] h-[14px] px-0.5 rounded-full bg-rose-500 text-white text-[8px] font-black flex items-center justify-center shadow-sm">
        {$totalChatUnreadCount > 99 ? '99+' : $totalChatUnreadCount}
      </span>
    {/if}
  </button>

  <button
    onclick={() => uiStore.setTab('library')}
    class="flex flex-col items-center gap-0.5 px-3 py-1 rounded-xl text-[10px] font-bold transition-colors cursor-pointer
      {$uiStore.activeTab === 'library' ? 'text-cyan-400' : 'text-slate-400 hover:text-white'}"
  >
    <Library class="w-4 h-4" />
    <span>Бібліотека</span>
  </button>

  <button
    onclick={() => uiStore.setTab('wallet')}
    class="flex flex-col items-center gap-0.5 px-3 py-1 rounded-xl text-[10px] font-bold transition-colors cursor-pointer
      {$uiStore.activeTab === 'wallet' ? 'text-cyan-400' : 'text-slate-400 hover:text-white'}"
  >
    <Wallet class="w-4 h-4" />
    <span>Гаманець</span>
  </button>
</nav>