<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { libraryStore } from '../../stores/libraryStore';
  import { uiStore } from '../../stores/uiStore';
  import { profileStore } from '../../stores/profileStore';
  import { communityService, type CommunityPost } from '../../services/communityService';
  import { formatPlayTime } from '../../utils/formatters';
  import type { Game, UserGame } from '../../types';
  import LibraryGameDetailsView from './LibraryGameDetailsView.svelte';
  import {
    Search,
    LayoutGrid,
    List,
    Heart,
    Gamepad2,
    ChevronLeft,
    ChevronRight,
    MessageSquare,
    Sliders,
    Plus,
  } from 'lucide-svelte';

  type LibraryTab = 'all' | 'favorites';

  let searchQuery = $state('');
  let viewMode = $state<'grid' | 'list'>('grid');
  let activeTab = $state<LibraryTab>('all');

  let openedItem = $state<UserGame | null>(null);

  let newsPosts = $state<CommunityPost[]>([]);
  let isLoadingNews = $state(false);
  let newsIndex = $state(0);
  let newsAutoplayHandle: ReturnType<typeof setInterval> | undefined;

  let communityFeed = $state<CommunityPost[]>([]);
  let isLoadingCommunity = $state(false);

  async function loadNews() {
    isLoadingNews = true;
    try {
      const res = await communityService.getPosts(null, 'news');
      newsPosts = res.posts.length > 0 ? res.posts : [];
    } catch (e) {
      console.warn('[LibraryView] Не вдалося завантажити новини:', e);
      newsPosts = [];
    } finally {
      isLoadingNews = false;
    }
  }

  async function loadCommunityFeed() {
    isLoadingCommunity = true;
    try {
      const res = await communityService.getPosts(null, 'all');
      communityFeed = res.posts.slice(0, 3);
    } catch (e) {
      console.warn('[LibraryView] Не вдалося завантажити стрічку спільноти:', e);
      communityFeed = [];
    } finally {
      isLoadingCommunity = false;
    }
  }

  function startNewsAutoplay() {
    stopNewsAutoplay();
    newsAutoplayHandle = setInterval(() => {
      if (newsPosts.length > 1) {
        newsIndex = (newsIndex + 1) % newsPosts.length;
      }
    }, 5000);
  }

  function stopNewsAutoplay() {
    if (newsAutoplayHandle) {
      clearInterval(newsAutoplayHandle);
      newsAutoplayHandle = undefined;
    }
  }

  function nextNews() {
    if (newsPosts.length === 0) return;
    newsIndex = (newsIndex + 1) % newsPosts.length;
    startNewsAutoplay();
  }

  function prevNews() {
    if (newsPosts.length === 0) return;
    newsIndex = (newsIndex - 1 + newsPosts.length) % newsPosts.length;
    startNewsAutoplay();
  }

  const visibleNews = $derived.by(() => {
    if (newsPosts.length === 0) return [];
    const items: CommunityPost[] = [];
    for (let i = 0; i < Math.min(3, newsPosts.length); i++) {
      items.push(newsPosts[(newsIndex + i) % newsPosts.length]);
    }
    return items;
  });

  onMount(() => {
    libraryStore.loadLibrary();
    loadNews().then(startNewsAutoplay);
    loadCommunityFeed();
  });

  onDestroy(() => {
    stopNewsAutoplay();
  });

  function openGame(item: UserGame) {
    if (!item.game) return;
    openedItem = item;
  }

  function closeGameDetails() {
    openedItem = null;
  }

  function handleToggleFavorite(e: MouseEvent, gameId: string) {
    e.stopPropagation();
    libraryStore.toggleFavorite(gameId);
  }

  const filteredItems = $derived(
    ($libraryStore.items as UserGame[])
      .filter((item) => !!item.game)
      .filter((item) => (activeTab === 'favorites' ? item.isFavorite : true))
      .filter((item) => {
        if (!searchQuery.trim()) return true;
        const q = searchQuery.toLowerCase();
        return item.game!.title.toLowerCase().includes(q);
      })
  );

  const totalCount = $derived($libraryStore.items.length);
  const favoritesCount = $derived($libraryStore.items.filter((i) => i.isFavorite).length);

  const openedGame = $derived<Game | undefined>(openedItem?.game);
</script>

