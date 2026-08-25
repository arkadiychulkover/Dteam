<script lang="ts">
  import { libraryStore } from '../../stores/libraryStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { uiStore } from '../../stores/uiStore';
  import { mockNews, mockCommunityHighlights } from '../../stores/mockAppData';
  import {
    Search,
    Filter,
    LayoutGrid,
    List,
    Star,
    Play,
    Info,
    MoreHorizontal,
    Heart,
    MessageSquare,
    Share2,
    ArrowLeft,
    Plus,
    FileText,
    Sparkles,
    Users,
    Gamepad2,
    Tag
  } from 'lucide-svelte';

  // State & Filters
  let activeTab = $state<'all' | 'favorites' | 'collections'>('all');
  let viewMode = $state<'grid' | 'list'>('grid');
  let searchQuery = $state('');
  let activeFilterTag = $state('All');
  let isFilterMenuOpen = $state(false);

  // Active game details navigation inside library
  let activeGameId = $state<string | null>(null);
  let gameDetailsSubTab = $state<'store' | 'dlc' | 'developer' | 'community'>('store');

  const userGames = $derived($libraryStore.items);

  const allLibraryGames = $derived(() => {
    return $gamesStore.games.map(g => {
      const userItem = userGames.find(item => item.gameId === g.id);
      return {
        ...g,
        isOwned: !!userItem,
        playTimeMinutes: userItem?.playTimeMinutes ?? 0,
        lastPlayedAt: userItem?.lastPlayedAt ?? null,
        isFavorite: userItem?.isFavorite ?? false,
      };
    });
  });

  const filteredGames = $derived(() => {
    return allLibraryGames().filter(g => {
      if (activeTab === 'favorites' && !g.isFavorite) return false;
      if (searchQuery.trim() && !g.title.toLowerCase().includes(searchQuery.toLowerCase())) return false;
      if (activeFilterTag !== 'All' && (!g.tags || !g.tags.includes(activeFilterTag))) return false;
      return true;
    });
  });

  const selectedGame = $derived(() => {
    if (!activeGameId) return null;
    return allLibraryGames().find(g => g.id === activeGameId) || null;
  });

  function selectGame(id: string) {
    activeGameId = id;
  }

  function backToLibraryHome() {
    activeGameId = null;
  }

  let isLaunching = $state(false);
  function launchGame(title: string) {
    isLaunching = true;
    uiStore.addToast({
      title: 'Запуск гри...',
      message: `Запускається ${title}. Приємної гри!`,
      type: 'info',
    });
    setTimeout(() => {
      isLaunching = false;
    }, 2000);
  }
</script>

