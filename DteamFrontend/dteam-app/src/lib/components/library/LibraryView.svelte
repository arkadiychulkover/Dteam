<script lang="ts">
  import { onMount } from 'svelte';
  import { libraryStore } from '../../stores/libraryStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { uiStore } from '../../stores/uiStore';
  import { profileStore } from '../../stores/profileStore';
  import { communityService, type CommunityPost } from '../../services/communityService';
  import { formatPlayTime, formatDate, formatBytes } from '../../utils/formatters';
  import {
    Play,
    Star,
    Clock,
    Settings,
    Folder,
    CheckCircle,
    Gamepad2,
    Loader2,
    Search,
    LayoutGrid,
    List as ListIcon,
    SlidersHorizontal,
    ChevronRight,
    ThumbsUp,
    MessageSquare,
    ExternalLink
  } from 'lucide-svelte';

  onMount(() => {
    libraryStore.loadLibrary();
    if ($gamesStore.games.length === 0) {
      gamesStore.loadGames();
    }
    loadNews();
    loadHighlights();
  });

  const userGames = $derived($libraryStore.items);

  function getGameFor(item: (typeof userGames)[number]) {
    return item.game || $gamesStore.games.find((x) => x.id === item.gameId);
  }

  // ==== Верхня панель: пошук по бібліотеці + вигляд (сітка/список) ====
  let searchQuery = $state('');
  let viewMode = $state<'grid' | 'list'>('grid');
  let activeLibraryTab = $state<'all' | 'favorites' | 'collection'>('all');

  const libraryTabLabels: Record<'all' | 'favorites' | 'collection', string> = {
    all: 'Усі ігри',
    favorites: 'Обране',
    collection: 'Моя колекція'
  };

  // "Моя колекція" наразі відображає всі придбані ігри — окремого поділу на
  // колекції бекенд ще не підтримує, тож фільтруємо лише за "Обране".
  const filteredUserGames = $derived.by(() => {
    let list = activeLibraryTab === 'favorites' ? userGames.filter((i) => i.isFavorite) : userGames;

    const q = searchQuery.trim().toLowerCase();
    if (!q) return list;

    return list.filter((item) => {
      const g = getGameFor(item);
      return g?.title.toLowerCase().includes(q);
    });
  });

  // ==== Постійна ліва панель "Мої ігри" ====
  let sidebarSearchQuery = $state('');

  const sidebarFilteredGames = $derived.by(() => {
    const q = sidebarSearchQuery.trim().toLowerCase();
    if (!q) return userGames;
    return userGames.filter((item) => {
      const g = getGameFor(item);
      return g?.title.toLowerCase().includes(q);
    });
  });

  // ==== Стрічки спільноти на головній сторінці бібліотеки ====
  let newsPosts = $state<CommunityPost[]>([]);
  let isLoadingNews = $state(false);
  let highlightPosts = $state<CommunityPost[]>([]);
  let isLoadingHighlights = $state(false);

  async function loadNews() {
    isLoadingNews = true;
    try {
      const res = await communityService.getPosts(null, 'news', '', 'newest');
      newsPosts = (res.posts || []).slice(0, 8);
    } catch (e) {
      console.warn('[LibraryView] Не вдалося завантажити новини:', e);
      newsPosts = [];
    } finally {
      isLoadingNews = false;
    }
  }

  async function loadHighlights() {
    isLoadingHighlights = true;
    try {
      const res = await communityService.getPosts(null, 'all', '', 'popular');
      highlightPosts = (res.posts || []).slice(0, 8);
    } catch (e) {
      console.warn('[LibraryView] Не вдалося завантажити стрічку спільноти:', e);
      highlightPosts = [];
    } finally {
      isLoadingHighlights = false;
    }
  }

  function openPostAuthor(authorId: string) {
    if (authorId && authorId !== 'guest') {
      profileStore.viewProfile(authorId);
    }
  }

  // ==== Деталі та запуск гри (майстер-деталь панель) ====
  const selectedUserGame = $derived(
    userGames.find((g) => g.gameId === $libraryStore.selectedGameId) || null
  );
  const selectedGameDetails = $derived(
    selectedUserGame
      ? selectedUserGame.game || $gamesStore.games.find((g) => g.id === selectedUserGame.gameId)
      : null
  );

  let isDetailOpen = $state(false);
  let isLaunching = $state(false);

  function openGameDetail(gameId: string) {
    libraryStore.selectGame(gameId);
    isDetailOpen = true;
  }

  function closeGameDetail() {
    isDetailOpen = false;
  }

  function launchGame() {
    isLaunching = true;
    uiStore.addToast({
      title: 'Запуск гри...',
      message: `Запускаємо ${selectedGameDetails?.title}. Гарної гри!`,
      type: 'info'
    });
    setTimeout(() => {
      isLaunching = false;
    }, 2000);
  }

  function openFullStorePage() {
    if (!selectedGameDetails) return;
    gamesStore.selectGame(selectedGameDetails);
    uiStore.setTab('game');
  }

  function toggleFilters() {
    uiStore.addToast({
      title: 'Фільтри',
      message: 'Розширені фільтри бібліотеки скоро з’являться.',
      type: 'info'
    });
  }