<div class="flex h-full min-h-[calc(100vh-56px)]">
  <aside class="w-60 shrink-0 border-r border-cyan-500/10 bg-[#04121a]/60 hidden md:flex flex-col">
    <div class="px-5 py-4 flex items-center justify-between">
      <span class="text-sm font-extrabold text-white">Усі ігри</span>
      <button class="text-slate-400 hover:text-white transition-colors cursor-pointer" title="Фільтри">
        <Sliders class="w-4 h-4" />
      </button>
    </div>
    <ul class="flex-1 overflow-y-auto px-2 space-y-0.5 pb-4">
      {#each $libraryStore.items as item (item.gameId)}
        {#if item.game}
          <li>
            <button
              onclick={() => openGame(item)}
              class="w-full flex items-center gap-3 px-3 py-2 rounded-xl text-left transition-colors cursor-pointer
                {openedItem?.gameId === item.gameId ? 'bg-cyan-500/15 text-cyan-300' : 'text-slate-300 hover:bg-[#0a2733] hover:text-white'}"
            >
              <img
                src={item.game.coverImageUrl || item.game.headerImageUrl || 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=100'}
                alt={item.game.title}
                class="w-7 h-7 rounded-md object-cover shrink-0"
              />
              <span class="text-xs font-semibold truncate">{item.game.title}</span>
            </button>
          </li>
        {/if}
      {/each}
    </ul>
  </aside>

  <main class="flex-1 min-w-0">
    <div class="max-w-7xl mx-auto px-4 lg:px-8 py-6 space-y-8 animate-in fade-in">
      <div class="flex flex-col sm:flex-row sm:items-center gap-3">
        <div class="relative flex-1">
          <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-cyan-400" />
          <input
            type="text"
            bind:value={searchQuery}
            placeholder="Пошук у Крамниці..."
            class="w-full pl-10 pr-4 py-2.5 rounded-xl bg-[#061820]/90 border border-cyan-500/20 focus:border-cyan-400 text-sm text-white placeholder:text-slate-500 outline-none transition-colors"
          />
        </div>

        <button
          type="button"
          class="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-[#061820]/90 border border-cyan-500/20 hover:border-cyan-400/60 text-xs font-bold text-slate-300 hover:text-white transition-colors cursor-pointer shrink-0"
        >
          <Sliders class="w-3.5 h-3.5" />
          Фільтри
        </button>

        {#if !openedItem}
          <div class="flex items-center gap-1 p-1 rounded-xl bg-[#061820]/90 border border-cyan-500/20 shrink-0">
            <button
              type="button"
              onclick={() => (viewMode = 'grid')}
              title="Сітка"
              class="p-2 rounded-lg transition-colors cursor-pointer {viewMode === 'grid' ? 'bg-cyan-500/20 text-cyan-300' : 'text-slate-400 hover:text-white'}"
            >
              <LayoutGrid class="w-4 h-4" />
            </button>
            <button
              type="button"
              onclick={() => (viewMode = 'list')}
              title="Список"
              class="p-2 rounded-lg transition-colors cursor-pointer {viewMode === 'list' ? 'bg-cyan-500/20 text-cyan-300' : 'text-slate-400 hover:text-white'}"
            >
              <List class="w-4 h-4" />
            </button>
          </div>
        {/if}
      </div>

      {#if openedItem && openedGame}
        <LibraryGameDetailsView game={openedGame} onBack={closeGameDetails} />
      {:else}
        <section class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-lg font-extrabold text-white">Новини</h2>
            <button
              onclick={() => uiStore.addToast({ title: 'Новини', message: 'Розділ усіх новин скоро зʼявиться.', type: 'info' })}
              class="text-xs font-bold text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer"
            >
              Усі новини &gt;
            </button>
          </div>

          {#if isLoadingNews}
            <div class="flex items-center justify-center py-10 text-slate-400 text-xs gap-2">
              <span class="animate-spin text-cyan-400 text-lg">⟳</span>
              <span>Завантаження новин...</span>
            </div>
          {:else if visibleNews.length > 0}
            <div class="relative">
              {#if newsPosts.length > 3}
                <button
                  onclick={prevNews}
                  class="hidden md:flex absolute -left-4 top-1/3 -translate-y-1/2 w-8 h-8 rounded-full bg-black/70 hover:bg-cyan-500 hover:text-black text-white border border-white/20 items-center justify-center transition-all z-10 cursor-pointer shadow-lg"
                  aria-label="Попередні новини"
                >
                  <ChevronLeft class="w-4 h-4" />
                </button>
                <button
                  onclick={nextNews}
                  class="hidden md:flex absolute -right-4 top-1/3 -translate-y-1/2 w-8 h-8 rounded-full bg-black/70 hover:bg-cyan-500 hover:text-black text-white border border-white/20 items-center justify-center transition-all z-10 cursor-pointer shadow-lg"
                  aria-label="Наступні новини"
                >
                  <ChevronRight class="w-4 h-4" />
                </button>
              {/if}

              <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
                {#each visibleNews as post (post.id)}
                  <article class="bg-[#061820]/90 border border-cyan-500/20 rounded-2xl overflow-hidden shadow-lg hover:border-cyan-400/50 transition-all flex flex-col justify-between">
                    {#if post.media?.url}
                      {#if post.media.type === 'video'}
                        <video src={post.media.url} class="w-full h-32 object-cover" muted></video>
                      {:else}
                        <img src={post.media.url} alt="" class="w-full h-32 object-cover" />
                      {/if}
                    {:else if post.gameBannerUrl}
                      <img src={post.gameBannerUrl} alt="" class="w-full h-32 object-cover opacity-80" />
                    {/if}

                    <div class="p-4 space-y-2 flex-1 flex flex-col justify-between">
                      <div class="space-y-1.5">
                        <div class="flex items-center justify-between gap-2">
                          <button
                            onclick={() => profileStore.viewProfile(post.author.id)}
                            class="flex items-center gap-2 group cursor-pointer"
                          >
                            <img
                              src={post.author.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(post.author.username)}`}
                              alt={post.author.username}
                              class="w-5 h-5 rounded-full object-cover border border-cyan-500/30"
                            />
                            <span class="text-xs font-bold text-slate-200 group-hover:text-cyan-300 transition-colors truncate max-w-[120px]">{post.author.username}</span>
                          </button>
                          {#if post.gameTitle}
                            <span class="text-[10px] font-bold px-2 py-0.5 rounded bg-cyan-950/80 text-cyan-300 border border-cyan-800/40 truncate max-w-[140px]">
                              {post.gameTitle}
                            </span>
                          {/if}
                        </div>

                        <h3 class="text-sm font-bold text-white line-clamp-1">{post.title}</h3>
                        <p class="text-[11px] text-slate-400 leading-relaxed line-clamp-2">{post.content}</p>
                      </div>

                      <div class="flex items-center gap-3 text-[11px] text-slate-500 pt-1">
                        <span class="flex items-center gap-1"><Heart class="w-3 h-3" />{post.stats.likesCount}</span>
                        <span class="flex items-center gap-1"><MessageSquare class="w-3 h-3" />{post.stats.commentsCount}</span>
                        <span class="ml-auto">{new Date(post.createdAt).toLocaleDateString('uk-UA')}</span>
                      </div>
                    </div>
                  </article>
                {/each}
              </div>

              {#if newsPosts.length > 1}
                <div class="flex items-center justify-center gap-1.5 mt-4">
                  {#each newsPosts as _, idx}
                    <button
                      onclick={() => { newsIndex = idx; startNewsAutoplay(); }}
                      class="h-1.5 rounded-full transition-all cursor-pointer {idx === newsIndex ? 'w-6 bg-cyan-400' : 'w-1.5 bg-cyan-500/20 hover:bg-cyan-500/40'}"
                      aria-label="Новина {idx + 1}"
                    ></button>
                  {/each}
                </div>
              {/if}
            </div>
          {:else}
            <div class="text-center py-10 bg-[#061820]/60 rounded-2xl border border-cyan-500/20">
              <p class="text-xs text-slate-400">Новин поки немає.</p>
            </div>
          {/if}
        </section>

        <section class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-lg font-extrabold text-white">Цікаве від Спільноти</h2>
            <button
              onclick={() => uiStore.setTab('community')}
              class="text-xs font-bold text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer"
            >
              Моя стрічка &gt;
            </button>
          </div>

          {#if isLoadingCommunity}
            <div class="flex items-center justify-center py-10 text-slate-400 text-xs gap-2">
              <span class="animate-spin text-cyan-400 text-lg">⟳</span>
              <span>Завантаження стрічки...</span>
            </div>
          {:else if communityFeed.length > 0}
            <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
              {#each communityFeed as post (post.id)}
                <article class="bg-[#061820]/90 border border-cyan-500/20 rounded-2xl overflow-hidden shadow-lg hover:border-cyan-400/50 transition-all">
                  <div class="p-4 pb-0">
                    <button
                      onclick={() => profileStore.viewProfile(post.author.id)}
                      class="flex items-center gap-2 group cursor-pointer"
                    >
                      <img
                        src={post.author.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(post.author.username)}`}
                        alt={post.author.username}
                        class="w-6 h-6 rounded-full object-cover border border-cyan-500/30"
                      />
                      <span class="text-xs font-bold text-slate-200 group-hover:text-cyan-300 transition-colors">{post.author.username}</span>
                    </button>
                  </div>

                  {#if post.media?.type === 'image' && post.media.url}
                    <div class="relative mt-3">
                      <img src={post.media.url} alt="" class="w-full h-40 object-cover" />
                    </div>
                  {:else if post.media?.type === 'video' && post.media.url}
                    <div class="relative mt-3">
                      <video src={post.media.url} class="w-full h-40 object-cover" muted></video>
                      <div class="absolute inset-0 flex items-center justify-center">
                        <div class="w-10 h-10 rounded-full bg-black/50 border-2 border-white flex items-center justify-center">
                          <div class="w-0 h-0 border-l-[10px] border-l-white border-y-[6px] border-y-transparent ml-1"></div>
                        </div>
                      </div>
                    </div>
                  {/if}

                  <div class="p-4 space-y-1.5">
                    {#if post.title}<h4 class="text-sm font-bold text-white line-clamp-1">{post.title}</h4>{/if}
                    <p class="text-[11px] text-slate-400 leading-relaxed line-clamp-2">{post.content}</p>
                    <div class="flex items-center gap-3 text-[11px] text-slate-500 pt-1">
                      <span class="flex items-center gap-1"><Heart class="w-3 h-3" />{post.stats.likesCount}</span>
                      <span class="flex items-center gap-1"><MessageSquare class="w-3 h-3" />{post.stats.commentsCount}</span>
                      <span class="ml-auto">{new Date(post.createdAt).toLocaleDateString('uk-UA')}</span>
                    </div>
                  </div>
                </article>
              {/each}
            </div>
          {:else}
            <div class="text-center py-10 bg-[#061820]/60 rounded-2xl border border-cyan-500/20">
              <p class="text-xs text-slate-400">Постів спільноти поки немає.</p>
            </div>
          {/if}
        </section>

        <section class="space-y-4">
          <div class="flex items-center gap-6 border-b border-cyan-500/10">
            <button
              type="button"
              onclick={() => (activeTab = 'all')}
              class="pb-3 text-sm font-bold transition-colors cursor-pointer relative {activeTab === 'all' ? 'text-cyan-300' : 'text-slate-400 hover:text-white'}"
            >
              Усі ігри
              {#if activeTab === 'all'}<span class="absolute -bottom-px left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>{/if}
            </button>
            <button
              type="button"
              onclick={() => (activeTab = 'favorites')}
              class="pb-3 text-sm font-bold transition-colors cursor-pointer relative {activeTab === 'favorites' ? 'text-cyan-300' : 'text-slate-400 hover:text-white'}"
            >
              Обране
              {#if activeTab === 'favorites'}<span class="absolute -bottom-px left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>{/if}
            </button>
            <button
              type="button"
              class="pb-3 text-sm font-bold transition-colors cursor-pointer relative flex items-center gap-1.5 text-slate-400 hover:text-white"
            >
              Моя колекція
              <Plus class="w-3.5 h-3.5" />
            </button>
          </div>

          {#if $libraryStore.isLoading}
            <div class="text-center py-20 text-slate-400 text-xs flex items-center justify-center gap-2">
              <span class="animate-spin text-cyan-400 text-lg">⟳</span>
              <span>Завантаження бібліотеки...</span>
            </div>
          {:else if filteredItems.length > 0}
            {#if viewMode === 'grid'}
              <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-5">
                {#each filteredItems as item (item.gameId)}
                  {@const g = item.game!}
                  <div
                    role="button"
                    tabindex="0"
                    onclick={() => openGame(item)}
                    onkeydown={(e) => e.key === 'Enter' && openGame(item)}
                    class="group relative flex flex-col rounded-2xl overflow-hidden bg-slate-950 border border-cyan-500/20 hover:border-cyan-400/80 shadow-lg hover:shadow-2xl hover:shadow-cyan-500/20 transition-all duration-300 transform hover:-translate-y-1.5 cursor-pointer"
                  >
                    <div class="relative aspect-[3/4] w-full overflow-hidden">
                      <img
                        src={g.coverImageUrl || g.headerImageUrl || 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=800&auto=format&fit=crop&q=80'}
                        alt={g.title}
                        class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                      />
                      <div class="absolute inset-0 bg-gradient-to-t from-black/70 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity"></div>

                      <button
                        type="button"
                        onclick={(e) => handleToggleFavorite(e, item.gameId)}
                        class="absolute top-2.5 right-2.5 p-1.5 rounded-xl bg-black/70 backdrop-blur-md border transition-all cursor-pointer opacity-0 group-hover:opacity-100
                          {item.isFavorite
                            ? 'border-cyan-400/60 text-cyan-300 opacity-100'
                            : 'border-white/20 text-slate-300 hover:text-cyan-300 hover:border-cyan-400/60'}"
                        title={item.isFavorite ? 'Видалити з обраного' : 'Додати до обраного'}
                      >
                        <Heart class="w-3.5 h-3.5 {item.isFavorite ? 'fill-cyan-300' : ''}" />
                      </button>
                    </div>
                  </div>
                {/each}
              </div>
            {:else}
              <div class="space-y-3">
                {#each filteredItems as item (item.gameId)}
                  {@const g = item.game!}
                  <div
                    role="button"
                    tabindex="0"
                    onclick={() => openGame(item)}
                    onkeydown={(e) => e.key === 'Enter' && openGame(item)}
                    class="group flex items-center gap-6 p-3 rounded-2xl bg-[#061820]/95 hover:bg-[#092530] border border-cyan-500/20 hover:border-cyan-400/60 shadow-lg transition-all duration-300 cursor-pointer"
                  >
                    <img
                      src={g.headerImageUrl || g.coverImageUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600'}
                      alt={g.title}
                      class="w-44 h-20 rounded-xl object-cover shrink-0"
                    />

                    <div class="flex-1 flex items-center gap-4 min-w-0">
                      <h4 class="text-base font-bold text-white truncate">{g.title}</h4>
                      <button
                        onclick={(e) => { e.stopPropagation(); uiStore.addToast({ title: 'Завантаження', message: `Завантаження '${g.title}' розпочато.`, type: 'success' }); }}
                        class="px-5 py-1.5 rounded-full bg-cyan-500 hover:bg-cyan-400 text-black font-bold text-xs transition-all cursor-pointer shrink-0"
                      >
                        Скачати
                      </button>
                    </div>

                    <div class="flex flex-col items-end text-xs text-slate-400 shrink-0">
                      <span>Розмір на диску</span>
                      <strong class="text-white text-sm">{formatPlayTime(item.playTimeMinutes)}</strong>
                    </div>

                    <div class="flex items-center gap-2 shrink-0">
                      <button
                        onclick={(e) => handleToggleFavorite(e, item.gameId)}
                        class="w-9 h-9 rounded-full flex items-center justify-center transition-colors cursor-pointer
                          {item.isFavorite ? 'bg-cyan-500/20 text-cyan-300' : 'bg-[#112833] text-slate-300 hover:text-cyan-300'}"
                      >
                        <Heart class="w-4 h-4 {item.isFavorite ? 'fill-cyan-300' : ''}" />
                      </button>
                    </div>
                  </div>
                {/each}
              </div>
            {/if}
          {:else if activeTab === 'favorites'}
            <div class="text-center py-20 bg-[#061820]/60 rounded-3xl border border-cyan-500/20 shadow-xl space-y-3">
              <div class="w-16 h-16 rounded-2xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center text-cyan-400 mx-auto">
                <Heart class="w-8 h-8 text-cyan-400/70" />
              </div>
              <h4 class="text-lg font-bold text-white">Немає обраних ігор</h4>
              <p class="text-xs text-slate-400 max-w-sm mx-auto">
                Позначайте улюблені ігри сердечком ♥, щоб швидко знаходити їх тут.
              </p>
            </div>
          {:else}
            <div class="text-center py-20 bg-[#061820]/60 rounded-3xl border border-cyan-500/20 shadow-xl space-y-3">
              <div class="w-16 h-16 rounded-2xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center text-cyan-400 mx-auto">
                <Gamepad2 class="w-8 h-8 text-cyan-400/70" />
              </div>
              <h4 class="text-lg font-bold text-white">Бібліотека порожня</h4>
              <p class="text-xs text-slate-400 max-w-sm mx-auto">
                Придбайте ігри в крамниці, щоб вони з’явилися тут.
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
        </section>
      {/if}
    </div>
  </main>
</div>