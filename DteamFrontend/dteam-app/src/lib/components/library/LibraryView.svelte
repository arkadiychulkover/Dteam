<script lang="ts">
  import { libraryStore } from '../../stores/libraryStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { uiStore } from '../../stores/uiStore';
  import { mockNews, mockCommunityHighlights } from '../../stores/mockAppData';
  import { formatPlayTime } from '../../utils/formatters';
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
    Download,
    CheckCircle2,
    Users,
    Gamepad2,
    MessageCircle,
    Plus,
    FileText,
    ExternalLink,
    Sparkles,
    Image,
    Video
  } from 'lucide-svelte';

  // State
  let activeTab = $state<'all' | 'favorites' | 'collections'>('all');
  let viewMode = $state<'grid' | 'list'>('grid');
  let searchQuery = $state('');
  let activeFilterTag = $state('All');
  let isFilterMenuOpen = $state(false);

  // Router for game details inside library
  let activeGameId = $state<string | null>(null);
  let gameDetailsSubTab = $state<'store' | 'dlc' | 'developer' | 'community'>('store');

  const userGames = $derived($libraryStore.items);
  
  // Rich Games Library with additional games for demo
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

<div class="min-h-[90vh] bg-[#0A0D14] text-slate-200 font-sans flex flex-col">
  <!-- Top Navigation Header -->
  <div class="bg-[#111C24]/90 backdrop-blur-md border-b border-white/10 px-4 sm:px-6 lg:px-8 py-3.5 flex flex-wrap items-center justify-between gap-4 sticky top-16 z-30">
    <!-- Search Bar -->
    <div class="relative flex-1 max-w-md min-w-[240px]">
      <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-cyan-400/70">
        <Search class="w-4 h-4" />
      </div>
      <input
        type="text"
        bind:value={searchQuery}
        placeholder="Пошук у Крамниці та Бібліотеці..."
        class="w-full pl-10 pr-4 py-2 bg-[#0a0d14]/80 border border-white/10 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
      />
    </div>

    <!-- Filters & View Switcher -->
    <div class="flex items-center gap-3">
      <!-- Filter Button -->
      <div class="relative">
        <button
          onclick={() => isFilterMenuOpen = !isFilterMenuOpen}
          class="flex items-center gap-2 px-3.5 py-2 rounded-xl bg-[#16222F] hover:bg-slate-800 border border-white/10 text-xs font-semibold text-slate-200 hover:text-white transition-all cursor-pointer"
        >
          <Filter class="w-4 h-4 text-cyan-400" />
          <span>Фільтри ({activeFilterTag})</span>
        </button>

        {#if isFilterMenuOpen}
          <div class="absolute right-0 mt-2 w-48 bg-[#16222F] border border-white/10 rounded-xl shadow-2xl p-1.5 z-40">
            <div class="px-2.5 py-1.5 text-[11px] font-bold uppercase tracking-wider text-slate-400 border-b border-white/5">
              Жанр
            </div>
            {#each ['All', 'Action', 'RPG', 'Strategy', 'Cyberpunk', 'Sci-Fi'] as tag}
              <button
                onclick={() => { activeFilterTag = tag; isFilterMenuOpen = false; }}
                class="w-full text-left px-2.5 py-1.5 text-xs rounded-lg flex items-center justify-between hover:bg-slate-800 transition-colors cursor-pointer {activeFilterTag === tag ? 'text-cyan-400 font-bold bg-cyan-950/40' : 'text-slate-300'}"
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

      <!-- View Switcher -->
      <div class="flex items-center bg-[#0a0d14] p-1 rounded-xl border border-white/10">
        <button
          onclick={() => viewMode = 'grid'}
          class="p-1.5 rounded-lg transition-colors cursor-pointer {viewMode === 'grid' ? 'bg-cyan-500/20 text-cyan-400' : 'text-slate-400 hover:text-white'}"
          title="Сітка"
        >
          <LayoutGrid class="w-4 h-4" />
        </button>
        <button
          onclick={() => viewMode = 'list'}
          class="p-1.5 rounded-lg transition-colors cursor-pointer {viewMode === 'list' ? 'bg-cyan-500/20 text-cyan-400' : 'text-slate-400 hover:text-white'}"
          title="Список"
        >
          <List class="w-4 h-4" />
        </button>
      </div>
    </div>
  </div>

  <!-- Main Grid Layout: Sidebar + Main Workspace -->
  <div class="flex-1 flex overflow-hidden">
    <!-- Left Sidebar: Games List -->
    <aside class="w-64 sm:w-72 bg-[#111C24]/80 border-r border-white/10 flex flex-col shrink-0 hidden md:flex">
      <!-- Header -->
      <div class="p-4 border-b border-white/10 flex items-center justify-between">
        <div class="flex items-center gap-2">
          <Gamepad2 class="w-4 h-4 text-cyan-400" />
          <h2 class="text-xs font-extrabold uppercase tracking-wider text-slate-200">Усі ігри</h2>
        </div>
        <span class="text-[11px] font-mono font-bold px-2 py-0.5 rounded-md bg-cyan-950/60 text-cyan-400 border border-cyan-500/30">
          {allLibraryGames().length}
        </span>
      </div>

      <!-- Scrollable List of Owned Games -->
      <div class="flex-1 overflow-y-auto p-2 space-y-1 custom-scrollbar">
        {#each allLibraryGames() as game (game.id)}
          <button
            onclick={() => selectGame(game.id)}
            class="w-full flex items-center justify-between p-2 rounded-xl text-left transition-all cursor-pointer group border
              {activeGameId === game.id
                ? 'bg-gradient-to-r from-cyan-950/80 to-slate-900 border-cyan-500/50 text-white shadow-lg'
                : 'hover:bg-[#16222F] border-transparent text-slate-300'}"
          >
            <div class="flex items-center gap-2.5 min-w-0">
              <img
                src={game.coverImageUrl || game.headerImageUrl}
                alt={game.title}
                class="w-7 h-7 rounded-lg object-cover ring-1 ring-white/10 shrink-0"
              />
              <span class="text-xs font-semibold truncate group-hover:text-cyan-300 transition-colors">
                {game.title}
              </span>
            </div>
            {#if game.isFavorite}
              <Star class="w-3.5 h-3.5 fill-amber-400 text-amber-400 shrink-0" />
            {/if}
          </button>
        {/each}
      </div>
    </aside>

    <!-- Content Work Area -->
    <main class="flex-1 overflow-y-auto p-4 sm:p-6 lg:p-8 space-y-8 custom-scrollbar">
      {#if selectedGame()}
        <!-- ROUTE 2: Game Details View -->
        {@const game = selectedGame()!}
        <div class="space-y-6">
          <!-- Hero Header Banner -->
          <div class="relative rounded-3xl overflow-hidden border border-white/10 min-h-[280px] sm:min-h-[340px] flex flex-col justify-end p-6 sm:p-8 bg-slate-900 shadow-2xl">
            <!-- Background Image with Overlay Gradient -->
            <div class="absolute inset-0 z-0">
              <img
                src={game.headerImageUrl || game.coverImageUrl}
                alt={game.title}
                class="w-full h-full object-cover filter brightness-75 scale-105"
              />
              <div class="absolute inset-0 bg-gradient-to-t from-[#0A0D14] via-[#0A0D14]/70 to-transparent"></div>
              <div class="absolute inset-0 bg-gradient-to-r from-[#0A0D14]/90 via-transparent to-transparent"></div>
            </div>

            <!-- Header Content -->
            <div class="relative z-10 space-y-4">
              <!-- Back button -->
              <button
                onclick={backToLibraryHome}
                class="inline-flex items-center gap-2 text-xs font-bold text-slate-300 hover:text-cyan-400 transition-colors cursor-pointer bg-[#0A0D14]/80 backdrop-blur-md px-3 py-1.5 rounded-xl border border-white/10"
              >
                <ArrowLeft class="w-4 h-4" /> Назад до Бібліотеки
              </button>

              <div class="flex flex-wrap items-end justify-between gap-6">
                <div>
                  <h1 class="text-3xl sm:text-5xl font-extrabold text-white tracking-wide font-['Outfit']">
                    {game.title}
                  </h1>
                  <p class="text-xs sm:text-sm text-slate-300 mt-2 max-w-xl line-clamp-2">
                    {game.description}
                  </p>
                </div>

                <!-- Action Button & Metadata -->
                <div class="flex items-center gap-4 flex-wrap">
                  <div class="text-right hidden sm:block">
                    <span class="block text-[11px] uppercase font-bold text-slate-400">Розмір на диску</span>
                    <span class="text-sm font-bold text-white font-mono">10 ГБ</span>
                  </div>

                  <button
                    onclick={() => launchGame(game.title)}
                    disabled={isLaunching}
                    class="px-8 py-4 rounded-2xl bg-gradient-to-r from-cyan-400 to-cyan-500 hover:from-cyan-300 hover:to-cyan-400 text-black font-extrabold text-base tracking-wider shadow-lg shadow-cyan-400/30 hover:scale-105 active:scale-95 transition-all cursor-pointer flex items-center gap-3 disabled:opacity-50"
                  >
                    <Play class="w-5 h-5 fill-black" />
                    <span>{isLaunching ? 'ЗАПУСК...' : 'Скачати / Грати'}</span>
                  </button>

                  <div class="flex items-center gap-2">
                    <button
                      onclick={() => libraryStore.toggleFavorite(game.id)}
                      class="p-3 rounded-xl bg-[#16222F]/90 hover:bg-slate-800 text-slate-300 border border-white/10 transition-colors cursor-pointer"
                      title="Додати в обране"
                    >
                      <Star class="w-4 h-4 {game.isFavorite ? 'fill-amber-400 text-amber-400' : ''}" />
                    </button>
                    <button
                      class="p-3 rounded-xl bg-[#16222F]/90 hover:bg-slate-800 text-slate-300 border border-white/10 transition-colors cursor-pointer"
                      title="Інформація"
                    >
                      <Info class="w-4 h-4" />
                    </button>
                    <button
                      class="p-3 rounded-xl bg-[#16222F]/90 hover:bg-slate-800 text-slate-300 border border-white/10 transition-colors cursor-pointer"
                      title="Опції"
                    >
                      <MoreHorizontal class="w-4 h-4" />
                    </button>
                  </div>
                </div>
              </div>

              <!-- Sub Navigation Links Bar -->
              <div class="pt-4 border-t border-white/10 flex flex-wrap items-center gap-6 text-xs font-bold">
                <button
                  onclick={() => gameDetailsSubTab = 'store'}
                  class="transition-colors cursor-pointer {gameDetailsSubTab === 'store' ? 'text-cyan-400 underline underline-offset-8 decoration-2' : 'text-slate-400 hover:text-white'}"
                >
                  Сторінка крамниці
                </button>
                <button
                  onclick={() => gameDetailsSubTab = 'dlc'}
                  class="transition-colors cursor-pointer {gameDetailsSubTab === 'dlc' ? 'text-cyan-400 underline underline-offset-8 decoration-2' : 'text-slate-400 hover:text-white'}"
                >
                  DLC
                </button>
                <button
                  onclick={() => gameDetailsSubTab = 'developer'}
                  class="transition-colors cursor-pointer {gameDetailsSubTab === 'developer' ? 'text-cyan-400 underline underline-offset-8 decoration-2' : 'text-slate-400 hover:text-white'}"
                >
                  Сторінка розробника
                </button>
                <button
                  onclick={() => gameDetailsSubTab = 'community'}
                  class="transition-colors cursor-pointer {gameDetailsSubTab === 'community' ? 'text-cyan-400 underline underline-offset-8 decoration-2' : 'text-slate-400 hover:text-white'}"
                >
                  Спільнота
                </button>
              </div>
            </div>
          </div>

          <!-- Split Details Layout: 70% Left Main / 30% Right Sidebar -->
          <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
            <!-- Left 70% Content Feed -->
            <div class="lg:col-span-8 space-y-6">
              <!-- Моя рецензія Section -->
              <div class="bg-[#111C24] border border-white/10 rounded-2xl p-5 flex flex-wrap items-center justify-between gap-4">
                <div>
                  <h3 class="text-sm font-bold text-white flex items-center gap-2">
                    <FileText class="w-4 h-4 text-cyan-400" /> Моя рецензія
                  </h3>
                  <p class="text-xs text-slate-400 mt-1">Поділіться своими враженнями від гри з іншими гравцями</p>
                </div>
                <button class="px-4 py-2 rounded-xl bg-cyan-500/20 hover:bg-cyan-500/30 text-cyan-400 border border-cyan-500/40 text-xs font-bold transition-all cursor-pointer">
                  Написати рецензію
                </button>
              </div>

              <!-- Що нового Section -->
              <div class="bg-[#111C24] border border-white/10 rounded-2xl p-6 space-y-4">
                <div class="flex items-center justify-between">
                  <h3 class="text-sm font-bold text-white uppercase tracking-wider flex items-center gap-2">
                    <Sparkles class="w-4 h-4 text-cyan-400" /> Що нового
                  </h3>
                  <span class="text-[11px] text-slate-500">Оновлення 1.5</span>
                </div>

                <div class="rounded-xl overflow-hidden border border-white/5">
                  <img
                    src={game.headerImageUrl}
                    alt="Update Banner"
                    class="w-full h-44 object-cover"
                  />
                  <div class="p-4 bg-[#16222F]/60 space-y-2">
                    <h4 class="text-base font-bold text-white">Патч 1.5: Оптимізація графіки та нові випробування</h4>
                    <p class="text-xs text-slate-300 leading-relaxed">
                      У цьому оновленні додано підтримку покращеного трасування променів, виправлено баланс зброї та додано 5 нових місій у режимі кооперативу.
                    </p>

                    <div class="pt-3 border-t border-white/5 flex items-center justify-between text-xs text-slate-400">
                      <div class="flex items-center gap-4">
                        <button class="flex items-center gap-1.5 hover:text-cyan-400 transition-colors cursor-pointer">
                          <Heart class="w-4 h-4 text-rose-400 fill-rose-400" /> 1.2k
                        </button>
                        <button class="flex items-center gap-1.5 hover:text-cyan-400 transition-colors cursor-pointer">
                          <MessageSquare class="w-4 h-4" /> 340
                        </button>
                      </div>
                      <button class="flex items-center gap-1.5 hover:text-white transition-colors cursor-pointer">
                        <Share2 class="w-4 h-4" /> Поділитись
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Цікаве від Спільноти (Community Feed for Game) -->
              <div class="bg-[#111C24] border border-white/10 rounded-2xl p-6 space-y-4">
                <h3 class="text-sm font-bold text-white uppercase tracking-wider flex items-center gap-2">
                  <Users class="w-4 h-4 text-cyan-400" /> Цікаве від Спільноти
                </h3>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {#each mockCommunityHighlights.slice(0, 2) as post}
                    <div class="bg-[#16222F]/80 border border-white/10 rounded-xl p-3 space-y-3">
                      <div class="flex items-center gap-2">
                        <img src={post.authorAvatar} alt="" class="w-7 h-7 rounded-full object-cover" />
                        <span class="text-xs font-bold text-white">{post.authorName}</span>
                      </div>
                      <img src={post.mediaUrl} alt="" class="w-full h-32 object-cover rounded-lg" />
                      <p class="text-xs text-slate-300 line-clamp-2">{post.caption}</p>
                    </div>
                  {/each}
                </div>
              </div>
            </div>

            <!-- Right 30% Sidebar -->
            <div class="lg:col-span-4 space-y-6">
              <!-- Друзі бажають цю гру -->
              <div class="bg-[#111C24] border border-white/10 rounded-2xl p-5 space-y-3">
                <div class="flex items-center justify-between">
                  <h4 class="text-xs font-bold uppercase tracking-wider text-slate-300">Друзі бажають эту гру</h4>
                  <span class="text-xs font-bold text-cyan-400">2</span>
                </div>
                <div class="flex items-center gap-2">
                  <img src="https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150" alt="" class="w-8 h-8 rounded-full border border-cyan-500/30" />
                  <img src="https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=150" alt="" class="w-8 h-8 rounded-full border border-cyan-500/30" />
                  <span class="text-xs text-slate-400">CyberViper та NikaNii</span>
                </div>
              </div>

              <!-- Друзі мають эту гру -->
              <div class="bg-[#111C24] border border-white/10 rounded-2xl p-5 space-y-3">
                <div class="flex items-center justify-between">
                  <h4 class="text-xs font-bold uppercase tracking-wider text-slate-300">Друзі мають цю гру</h4>
                  <span class="text-xs font-bold text-cyan-400">11</span>
                </div>
                <div class="grid grid-cols-4 gap-2">
                  {#each Array(8) as _, i}
                    <img
                      src={`https://images.unsplash.com/photo-${1535713875002 + i}?w=100`}
                      alt="Friend Avatar"
                      class="w-10 h-10 rounded-xl object-cover border border-white/10"
                    />
                  {/each}
                </div>
              </div>
            </div>
          </div>
        </div>

      {:else}
        <!-- ROUTE 1: Library Home -->

        <!-- Top Section: Новини (News) -->
        <section class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-lg sm:text-xl font-black text-white font-['Outfit'] flex items-center gap-2">
              <Sparkles class="w-5 h-5 text-cyan-400" /> Новини
            </h2>
            <button class="text-xs font-bold text-cyan-400 hover:text-cyan-300 flex items-center gap-1 transition-colors cursor-pointer">
              Усі новини &gt;
            </button>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            {#each mockNews as news}
              <div class="bg-[#111C24] border border-white/10 hover:border-cyan-500/40 rounded-2xl overflow-hidden transition-all duration-300 group flex flex-col">
                <div class="relative h-40 overflow-hidden">
                  <img
                    src={news.bannerUrl}
                    alt={news.headline}
                    class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                  />
                  <span class="absolute top-3 left-3 px-2.5 py-1 rounded-lg bg-[#0A0D14]/80 backdrop-blur-md text-[10px] font-bold text-cyan-400 border border-cyan-500/30">
                    {news.gameTag}
                  </span>
                </div>

                <div class="p-4 flex-1 flex flex-col justify-between space-y-3">
                  <div>
                    <h3 class="text-sm font-bold text-white group-hover:text-cyan-300 transition-colors line-clamp-2">
                      {news.headline}
                    </h3>
                    <p class="text-xs text-slate-400 mt-1.5 line-clamp-2 leading-relaxed">
                      {news.summary}
                    </p>
                  </div>

                  <div class="pt-3 border-t border-white/5 flex items-center justify-between text-[11px] text-slate-500">
                    <div class="flex items-center gap-3">
                      <span class="flex items-center gap-1 text-slate-400">
                        <Heart class="w-3.5 h-3.5 text-rose-400 fill-rose-400" /> {(news.likes / 1000).toFixed(1)}k
                      </span>
                      <span class="flex items-center gap-1 text-slate-400">
                        <MessageSquare class="w-3.5 h-3.5" /> {(news.comments / 1000).toFixed(1)}k
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
            <h2 class="text-lg sm:text-xl font-black text-white font-['Outfit'] flex items-center gap-2">
              <Users class="w-5 h-5 text-cyan-400" /> Цікаве від Спільноти
            </h2>
            <button class="text-xs font-bold text-cyan-400 hover:text-cyan-300 flex items-center gap-1 transition-colors cursor-pointer">
              Моя стрічка &gt;
            </button>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            {#each mockCommunityHighlights as post}
              <div class="bg-[#111C24] border border-white/10 hover:border-cyan-500/40 rounded-2xl p-4 space-y-3 transition-all">
                <!-- User Avatar Header -->
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-2.5">
                    <img src={post.authorAvatar} alt={post.authorName} class="w-8 h-8 rounded-full object-cover ring-1 ring-cyan-500/30" />
                    <span class="text-xs font-bold text-white">{post.authorName}</span>
                  </div>
                  <button class="text-slate-500 hover:text-slate-300 transition-colors cursor-pointer">
                    <MoreHorizontal class="w-4 h-4" />
                  </button>
                </div>

                <!-- Media Content -->
                <div class="relative rounded-xl overflow-hidden border border-white/5 h-44 bg-slate-900 group">
                  <img src={post.mediaUrl} alt="" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
                  {#if post.mediaType === 'video'}
                    <div class="absolute inset-0 flex items-center justify-center bg-black/30 backdrop-blur-[2px]">
                      <div class="w-12 h-12 rounded-full bg-cyan-400 text-black flex items-center justify-center shadow-lg shadow-cyan-400/40 group-hover:scale-110 transition-transform">
                        <Play class="w-5 h-5 fill-black ml-0.5" />
                      </div>
                    </div>
                  {/if}
                </div>

                <!-- Caption -->
                <p class="text-xs text-slate-300 line-clamp-2 leading-relaxed">
                  {post.caption}
                </p>

                <!-- Stats Footer -->
                <div class="pt-2 border-t border-white/5 flex items-center justify-between text-[11px] text-slate-500">
                  <div class="flex items-center gap-3">
                    <span class="flex items-center gap-1 text-slate-400">
                      <Heart class="w-3.5 h-3.5 text-rose-400 fill-rose-400" /> {(post.likes / 1000).toFixed(1)}k
                    </span>
                    <span class="flex items-center gap-1 text-slate-400">
                      <MessageSquare class="w-3.5 h-3.5" /> {post.comments}
                    </span>
                  </div>
                  <span>{post.date}</span>
                </div>
              </div>
            {/each}
          </div>
        </section>

        <!-- Bottom Section: Game Collection -->
        <section class="space-y-4">
          <!-- Tab Switcher Header -->
          <div class="flex flex-wrap items-center justify-between gap-4 border-b border-white/10 pb-3">
            <div class="flex items-center gap-6 text-sm font-extrabold">
              <button
                onclick={() => activeTab = 'all'}
                class="transition-colors cursor-pointer pb-3 relative {activeTab === 'all' ? 'text-cyan-400' : 'text-slate-400 hover:text-white'}"
              >
                Усі ігри
                {#if activeTab === 'all'}
                  <span class="absolute bottom-0 left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>
                {/if}
              </button>

              <button
                onclick={() => activeTab = 'favorites'}
                class="transition-colors cursor-pointer pb-3 relative {activeTab === 'favorites' ? 'text-cyan-400' : 'text-slate-400 hover:text-white'}"
              >
                Обране
                {#if activeTab === 'favorites'}
                  <span class="absolute bottom-0 left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>
                {/if}
              </button>

              <button
                onclick={() => activeTab = 'collections'}
                class="transition-colors cursor-pointer pb-3 relative {activeTab === 'collections' ? 'text-cyan-400' : 'text-slate-400 hover:text-white'}"
              >
                Моя колекція
                {#if activeTab === 'collections'}
                  <span class="absolute bottom-0 left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>
                {/if}
              </button>
            </div>

            <button
              onclick={() => uiStore.addToast({ title: 'Колекція', message: 'Створення нової колекції скоро буде доступно!', type: 'info' })}
              class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-[#16222F] hover:bg-slate-800 text-xs font-bold text-cyan-400 border border-cyan-500/30 transition-all cursor-pointer"
            >
              <Plus class="w-4 h-4" /> Створити колекцію
            </button>
          </div>

          <!-- Collection Content: Grid vs List Mode -->
          {#if viewMode === 'grid'}
            <!-- Mode A: Grid View (6-column 3:4 Aspect Ratio) -->
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
              {#each filteredGames() as game (game.id)}
                <div
                  role="button"
                  tabindex="0"
                  onclick={() => selectGame(game.id)}
                  onkeydown={(e) => e.key === 'Enter' && selectGame(game.id)}
                  class="group relative bg-[#111C24] border border-white/10 hover:border-cyan-400/60 rounded-2xl overflow-hidden transition-all duration-300 hover:-translate-y-1.5 shadow-lg cursor-pointer flex flex-col"
                >
                  <!-- 3:4 Poster Image Container -->
                  <div class="aspect-[3/4] w-full overflow-hidden relative bg-slate-900">
                    <img
                      src={game.coverImageUrl || game.headerImageUrl}
                      alt={game.title}
                      class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                    />
                    <div class="absolute inset-0 bg-gradient-to-t from-[#0A0D14] via-transparent to-transparent opacity-80 group-hover:opacity-60 transition-opacity"></div>

                    <!-- Favorite Star Badge -->
                    {#if game.isFavorite}
                      <div class="absolute top-2 right-2 p-1.5 rounded-full bg-black/60 backdrop-blur-md">
                        <Star class="w-3.5 h-3.5 fill-amber-400 text-amber-400" />
                      </div>
                    {/if}
                  </div>

                  <!-- Game Title Footer -->
                  <div class="p-3 bg-[#111C24]">
                    <h3 class="text-xs font-bold text-white truncate group-hover:text-cyan-300 transition-colors">
                      {game.title}
                    </h3>
                  </div>
                </div>
              {/each}
            </div>
          {:else}
            <!-- Mode B: List View -->
            <div class="space-y-3">
              {#each filteredGames() as game (game.id)}
                <div
                  class="bg-[#111C24] border border-white/10 hover:border-cyan-400/50 rounded-2xl p-3.5 flex flex-wrap items-center justify-between gap-4 transition-all hover:bg-[#16222F]/60"
                >
                  <!-- Left: Poster + Title -->
                  <div class="flex items-center gap-4 min-w-[200px] flex-1">
                    <img
                      src={game.headerImageUrl || game.coverImageUrl}
                      alt={game.title}
                      class="w-24 h-14 rounded-xl object-cover ring-1 ring-white/10 shrink-0"
                    />
                    <div>
                      <button type="button" class="text-sm font-bold text-white hover:text-cyan-300 transition-colors cursor-pointer text-left block" onclick={() => selectGame(game.id)}>
                        {game.title}
                      </button>
                      <p class="text-xs text-slate-400 mt-0.5">Розмір на диску: 10 ГБ</p>
                    </div>
                  </div>

                  <!-- Right: Primary Action Button + Icons -->
                  <div class="flex items-center gap-3">
                    <button
                      onclick={() => launchGame(game.title)}
                      class="px-5 py-2.5 rounded-xl bg-cyan-400 hover:bg-cyan-300 text-black font-extrabold text-xs shadow-md shadow-cyan-400/20 transition-all cursor-pointer flex items-center gap-2"
                    >
                      <Play class="w-4 h-4 fill-black" />
                      <span>Скачати</span>
                    </button>

                    <button
                      onclick={() => libraryStore.toggleFavorite(game.id)}
                      class="p-2.5 rounded-xl bg-[#16222F] hover:bg-slate-800 text-slate-300 border border-white/10 transition-colors cursor-pointer"
                      title="Favorite"
                    >
                      <Star class="w-4 h-4 {game.isFavorite ? 'fill-amber-400 text-amber-400' : ''}" />
                    </button>

                    <button
                      class="p-2.5 rounded-xl bg-[#16222F] hover:bg-slate-800 text-slate-300 border border-white/10 transition-colors cursor-pointer"
                      title="Опції"
                    >
                      <MoreHorizontal class="w-4 h-4" />
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
    width: 6px;
  }
  .custom-scrollbar::-webkit-scrollbar-track {
    background: transparent;
  }
  .custom-scrollbar::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.1);
    border-radius: 9999px;
  }
  .custom-scrollbar::-webkit-scrollbar-thumb:hover {
    background: rgba(34, 211, 238, 0.3);
  }
</style>