</script>

<div class="max-w-7xl mx-auto px-4 lg:px-8 py-6">
  {#if $libraryStore.isLoading && !$libraryStore.hasLoaded}
    <div class="flex items-center justify-center py-24 bg-slate-900/60 rounded-2xl border border-slate-800">
      <Loader2 class="w-10 h-10 text-cyan-400 animate-spin" />
    </div>
  {:else if userGames.length === 0}
    <div class="text-center py-24 bg-slate-900/60 rounded-2xl border border-slate-800">
      <Gamepad2 class="w-16 h-16 text-cyan-400/40 mx-auto mb-4" />
      <h3 class="text-2xl font-bold text-white">Ваша бібліотека порожня</h3>
      <p class="text-sm text-slate-400 mt-2 max-w-md mx-auto">
        Досліджуйте децентралізовану Web3 крамницю та здобувайте улюблені ігри за TON.
      </p>
      <button
        onclick={() => uiStore.setTab('store')}
        class="mt-6 px-6 py-3 rounded-xl bg-cyan-500 hover:bg-cyan-400 text-black font-bold text-sm transition-all cursor-pointer"
      >
        Переглянути крамницю
      </button>
    </div>
  {:else}
    <!-- ==================== ПОСТІЙНА ЛІВА ПАНЕЛЬ + КОНТЕНТ ==================== -->
    <div class="flex flex-col lg:flex-row items-start gap-6">

      <!-- Ліва панель: усі куплені ігри (завжди видима) -->
      <aside class="w-full lg:w-[300px] shrink-0 lg:sticky lg:top-24">
        <div class="bg-[#0b0d14]/80 border border-slate-800/80 rounded-2xl p-4 flex flex-col max-h-[calc(100vh-140px)]">
          <div class="flex items-center justify-between px-1 mb-3">
            <span class="text-xs font-bold text-slate-400 uppercase tracking-wider">Мої ігри ({userGames.length})</span>
            {#if isDetailOpen}
              <button
                type="button"
                onclick={closeGameDetail}
                class="flex items-center gap-1 text-[11px] font-bold text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer"
                title="Огляд бібліотеки"
              >
                <LayoutGrid class="w-3.5 h-3.5" />
              </button>
            {/if}
          </div>

          <div class="relative mb-3">
            <Search class="w-3.5 h-3.5 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              bind:value={sidebarSearchQuery}
              placeholder="Пошук у бібліотеці..."
              class="w-full pl-8 pr-3 py-2 rounded-xl bg-[#061820]/90 border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-500 transition-all"
            />
          </div>

          <div class="space-y-1 overflow-y-auto flex-1 pr-1">
            {#each sidebarFilteredGames as item (item.gameId)}
              {@const g = getGameFor(item)}
              {#if g}
                <div
                  role="button"
                  tabindex="0"
                  onclick={() => openGameDetail(item.gameId)}
                  onkeydown={(e) => e.key === 'Enter' && openGameDetail(item.gameId)}
                  class="w-full flex items-center justify-between p-2.5 rounded-xl text-left transition-all cursor-pointer group
                    {isDetailOpen && $libraryStore.selectedGameId === item.gameId
                      ? 'bg-gradient-to-r from-cyan-950/90 to-blue-950/60 border border-cyan-500/40 text-white shadow-md'
                      : 'hover:bg-slate-800/50 text-slate-300 border border-transparent'}"
                >
                  <div class="flex items-center gap-3 min-w-0">
                    <img
                      src={g.coverImageUrl || g.headerImageUrl}
                      alt={g.title}
                      class="w-9 h-9 rounded-lg object-cover ring-1 ring-slate-700"
                    />
                    <div class="truncate">
                      <span class="block text-xs font-bold truncate group-hover:text-cyan-300 transition-colors">
                        {g.title}
                      </span>
                      <span class="block text-[10px] text-slate-500 mt-0.5">
                        {formatPlayTime(item.playTimeMinutes)} у грі
                      </span>
                    </div>
                  </div>

                  <button
                    type="button"
                    onclick={(e) => { e.stopPropagation(); libraryStore.toggleFavorite(item.gameId); }}
                    class="p-1 text-slate-500 hover:text-amber-400 transition-colors cursor-pointer"
                    title="Додати/прибрати з обраного"
                  >
                    <Star class="w-3.5 h-3.5 {item.isFavorite ? 'fill-amber-400 text-amber-400' : ''}" />
                  </button>
                </div>
              {/if}
            {/each}
            {#if sidebarFilteredGames.length === 0}
              <p class="text-[11px] text-slate-500 text-center py-6">Нічого не знайдено.</p>
            {/if}
          </div>
        </div>
      </aside>

      <!-- Права колонка: деталі активної гри або огляд бібліотеки -->
      <div class="flex-1 min-w-0 w-full">
        {#if isDetailOpen && selectedGameDetails && selectedUserGame}
          <!-- ==================== РЕЖИМ ЗАПУСКУ ГРИ ==================== -->
          <div class="flex flex-col justify-between p-6 md:p-8 relative bg-gradient-to-b from-[#131726]/60 to-[#0e111a] border border-slate-800/80 rounded-2xl overflow-hidden shadow-2xl min-h-[680px]">
        <div class="absolute inset-0 opacity-20 pointer-events-none overflow-hidden">
          <img
            src={selectedGameDetails.headerImageUrl || selectedGameDetails.coverImageUrl}
            alt=""
            class="w-full h-full object-cover filter blur-md"
          />
        </div>

        <div class="relative z-10">
          <div class="flex items-center justify-between gap-4">
            <span class="text-xs px-2.5 py-1 rounded-md bg-cyan-950 text-cyan-400 border border-cyan-500/30 font-semibold uppercase tracking-wider">
              Встановлено та перевірено
            </span>
            <span class="text-xs text-slate-400">
              Версія: <span class="font-mono text-slate-200">{selectedGameDetails.version}</span>
            </span>
          </div>

          <h2 class="text-3xl md:text-5xl font-black text-white font-display mt-4">
            {selectedGameDetails.title}
          </h2>

          <p class="text-xs md:text-sm text-slate-300 max-w-2xl mt-2 leading-relaxed">
            {selectedGameDetails.description}
          </p>

          <div class="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-6 p-4 rounded-xl bg-slate-900/80 border border-slate-800">
            <div>
              <span class="text-[10px] text-slate-400 block uppercase font-semibold">Час у грі</span>
              <span class="text-sm font-bold text-white flex items-center gap-1.5 mt-0.5">
                <Clock class="w-3.5 h-3.5 text-cyan-400" />
                {formatPlayTime(selectedUserGame.playTimeMinutes)}
              </span>
            </div>
            <div>
              <span class="text-[10px] text-slate-400 block uppercase font-semibold">Востаннє грали</span>
              <span class="text-sm font-bold text-white mt-0.5 block">
                {selectedUserGame.lastPlayedAt ? formatDate(selectedUserGame.lastPlayedAt) : 'Ніколи'}
              </span>
            </div>
            <div>
              <span class="text-[10px] text-slate-400 block uppercase font-semibold">Розмір</span>
              <span class="text-sm font-bold text-white mt-0.5 block">
                {formatBytes(selectedGameDetails.sizeInBytes)}
              </span>
            </div>
            <div>
              <span class="text-[10px] text-slate-400 block uppercase font-semibold">Хмарна синхронізація</span>
              <span class="text-sm font-bold text-emerald-400 flex items-center gap-1 mt-0.5">
                <CheckCircle class="w-3.5 h-3.5" /> Синхронізовано
              </span>
            </div>
          </div>
        </div>

        <div class="relative z-10 mt-8 pt-6 border-t border-slate-800/80 flex flex-wrap items-center justify-between gap-4">
          <button
            onclick={launchGame}
            disabled={isLaunching}
            class="flex items-center gap-3 px-8 py-4 rounded-xl bg-gradient-to-r from-emerald-500 to-teal-600 hover:from-emerald-400 hover:to-teal-500 text-black font-black text-base tracking-wider shadow-lg shadow-emerald-500/25 hover:shadow-emerald-500/40 hover:scale-105 active:scale-95 transition-all cursor-pointer disabled:opacity-70 disabled:cursor-wait"
          >
            <Play class="w-5 h-5 fill-black" />
            <span>{isLaunching ? 'ЗАПУСК...' : 'ГРАТИ'}</span>
          </button>

          <div class="flex items-center gap-2">
            <button
              onclick={openFullStorePage}
              class="flex items-center gap-2 px-4 py-3 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-300 hover:text-white border border-slate-700 transition-colors cursor-pointer text-xs font-bold"
              title="Переглянути сторінку гри у крамниці"
            >
              <ExternalLink class="w-4 h-4" />
              Сторінка гри
            </button>
            <button
              class="p-3 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-300 hover:text-white border border-slate-700 transition-colors cursor-pointer"
              title="Показати у локальній папці"
            >
              <Folder class="w-4 h-4" />
            </button>
            <button
              class="p-3 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-300 hover:text-white border border-slate-700 transition-colors cursor-pointer"
              title="Налаштування гри"
            >
              <Settings class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
        {:else}
        <!-- ==================== РЕЖИМ ОГЛЯДУ БІБЛІОТЕКИ ==================== -->

    <!-- Пошук по бібліотеці + перемикач вигляду -->
    <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 mb-6">
      <div class="relative w-full sm:w-96">
        <Search class="w-4 h-4 text-slate-400 absolute left-4 top-1/2 -translate-y-1/2" />
        <input
          type="text"
          bind:value={searchQuery}
          placeholder="Пошук у бібліотеці..."
          class="w-full pl-10 pr-4 py-2.5 rounded-2xl bg-[#061820]/90 border border-cyan-500/30 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-400 transition-all shadow-inner"
        />
      </div>

      <div class="flex items-center gap-2 shrink-0">
        <button
          onclick={toggleFilters}
          class="flex items-center gap-1.5 px-3.5 py-2.5 rounded-xl bg-[#061820] hover:bg-cyan-950/60 border border-cyan-500/20 hover:border-cyan-400 text-xs font-bold text-slate-300 hover:text-white transition-colors cursor-pointer"
        >
          <SlidersHorizontal class="w-3.5 h-3.5" />
          Фільтри
        </button>

        <div class="flex items-center gap-1 bg-[#061820] p-1 rounded-xl border border-cyan-500/20">
          <button
            onclick={() => (viewMode = 'grid')}
            class="p-2 rounded-lg transition-colors cursor-pointer {viewMode === 'grid' ? 'bg-cyan-500 text-black' : 'text-slate-400 hover:text-white'}"
            title="Сітка"
          >
            <LayoutGrid class="w-4 h-4" />
          </button>
          <button
            onclick={() => (viewMode = 'list')}
            class="p-2 rounded-lg transition-colors cursor-pointer {viewMode === 'list' ? 'bg-cyan-500 text-black' : 'text-slate-400 hover:text-white'}"
            title="Список"
          >
            <ListIcon class="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>

    <!-- Стрічка "Новини" -->
    {#if isLoadingNews}
      <div class="flex items-center gap-2 text-xs text-slate-500 mb-2">
        <Loader2 class="w-3.5 h-3.5 animate-spin" /> Завантаження новин...
      </div>
    {:else if newsPosts.length > 0}
      <div class="flex items-center justify-between mb-3">
        <h2 class="text-lg font-black text-white font-display">Новини</h2>
        <button
          onclick={() => uiStore.setTab('community')}
          class="flex items-center gap-1 text-xs font-bold text-slate-400 hover:text-cyan-300 transition-colors cursor-pointer"
        >
          Усі новини <ChevronRight class="w-3.5 h-3.5" />
        </button>
      </div>
      <div class="flex gap-4 overflow-x-auto pb-3 mb-6 scrollbar-none">
        {#each newsPosts as post (post.id)}
          <div class="bg-[#0d232c] border border-cyan-900/40 rounded-2xl min-w-[300px] max-w-[340px] overflow-hidden shrink-0">
            {#if post.media?.thumbnailUrl || post.media?.url}
              <img src={post.media.thumbnailUrl || post.media.url} alt="" class="w-full h-36 object-cover" />
            {/if}
            <div class="p-3.5">
              <button
                type="button"
                onclick={() => openPostAuthor(post.author.id)}
                class="flex items-center gap-2 text-[11px] text-slate-400 mb-1.5 cursor-pointer hover:text-cyan-300 transition-colors"
              >
                <img src={post.author.avatarUrl} alt={post.author.username} class="w-4 h-4 rounded-full object-cover" />
                {post.author.username}
              </button>
              <h3 class="text-sm font-bold text-white mb-1 line-clamp-1">{post.title}</h3>
              <p class="text-xs text-slate-400 line-clamp-2 mb-3">{post.content}</p>
              <div class="flex items-center gap-3 text-[11px] text-slate-500">
                <span class="flex items-center gap-1"><ThumbsUp class="w-3 h-3" /> {post.stats.likesCount}</span>
                <span class="flex items-center gap-1"><MessageSquare class="w-3 h-3" /> {post.stats.commentsCount}</span>
                <span class="ml-auto">{formatDate(post.createdAt)}</span>
              </div>
            </div>
          </div>
        {/each}
      </div>
    {/if}

    <!-- Стрічка "Цікаве від Спільноти" -->
    {#if isLoadingHighlights}
      <div class="flex items-center gap-2 text-xs text-slate-500 mb-2">
        <Loader2 class="w-3.5 h-3.5 animate-spin" /> Завантаження стрічки спільноти...
      </div>
    {:else if highlightPosts.length > 0}
      <div class="flex items-center justify-between mb-3">
        <h2 class="text-lg font-black text-white font-display">Цікаве від Спільноти</h2>
        <button
          onclick={() => uiStore.setTab('community')}
          class="flex items-center gap-1 text-xs font-bold text-slate-400 hover:text-cyan-300 transition-colors cursor-pointer"
        >
          Моя стрічка <ChevronRight class="w-3.5 h-3.5" />
        </button>
      </div>
      <div class="flex gap-4 overflow-x-auto pb-3 mb-8 scrollbar-none">
        {#each highlightPosts as post (post.id)}
          <div class="bg-[#0d232c] border border-cyan-900/40 rounded-2xl min-w-[300px] max-w-[340px] overflow-hidden shrink-0">
            {#if post.media?.thumbnailUrl || post.media?.url}
              <img src={post.media.thumbnailUrl || post.media.url} alt="" class="w-full h-36 object-cover" />
            {/if}
            <div class="p-3.5">
              <button
                type="button"
                onclick={() => openPostAuthor(post.author.id)}
                class="flex items-center gap-2 text-[11px] text-slate-400 mb-1.5 cursor-pointer hover:text-cyan-300 transition-colors"
              >
                <img src={post.author.avatarUrl} alt={post.author.username} class="w-4 h-4 rounded-full object-cover" />
                {post.author.username}
              </button>
              <p class="text-xs text-slate-300 line-clamp-3 mb-3">{post.content}</p>
              <div class="flex items-center gap-3 text-[11px] text-slate-500">
                <span class="flex items-center gap-1"><ThumbsUp class="w-3 h-3" /> {post.stats.likesCount}</span>
                <span class="flex items-center gap-1"><MessageSquare class="w-3 h-3" /> {post.stats.commentsCount}</span>
              </div>
            </div>
          </div>
        {/each}
      </div>
    {/if}

    <!-- Вкладки бібліотеки -->
    <div class="flex items-center gap-6 mb-5 border-b border-slate-800/80">
      {#each Object.keys(libraryTabLabels) as key (key)}
        {@const tabKey = key as keyof typeof libraryTabLabels}
        <button
          onclick={() => (activeLibraryTab = tabKey)}
          class="pb-3 text-sm font-bold transition-colors cursor-pointer relative
            {activeLibraryTab === tabKey ? 'text-cyan-400' : 'text-slate-400 hover:text-white'}"
        >
          {libraryTabLabels[tabKey]}
          {#if activeLibraryTab === tabKey}
            <span class="absolute left-0 right-0 -bottom-px h-0.5 bg-cyan-400 rounded-full"></span>
          {/if}
        </button>
      {/each}
    </div>

    {#if filteredUserGames.length === 0}
      <div class="text-center py-16 bg-slate-900/60 rounded-2xl border border-slate-800">
        <p class="text-sm text-slate-400">Ігор за цим фільтром не знайдено.</p>
      </div>
    {:else if viewMode === 'grid'}
      <!-- Сітковий вигляд -->
      <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
        {#each filteredUserGames as item (item.gameId)}
          {@const g = getGameFor(item)}
          {#if g}
            <div
              role="button"
              tabindex="0"
              onclick={() => openGameDetail(item.gameId)}
              onkeydown={(e) => e.key === 'Enter' && openGameDetail(item.gameId)}
              class="group relative rounded-xl overflow-hidden cursor-pointer border border-slate-800 hover:border-cyan-500/50 transition-all hover:-translate-y-1 shadow-lg"
            >
              <img
                src={g.coverImageUrl || g.headerImageUrl}
                alt={g.title}
                class="w-full aspect-[3/4] object-cover"
              />
              <div class="absolute inset-0 bg-gradient-to-t from-black/90 via-black/10 to-transparent opacity-0 group-hover:opacity-100 transition-opacity"></div>
              <button
                type="button"
                onclick={(e) => { e.stopPropagation(); libraryStore.toggleFavorite(item.gameId); }}
                class="absolute top-2 right-2 p-1.5 rounded-lg bg-black/60 backdrop-blur-md text-slate-300 hover:text-amber-400 transition-colors cursor-pointer"
                title="Додати/прибрати з обраного"
              >
                <Star class="w-3.5 h-3.5 {item.isFavorite ? 'fill-amber-400 text-amber-400' : ''}" />
              </button>
              <div class="absolute bottom-0 left-0 right-0 p-2.5 opacity-0 group-hover:opacity-100 transition-opacity">
                <span class="block text-xs font-bold text-white line-clamp-2">{g.title}</span>
                <span class="block text-[10px] text-slate-300 mt-0.5">{formatBytes(g.sizeInBytes)}</span>
              </div>
            </div>
          {/if}
        {/each}
      </div>
    {:else}
      <!-- Списковий вигляд -->
      <div class="flex flex-col gap-2.5">
        {#each filteredUserGames as item (item.gameId)}
          {@const g = getGameFor(item)}
          {#if g}
            <div
              role="button"
              tabindex="0"
              onclick={() => openGameDetail(item.gameId)}
              onkeydown={(e) => e.key === 'Enter' && openGameDetail(item.gameId)}
              class="flex items-center gap-4 bg-[#0b0d14]/80 border border-slate-800 hover:border-cyan-500/40 rounded-xl px-4 py-3 cursor-pointer transition-colors group"
            >
              <img
                src={g.headerImageUrl || g.coverImageUrl}
                alt={g.title}
                class="w-28 h-14 rounded-lg object-cover ring-1 ring-slate-700 shrink-0"
              />
              <div class="flex-1 min-w-0">
                <h4 class="text-sm font-bold text-white truncate group-hover:text-cyan-300 transition-colors">{g.title}</h4>
                <span class="text-[11px] text-slate-500">{formatPlayTime(item.playTimeMinutes)} у грі</span>
              </div>

              <button
                type="button"
                onclick={(e) => { e.stopPropagation(); libraryStore.selectGame(item.gameId); launchGame(); }}
                class="hidden sm:flex items-center gap-1.5 px-4 py-2 rounded-full bg-cyan-500 hover:bg-cyan-400 text-black text-xs font-black transition-colors cursor-pointer shrink-0"
              >
                <Play class="w-3.5 h-3.5 fill-black" /> Грати
              </button>

              <div class="hidden md:flex flex-col items-end shrink-0 w-24">
                <span class="text-[10px] text-slate-500 uppercase font-semibold">Розмір</span>
                <span class="text-xs font-bold text-slate-300">{formatBytes(g.sizeInBytes)}</span>
              </div>

              <button
                type="button"
                onclick={(e) => { e.stopPropagation(); libraryStore.toggleFavorite(item.gameId); }}
                class="p-2 text-slate-500 hover:text-amber-400 transition-colors cursor-pointer shrink-0"
                title="Додати/прибрати з обраного"
              >
                <Star class="w-4 h-4 {item.isFavorite ? 'fill-amber-400 text-amber-400' : ''}" />
              </button>
            </div>
          {/if}
        {/each}
      </div>
    {/if}
      {/if}
      </div>
    </div>
  {/if}
</div>