<!-- AppShell Layout: App Background #070C12, Fixed Sidebar + Main Scrollable Area -->
<div class="min-h-[90vh] bg-[#070C12] text-[#F1F5F9] font-sans flex overflow-hidden">
  
  <!-- Left Global Sidebar (240px Fixed) -->
  <aside class="w-60 bg-[#0A1118] border-r border-white/5 flex flex-col shrink-0 hidden md:flex">
    <!-- Sidebar Header -->
    <div class="p-3.5 border-b border-white/5 flex items-center justify-between">
      <h2 class="text-xs font-bold text-[#F1F5F9] uppercase tracking-wider">Усі ігри</h2>
      <button 
        onclick={() => isFilterMenuOpen = !isFilterMenuOpen}
        class="text-slate-500 hover:text-cyan-400 transition-colors cursor-pointer"
        title="Фільтр"
      >
        <Filter class="w-3.5 h-3.5" />
      </button>
    </div>

    <!-- Scrollable Game List -->
    <div class="flex-1 overflow-y-auto p-1.5 space-y-0.5 custom-scrollbar">
      {#each allLibraryGames() as game (game.id)}
        <button
          onclick={() => selectGame(game.id)}
          class="w-full h-9 px-2.5 rounded-lg flex items-center justify-between text-left transition-all cursor-pointer group border
            {activeGameId === game.id
              ? 'bg-[#1A2B3C] border-cyan-500/40 text-white shadow-sm'
              : 'hover:bg-[#162330] border-transparent text-[#94A3B8]'}"
        >
          <div class="flex items-center gap-2 min-w-0">
            <img
              src={game.coverImageUrl || game.headerImageUrl}
              alt={game.title}
              class="w-6 h-6 rounded-sm object-cover shrink-0 ring-1 ring-white/10"
            />
            <span class="text-xs font-medium truncate group-hover:text-[#F1F5F9] transition-colors">
              {game.title}
            </span>
          </div>
          {#if game.isFavorite}
            <Star class="w-3 h-3 fill-amber-400 text-amber-400 shrink-0" />
          {/if}
        </button>
      {/each}
    </div>
  </aside>

  <!-- Main Work & Content Area -->
  <div class="flex-1 flex flex-col min-w-0 overflow-hidden">
    
    <!-- Top Header Bar (Sticky) -->
    <header class="bg-[#0A1118] border-b border-white/5 px-6 py-3 flex flex-wrap items-center justify-between gap-4 sticky top-0 z-30 shrink-0">
      <!-- Search & Filters -->
      <div class="flex items-center gap-3 flex-1 max-w-lg">
        <!-- Search Input -->
        <div class="relative w-72">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-500">
            <Search class="w-4 h-4" />
          </div>
          <input
            type="text"
            bind:value={searchQuery}
            placeholder="Пошук у Крамниці..."
            class="w-full pl-9 pr-3 py-1.5 bg-[#0D151D] border border-white/10 rounded-lg text-xs text-[#F1F5F9] placeholder-slate-500 focus:outline-none focus:border-cyan-400 transition-all"
          />
        </div>

        <!-- Filter Button -->
        <div class="relative">
          <button
            onclick={() => isFilterMenuOpen = !isFilterMenuOpen}
            class="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-[#101922] hover:bg-[#162330] border border-white/[0.06] text-xs font-medium text-[#94A3B8] hover:text-[#F1F5F9] transition-all cursor-pointer"
          >
            <Filter class="w-3.5 h-3.5 text-cyan-400" />
            <span>Фільтри</span>
          </button>

          {#if isFilterMenuOpen}
            <div class="absolute left-0 mt-2 w-48 bg-[#101922] border border-white/10 rounded-xl shadow-2xl p-1.5 z-40">
              <div class="px-2.5 py-1 text-[10px] font-bold uppercase tracking-wider text-slate-500 border-b border-white/5">
                Жанри
              </div>
              {#each ['All', 'Action', 'RPG', 'Strategy', 'Cyberpunk', 'Sci-Fi', 'Indie'] as tag}
                <button
                  onclick={() => { activeFilterTag = tag; isFilterMenuOpen = false; }}
                  class="w-full text-left px-2.5 py-1.5 text-xs rounded-lg flex items-center justify-between hover:bg-[#162330] transition-colors cursor-pointer {activeFilterTag === tag ? 'text-cyan-400 font-bold bg-cyan-950/40' : 'text-slate-300'}"
                >
                  <span>{tag === 'All' ? 'Усі жанри' : tag}</span>
                  {#if activeFilterTag === tag}
                    <span class="w-1.5 h-1.5 rounded-full bg-cyan-400"></span>
                  {/if}
                </button>
              {/each}
            </div>
          {/if}
        </div>
      </div>

      <!-- View Switcher -->
      <div class="flex items-center bg-[#0D151D] p-1 rounded-lg border border-white/10">
        <button
          onclick={() => viewMode = 'grid'}
          class="p-1.5 rounded-md transition-colors cursor-pointer {viewMode === 'grid' ? 'bg-cyan-500/20 text-cyan-400' : 'text-slate-500 hover:text-white'}"
          title="Сітка"
        >
          <LayoutGrid class="w-4 h-4" />
        </button>
        <button
          onclick={() => viewMode = 'list'}
          class="p-1.5 rounded-md transition-colors cursor-pointer {viewMode === 'list' ? 'bg-cyan-500/20 text-cyan-400' : 'text-slate-500 hover:text-white'}"
          title="Список"
        >
          <List class="w-4 h-4" />
        </button>
      </div>
    </header>

    <!-- Scrollable Workspace Body -->
    <main class="flex-1 overflow-y-auto p-6 space-y-8 custom-scrollbar">
      {#if selectedGame()}
        <!-- ROUTE 2: Game Details View -->
        {@const game = selectedGame()!}
        <div class="space-y-6">
          <!-- Hero Header Banner -->
          <div class="relative rounded-xl overflow-hidden border border-white/[0.06] min-h-[320px] flex flex-col justify-end p-8 bg-[#0D151D] shadow-2xl">
            <!-- Panoramic Wallpaper Background -->
            <div class="absolute inset-0 z-0">
              <img
                src={game.headerImageUrl || game.coverImageUrl}
                alt={game.title}
                class="w-full h-full object-cover filter brightness-75 scale-105"
              />
              <div class="absolute inset-0 bg-gradient-to-t from-[#070C12] via-[#070C12]/70 to-transparent"></div>
              <div class="absolute inset-0 bg-gradient-to-r from-[#070C12]/90 via-transparent to-transparent"></div>
            </div>

            <!-- Content Container -->
            <div class="relative z-10 space-y-5">
              <!-- Back button -->
              <button
                onclick={backToLibraryHome}
                class="inline-flex items-center gap-2 text-xs font-bold text-slate-300 hover:text-cyan-400 transition-colors cursor-pointer bg-[#070C12]/80 backdrop-blur-md px-3 py-1.5 rounded-lg border border-white/10"
              >
                <ArrowLeft class="w-4 h-4" /> ← Назад до Бібліотеки
              </button>

              <div class="flex flex-wrap items-end justify-between gap-6">
                <div>
                  <h1 class="text-4xl sm:text-5xl font-black text-[#F1F5F9] tracking-tight">
                    {game.title}
                  </h1>
                </div>

                <!-- Primary Action + Disk Size + Action Icons -->
                <div class="flex items-center gap-4 flex-wrap">
                  <div class="text-right">
                    <span class="block text-xs font-medium text-[#94A3B8]">Розмір на диску: 10 ГБ</span>
                  </div>

                  <button
                    onclick={() => launchGame(game.title)}
                    disabled={isLaunching}
                    class="px-8 py-3.5 rounded-lg bg-gradient-to-r from-[#00F2FE] to-[#4FACFE] hover:from-cyan-300 hover:to-cyan-400 text-black font-extrabold text-sm tracking-wider shadow-lg shadow-cyan-400/20 hover:scale-105 active:scale-95 transition-all cursor-pointer flex items-center gap-2.5 disabled:opacity-50"
                  >
                    <Play class="w-4 h-4 fill-black" />
                    <span>{isLaunching ? 'ЗАПУСК...' : 'Скачати'}</span>
                  </button>

                  <div class="flex items-center gap-1.5">
                    <button
                      onclick={() => libraryStore.toggleFavorite(game.id)}
                      class="p-3 rounded-lg bg-[#101922] hover:bg-[#162330] text-[#94A3B8] hover:text-white border border-white/[0.06] transition-colors cursor-pointer"
                      title="Favorite"
                    >
                      <Star class="w-4 h-4 {game.isFavorite ? 'fill-amber-400 text-amber-400' : ''}" />
                    </button>
                    <button
                      class="p-3 rounded-lg bg-[#101922] hover:bg-[#162330] text-[#94A3B8] hover:text-white border border-white/[0.06] transition-colors cursor-pointer"
                      title="Info"
                    >
                      <Info class="w-4 h-4" />
                    </button>
                    <button
                      class="p-3 rounded-lg bg-[#101922] hover:bg-[#162330] text-[#94A3B8] hover:text-white border border-white/[0.06] transition-colors cursor-pointer"
                      title="Options"
                    >
                      <MoreHorizontal class="w-4 h-4" />
                    </button>
                  </div>
                </div>
              </div>

              <!-- Sub Navigation Links Bar -->
              <div class="pt-4 border-t border-white/10 flex flex-wrap items-center gap-6 text-xs font-semibold text-[#94A3B8]">
                <button
                  onclick={() => gameDetailsSubTab = 'store'}
                  class="transition-colors cursor-pointer {gameDetailsSubTab === 'store' ? 'text-cyan-400 font-bold border-b-2 border-cyan-400 pb-1' : 'hover:text-[#F1F5F9]'}"
                >
                  Сторінка крамниці
                </button>
                <button
                  onclick={() => gameDetailsSubTab = 'dlc'}
                  class="transition-colors cursor-pointer {gameDetailsSubTab === 'dlc' ? 'text-cyan-400 font-bold border-b-2 border-cyan-400 pb-1' : 'hover:text-[#F1F5F9]'}"
                >
                  DLC
                </button>
                <button
                  onclick={() => gameDetailsSubTab = 'developer'}
                  class="transition-colors cursor-pointer {gameDetailsSubTab === 'developer' ? 'text-cyan-400 font-bold border-b-2 border-cyan-400 pb-1' : 'hover:text-[#F1F5F9]'}"
                >
                  Сторінка розробника
                </button>
                <button
                  onclick={() => gameDetailsSubTab = 'community'}
                  class="transition-colors cursor-pointer {gameDetailsSubTab === 'community' ? 'text-cyan-400 font-bold border-b-2 border-cyan-400 pb-1' : 'hover:text-[#F1F5F9]'}"
                >
                  Спільнота
                </button>
              </div>
            </div>
          </div>

          <!-- Two-Column Layout (70% Left / 30% Right Split) -->
          <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
            <!-- Left 70% Feed -->
            <div class="lg:col-span-8 space-y-6">
              
              <!-- Моя рецензія Section -->
              <div class="bg-[#101922] border border-white/[0.06] rounded-xl p-5 flex flex-wrap items-center justify-between gap-4">
                <div>
                  <h3 class="text-sm font-bold text-[#F1F5F9]">Моя рецензія</h3>
                  <p class="text-xs text-[#94A3B8] mt-1">Поділіться своїми враженнями від гри з іншими гравцями</p>
                </div>
                <button class="px-4 py-2 rounded-lg bg-transparent hover:bg-cyan-500/10 text-cyan-400 border border-cyan-400 text-xs font-bold transition-all cursor-pointer">
                  Написати рецензію
                </button>
              </div>

              <!-- Що нового Section -->
              <div class="bg-[#101922] border border-white/[0.06] rounded-xl p-6 space-y-4">
                <div class="flex items-center justify-between">
                  <h3 class="text-xs font-bold text-[#F1F5F9] uppercase tracking-wider">Що нового</h3>
                  <span class="text-xs text-slate-500">25.02.2024</span>
                </div>

                <div class="rounded-xl overflow-hidden border border-white/5 bg-[#162330]">
                  <img
                    src={game.headerImageUrl}
                    alt="Update Banner"
                    class="w-full h-48 object-cover"
                  />
                  <div class="p-4 space-y-2">
                    <h4 class="text-base font-bold text-[#F1F5F9]">Патч 1.5: Оптимізація та нові випробування</h4>
                    <p class="text-xs text-[#94A3B8] leading-relaxed">
                      У цьому оновленні додано підтримку покращеного трасування променів, виправлено баланс зброї та додано місії у кооперативі.
                    </p>

                    <div class="pt-3 border-t border-white/5 flex items-center justify-between text-xs text-[#94A3B8]">
                      <div class="flex items-center gap-4">
                        <span class="flex items-center gap-1.5 text-rose-500 font-semibold">
                          <Heart class="w-4 h-4 fill-rose-500 text-rose-500" /> 2.5k
                        </span>
                        <span class="flex items-center gap-1.5">
                          <MessageSquare class="w-4 h-4" /> 2.5k
                        </span>
                      </div>
                      <button class="flex items-center gap-1.5 hover:text-white transition-colors cursor-pointer font-medium">
                        <Share2 class="w-4 h-4" /> Поділитись
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Цікаве від Спільноти Section -->
              <div class="bg-[#101922] border border-white/[0.06] rounded-xl p-6 space-y-4">
                <h3 class="text-xs font-bold text-[#F1F5F9] uppercase tracking-wider">Цікаве від Спільноти</h3>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {#each mockCommunityHighlights.slice(0, 2) as post}
                    <div class="bg-[#162330] border border-white/5 rounded-xl p-3 space-y-2.5">
                      <div class="flex items-center gap-2">
                        <img src={post.authorAvatar} alt="" class="w-6 h-6 rounded-full object-cover" />
                        <span class="text-xs font-bold text-[#F1F5F9]">{post.authorName}</span>
                      </div>
                      <img src={post.mediaUrl} alt="" class="w-full h-32 object-cover rounded-lg" />
                      <p class="text-xs text-[#94A3B8] line-clamp-2">{post.caption}</p>
                    </div>
                  {/each}
                </div>
              </div>

            </div>

            <!-- Right 30% Sidebar -->
            <div class="lg:col-span-4 space-y-6">
              
              <!-- Друзі бажають цю гру: 2 -->
              <div class="bg-[#101922] border border-white/[0.06] rounded-xl p-5 space-y-3">
                <h4 class="text-xs font-bold uppercase tracking-wider text-[#F1F5F9]">Друзі бажають цю гру: 2</h4>
                <div class="flex items-center gap-2">
                  <img src="https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150" alt="" class="w-8 h-8 rounded-full border border-cyan-500/30" />
                  <img src="https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=150" alt="" class="w-8 h-8 rounded-full border border-cyan-500/30" />
                  <span class="text-xs text-[#94A3B8]">CyberViper, NikaNii</span>
                </div>
              </div>

              <!-- Друзі мають цю гру: 11 -->
              <div class="bg-[#101922] border border-white/[0.06] rounded-xl p-5 space-y-3">
                <div class="flex items-center justify-between">
                  <h4 class="text-xs font-bold uppercase tracking-wider text-[#F1F5F9]">Друзі мають эту гру: 11</h4>
                </div>
                <div class="flex flex-wrap items-center gap-2">
                  {#each Array(6) as _, i}
                    <img
                      src={`https://images.unsplash.com/photo-${1535713875002 + i}?w=100`}
                      alt="Friend Avatar"
                      class="w-8 h-8 rounded-full object-cover border border-white/10"
                    />
                  {/each}
                  <div class="w-8 h-8 rounded-full bg-[#162330] border border-white/10 flex items-center justify-center text-xs font-bold text-cyan-400">
                    +5
                  </div>
                </div>
              </div>

            </div>
          </div>
        </div>

      {:else}
        <!-- ROUTE 1: Library Home -->

        <!-- Top Section: Новини -->
        <section class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-base font-bold text-[#F1F5F9] uppercase tracking-wider">Новини</h2>
            <button class="text-xs font-bold text-cyan-400 hover:underline transition-all cursor-pointer">
              Усі новини &gt;
            </button>
          </div>

          <!-- 3-Column Grid -->
          <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
            {#each mockNews as news}
              <div class="bg-[#101922] border border-white/[0.06] hover:border-cyan-400/40 rounded-xl overflow-hidden transition-all duration-200 hover:bg-[#162330] group flex flex-col">
                <div class="relative aspect-video w-full overflow-hidden bg-slate-900">
                  <img
                    src={news.bannerUrl}
                    alt={news.headline}
                    class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                  />
                  <div class="absolute top-2.5 left-2.5 flex items-center gap-1 px-2 py-0.5 rounded bg-[#070C12]/80 backdrop-blur-md text-[10px] font-bold text-cyan-400 border border-white/10">
                    <Tag class="w-3 h-3 text-cyan-400" />
                    <span>{news.gameTag}</span>
                  </div>
                </div>

                <div class="p-4 flex-1 flex flex-col justify-between space-y-3">
                  <div>
                    <h3 class="text-sm font-bold text-[#F1F5F9] group-hover:text-cyan-300 transition-colors line-clamp-1">
                      {news.headline}
                    </h3>
                    <p class="text-xs text-[#94A3B8] mt-1.5 line-clamp-2 leading-relaxed">
                      {news.summary}
                    </p>
                  </div>

                  <div class="pt-3 border-t border-white/5 flex items-center justify-between text-[11px] text-[#64748B]">
                    <div class="flex items-center gap-3">
                      <span class="flex items-center gap-1 text-rose-500 font-medium">
                        <Heart class="w-3.5 h-3.5 fill-rose-500 text-rose-500" /> 2.5k
                      </span>
                      <span class="flex items-center gap-1 text-[#94A3B8]">
                        <MessageSquare class="w-3.5 h-3.5" /> 2.5k
                      </span>
                    </div>
                    <span>{news.date}</span>
                  </div>
                </div>
              </div>
            {/each}
          </div>
        </section>

        <!-- Middle Section: Цікаве від Спільноти -->
        <section class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-base font-bold text-[#F1F5F9] uppercase tracking-wider">Цікаве від Спільноти</h2>
            <button class="text-xs font-bold text-cyan-400 hover:underline transition-all cursor-pointer">
              Моя стрічка &gt;
            </button>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
            {#each mockCommunityHighlights as post}
              <div class="bg-[#101922] border border-white/[0.06] hover:border-cyan-400/40 rounded-xl p-4 space-y-3 transition-all hover:bg-[#162330]">
                <!-- User Header -->
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-2.5">
                    <img src={post.authorAvatar} alt={post.authorName} class="w-8 h-8 rounded-full object-cover ring-1 ring-white/10" />
                    <span class="text-xs font-bold text-[#F1F5F9]">{post.authorName}</span>
                  </div>
                  <button class="text-slate-500 hover:text-slate-300 transition-colors cursor-pointer">
                    <MoreHorizontal class="w-4 h-4" />
                  </button>
                </div>

                <!-- Media Content -->
                <div class="relative rounded-lg overflow-hidden border border-white/5 h-44 bg-slate-900 group">
                  <img src={post.mediaUrl} alt="" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />
                  {#if post.mediaType === 'video'}
                    <div class="absolute inset-0 flex items-center justify-center bg-black/40 backdrop-blur-[1px]">
                      <div class="w-10 h-10 rounded-full bg-white text-black flex items-center justify-center shadow-lg group-hover:scale-110 transition-transform">
                        <Play class="w-4 h-4 fill-black ml-0.5" />
                      </div>
                    </div>
                  {/if}
                </div>

                <!-- Caption -->
                <p class="text-xs text-[#94A3B8] line-clamp-2 leading-relaxed">
                  {post.caption}
                </p>

                <!-- Stats Footer -->
                <div class="pt-2 border-t border-white/5 flex items-center justify-between text-[11px] text-[#64748B]">
                  <div class="flex items-center gap-3">
                    <span class="flex items-center gap-1 text-rose-500 font-medium">
                      <Heart class="w-3.5 h-3.5 fill-rose-500 text-rose-500" /> 2.5k
                    </span>
                    <span class="flex items-center gap-1 text-[#94A3B8]">
                      <MessageSquare class="w-3.5 h-3.5" /> 2.5k
                    </span>
                  </div>
                  <span>{post.date}</span>
                </div>
              </div>
            {/each}
          </div>
        </section>

        <!-- Bottom Section: Game Collection View -->
        <section class="space-y-4">
          <!-- Tab Bar -->
          <div class="flex flex-wrap items-center justify-between gap-4 border-b border-white/10 pb-3">
            <div class="flex items-center gap-6 text-xs font-bold">
              <button
                onclick={() => activeTab = 'all'}
                class="transition-colors cursor-pointer pb-3 relative {activeTab === 'all' ? 'text-cyan-400 font-bold' : 'text-[#64748B] hover:text-[#F1F5F9]'}"
              >
                Усі ігри
                {#if activeTab === 'all'}
                  <span class="absolute bottom-0 left-0 right-0 h-0.5 bg-cyan-400 rounded-full shadow-[0_0_8px_#22d3ee]"></span>
                {/if}
              </button>

              <button
                onclick={() => activeTab = 'favorites'}
                class="transition-colors cursor-pointer pb-3 relative {activeTab === 'favorites' ? 'text-cyan-400 font-bold' : 'text-[#64748B] hover:text-[#F1F5F9]'}"
              >
                Обране
                {#if activeTab === 'favorites'}
                  <span class="absolute bottom-0 left-0 right-0 h-0.5 bg-cyan-400 rounded-full shadow-[0_0_8px_#22d3ee]"></span>
                {/if}
              </button>

              <button
                onclick={() => activeTab = 'collections'}
                class="transition-colors cursor-pointer pb-3 relative {activeTab === 'collections' ? 'text-cyan-400 font-bold' : 'text-[#64748B] hover:text-[#F1F5F9]'}"
              >
                Моя колекція
                {#if activeTab === 'collections'}
                  <span class="absolute bottom-0 left-0 right-0 h-0.5 bg-cyan-400 rounded-full shadow-[0_0_8px_#22d3ee]"></span>
                {/if}
              </button>
            </div>

            <!-- Cyan Plus Icon Button -->
            <button
              onclick={() => uiStore.addToast({ title: 'Колекція', message: 'Створення нової колекції доступно!', type: 'info' })}
              class="p-1.5 rounded-lg bg-cyan-400/10 hover:bg-cyan-400/20 text-cyan-400 border border-cyan-400/30 transition-all cursor-pointer"
              title="Створити колекцію"
            >
              <Plus class="w-4 h-4" />
            </button>
          </div>

          <!-- MODE 1: GRID VIEW (Default 6-column 3:4 Aspect Ratio) -->
          {#if viewMode === 'grid'}
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
              {#each filteredGames() as game (game.id)}
                <div
                  role="button"
                  tabindex="0"
                  onclick={() => selectGame(game.id)}
                  onkeydown={(e) => e.key === 'Enter' && selectGame(game.id)}
                  class="group relative bg-[#101922] border border-white/[0.06] hover:border-cyan-400/60 rounded-xl overflow-hidden transition-transform duration-200 hover:scale-105 shadow-md cursor-pointer flex flex-col"
                >
                  <!-- 3:4 Aspect Ratio Poster -->
                  <div class="aspect-[3/4] w-full overflow-hidden relative bg-slate-900">
                    <img
                      src={game.coverImageUrl || game.headerImageUrl}
                      alt={game.title}
                      class="w-full h-full object-cover"
                    />
                    <div class="absolute inset-0 bg-gradient-to-t from-[#070C12] via-transparent to-transparent opacity-80 group-hover:opacity-40 transition-opacity"></div>

                    {#if game.isFavorite}
                      <div class="absolute top-2 right-2 p-1 rounded-full bg-black/60 backdrop-blur-md">
                        <Star class="w-3 h-3 fill-amber-400 text-amber-400" />
                      </div>
                    {/if}
                  </div>

                  <div class="p-2.5 bg-[#101922]">
                    <h3 class="text-xs font-semibold text-[#F1F5F9] truncate group-hover:text-cyan-300 transition-colors">
                      {game.title}
                    </h3>
                  </div>
                </div>
              {/each}
            </div>
          {:else}
            <!-- MODE 2: LIST VIEW -->
            <div class="space-y-2">
              {#each filteredGames() as game (game.id)}
                <div
                  class="bg-[#0D151D] hover:bg-[#121C26] rounded-xl p-2 mb-2 flex items-center justify-between gap-4 border border-white/[0.06] transition-colors"
                >
                  <!-- Left: Cropped wide banner artwork (16:9) + Center: Title + Primary Action + Disk size -->
                  <div class="flex items-center gap-4 min-w-0 flex-1">
                    <img
                      src={game.headerImageUrl || game.coverImageUrl}
                      alt={game.title}
                      class="w-28 aspect-video rounded-lg object-cover ring-1 ring-white/10 shrink-0"
                    />
                    <div class="flex flex-col sm:flex-row sm:items-center gap-3 flex-1 min-w-0">
                      <button 
                        type="button" 
                        class="text-xs font-bold text-[#F1F5F9] hover:text-cyan-300 transition-colors cursor-pointer text-left truncate" 
                        onclick={() => selectGame(game.id)}
                      >
                        {game.title}
                      </button>

                      <button
                        onclick={() => launchGame(game.title)}
                        class="px-4 py-1.5 rounded-lg bg-cyan-400 hover:bg-cyan-300 text-black font-extrabold text-xs shadow-md shadow-cyan-400/20 transition-all cursor-pointer flex items-center gap-1.5 shrink-0"
                      >
                        <Play class="w-3.5 h-3.5 fill-black" />
                        <span>Скачати</span>
                      </button>

                      <span class="text-xs text-[#94A3B8] shrink-0">Розмір на диску: 10 ГБ</span>
                    </div>
                  </div>

                  <!-- Right side: Star icon, Info icon, Options icon -->
                  <div class="flex items-center gap-2 shrink-0">
                    <button
                      onclick={() => libraryStore.toggleFavorite(game.id)}
                      class="p-2 rounded-lg bg-[#101922] hover:bg-[#162330] text-[#94A3B8] hover:text-white transition-colors cursor-pointer border border-white/[0.06]"
                      title="Favorite"
                    >
                      <Star class="w-3.5 h-3.5 {game.isFavorite ? 'fill-amber-400 text-amber-400' : ''}" />
                    </button>

                    <button
                      class="p-2 rounded-lg bg-[#101922] hover:bg-[#162330] text-[#94A3B8] hover:text-white transition-colors cursor-pointer border border-white/[0.06]"
                      title="Info"
                    >
                      <Info class="w-3.5 h-3.5" />
                    </button>

                    <button
                      class="p-2 rounded-lg bg-[#101922] hover:bg-[#162330] text-[#94A3B8] hover:text-white transition-colors cursor-pointer border border-white/[0.06]"
                      title="Options"
                    >
                      <MoreHorizontal class="w-3.5 h-3.5" />
                    </button>
                  </div>
                </div>
              {/each}
            </div>
          {/if}
        </section>
      {/if}
    </main>
  </div>
</div>

<style>
  .custom-scrollbar::-webkit-scrollbar {
    width: 5px;
  }
  .custom-scrollbar::-webkit-scrollbar-track {
    background: transparent;
  }
  .custom-scrollbar::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.08);
    border-radius: 9999px;
  }
  .custom-scrollbar::-webkit-scrollbar-thumb:hover {
    background: rgba(34, 211, 238, 0.3);
  }
</style>
