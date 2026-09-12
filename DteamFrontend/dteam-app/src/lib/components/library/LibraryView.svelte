<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { libraryStore } from '../../stores/libraryStore';
  import { uiStore } from '../../stores/uiStore';
  import { profileStore } from '../../stores/profileStore';
  import { communityService, type CommunityPost } from '../../services/communityService';
  import { formatPlayTime } from '../../utils/formatters';
  import { renderDecoratedText, resolveMediaUrl } from '../../utils/textDecorator';
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
    Folder,
    FolderPlus,
    Edit2,
    Trash2,
    Rss,
  } from 'lucide-svelte';
  import CreateCollectionModal from './CreateCollectionModal.svelte';
  import LibraryMyFeedView from './LibraryMyFeedView.svelte';
  import type { GameCollection } from '../../types';

  let searchQuery = $state('');
  let viewMode = $state<'grid' | 'list'>('grid');
  let showCollectionModal = $state(false);
  let editingCollection = $state<GameCollection | null>(null);

  let openedItem = $state<UserGame | null>(null);

  let newsPosts = $state<CommunityPost[]>([]);
  let isLoadingNews = $state(false);
  let newsIndex = $state(0);
  let newsAutoplayHandle: ReturnType<typeof setInterval> | undefined;

  let communityFeed = $state<CommunityPost[]>([]);
  let isLoadingCommunity = $state(false);

  async function handleDeleteCollection(col: GameCollection) {
    if (!confirm(`Ви дійсно бажаєте видалити колекцію "${col.name}"?`)) return;
    try {
      await libraryStore.deleteCollection(col.id);
    } catch (e) {
      console.error(e);
    }
  }

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
      .filter((item) => {
        const colId = $libraryStore.activeCollectionId;
        if (colId === 'all') return true;
        if (colId === 'favorites') return item.isFavorite;
        const col = $libraryStore.collections.find((c) => c.id === colId);
        if (!col) return true;
        return (col.items ?? []).some((i) => i.gameId === item.gameId);
      })
      .filter((item) => {
        if (!searchQuery.trim()) return true;
        const q = searchQuery.toLowerCase();
        return item.game!.title.toLowerCase().includes(q);
      })
  );

  const totalCount = $derived($libraryStore.items.length);
  const favoritesCount = $derived($libraryStore.items.filter((i) => i.isFavorite).length);
  const activeCustomCollection = $derived<GameCollection | undefined>(
    $libraryStore.collections.find((c) => c.id === $libraryStore.activeCollectionId)
  );

  const openedGame = $derived<Game | undefined>(openedItem?.game);
</script>

<div class="flex h-full min-h-[calc(100vh-56px)]">
  <aside class="w-60 shrink-0 border-r border-cyan-500/10 bg-[#04121a]/60 hidden md:flex flex-col">
    <div class="px-4 py-3.5 flex items-center justify-between border-b border-cyan-500/10">
      <span class="text-xs font-extrabold text-white uppercase tracking-wider">Колекції</span>
      <button
        onclick={() => { editingCollection = null; showCollectionModal = true; }}
        class="p-1.5 rounded-lg text-slate-400 hover:text-cyan-300 hover:bg-cyan-500/15 transition-colors cursor-pointer"
        title="Створити нову колекцію"
      >
        <Plus class="w-4 h-4" />
      </button>
    </div>

    <div class="p-2 space-y-0.5 border-b border-cyan-500/10">
      <button
        type="button"
        onclick={() => libraryStore.setActiveCollection('all')}
        class="w-full flex items-center justify-between px-3 py-1.5 rounded-xl text-xs font-bold transition-colors cursor-pointer {$libraryStore.activeCollectionId === 'all' ? 'bg-cyan-500/20 text-cyan-300' : 'text-slate-400 hover:bg-white/5 hover:text-white'}"
      >
        <span>Усі ігри</span>
        <span class="text-[10px] px-1.5 py-0.2 rounded-full bg-black/40">{totalCount}</span>
      </button>
      <button
        type="button"
        onclick={() => libraryStore.setActiveCollection('favorites')}
        class="w-full flex items-center justify-between px-3 py-1.5 rounded-xl text-xs font-bold transition-colors cursor-pointer {$libraryStore.activeCollectionId === 'favorites' ? 'bg-cyan-500/20 text-cyan-300' : 'text-slate-400 hover:bg-white/5 hover:text-white'}"
      >
        <span class="flex items-center gap-1.5"><Heart class="w-3 h-3" /> Обране</span>
        <span class="text-[10px] px-1.5 py-0.2 rounded-full bg-black/40">{favoritesCount}</span>
      </button>
      {#each $libraryStore.collections as col (col.id)}
        {@const isActive = $libraryStore.activeCollectionId === col.id}
        <div class="group/col-item relative flex items-center justify-between rounded-xl px-3 py-1.5 transition-colors cursor-pointer {isActive ? 'bg-cyan-500/20 text-cyan-300' : 'text-slate-400 hover:bg-white/5 hover:text-white'}">
          <button
            type="button"
            onclick={() => libraryStore.setActiveCollection(col.id)}
            class="flex-1 flex items-center gap-1.5 text-xs font-bold text-left min-w-0 cursor-pointer"
          >
            <Folder class="w-3 h-3 shrink-0" />
            <span class="truncate">{col.name}</span>
          </button>

          <div class="flex items-center gap-1 shrink-0 ml-1">
            <div class="flex items-center gap-0.5 opacity-0 group-hover/col-item:opacity-100 transition-opacity">
              <button
                type="button"
                onclick={(e) => { e.stopPropagation(); editingCollection = col; showCollectionModal = true; }}
                class="p-0.5 rounded text-slate-400 hover:text-cyan-300 hover:bg-white/10 transition-colors cursor-pointer"
                title="Редагувати колекцію"
              >
                <Edit2 class="w-3 h-3" />
              </button>
              <button
                type="button"
                onclick={(e) => { e.stopPropagation(); handleDeleteCollection(col); }}
                class="p-0.5 rounded text-slate-400 hover:text-red-400 hover:bg-red-500/15 transition-colors cursor-pointer"
                title="Видалити колекцію"
              >
                <Trash2 class="w-3 h-3" />
              </button>
            </div>
            <span class="text-[10px] px-1.5 py-0.2 rounded-full bg-black/40 shrink-0">{col.items?.length ?? col.gamesCount ?? 0}</span>
          </div>
        </div>
      {/each}
    </div>

    <ul class="flex-1 overflow-y-auto px-2 space-y-0.5 py-2">
      {#each filteredItems as item (item.gameId)}
        {#if item.game}
          <li>
            <button
              onclick={() => openGame(item)}
              class="w-full flex items-center gap-2.5 px-2.5 py-1.5 rounded-xl text-left transition-colors cursor-pointer
                {openedItem?.gameId === item.gameId ? 'bg-cyan-500/15 text-cyan-300' : 'text-slate-300 hover:bg-[#0a2733] hover:text-white'}"
            >
              <img
                src={item.game.coverImageUrl || item.game.headerImageUrl || 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=100'}
                alt={item.game.title}
                class="w-6 h-6 rounded-md object-cover shrink-0"
              />
              <span class="text-xs font-medium truncate">{item.game.title}</span>
            </button>
          </li>
        {/if}
      {/each}
    </ul>
  </aside>

  <main class="flex-1 min-w-0">
    <div class="max-w-7xl mx-auto px-4 lg:px-8 py-6 space-y-8 animate-in fade-in">

      <div class="flex items-center gap-3 border-b border-cyan-500/10 pb-4">
        <button
          onclick={() => libraryStore.setActivePrimaryTab('games')}
          class="flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-extrabold transition-all cursor-pointer {$libraryStore.activePrimaryTab === 'games' ? 'bg-cyan-500/20 text-cyan-300 border border-cyan-400/50 shadow-lg shadow-cyan-500/10' : 'text-slate-400 hover:text-white hover:bg-white/5'}"
        >
          <Gamepad2 class="w-4 h-4" />
          <span>Мої ігри ({totalCount})</span>
        </button>
        <button
          onclick={() => libraryStore.setActivePrimaryTab('feed')}
          class="flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-extrabold transition-all cursor-pointer {$libraryStore.activePrimaryTab === 'feed' ? 'bg-cyan-500/20 text-cyan-300 border border-cyan-400/50 shadow-lg shadow-cyan-500/10' : 'text-slate-400 hover:text-white hover:bg-white/5'}"
        >
          <Rss class="w-4 h-4" />
          <span>Моя стрічка</span>
        </button>
      </div>

      {#if $libraryStore.activePrimaryTab === 'feed'}
        <LibraryMyFeedView />
      {:else}
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
                        <video src={resolveMediaUrl(post.media.url)} class="w-full h-32 object-cover" controls playsinline muted></video>
                      {:else}
                        <img src={resolveMediaUrl(post.media.url)} alt="" class="w-full h-32 object-cover" />
                      {/if}
                    {:else if post.gameBannerUrl}
                      <img src={resolveMediaUrl(post.gameBannerUrl)} alt="" class="w-full h-32 object-cover opacity-80" />
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
                        <p class="text-[11px] text-slate-400 leading-relaxed line-clamp-2">{@html renderDecoratedText(post.content)}</p>
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
                      <img src={resolveMediaUrl(post.media.url)} alt="" class="w-full h-40 object-cover" />
                    </div>
                  {:else if post.media?.type === 'video' && post.media.url}
                    <div class="relative mt-3">
                      <video src={resolveMediaUrl(post.media.url)} class="w-full h-40 object-cover" muted></video>
                      <div class="absolute inset-0 flex items-center justify-center">
                        <div class="w-10 h-10 rounded-full bg-black/50 border-2 border-white flex items-center justify-center">
                          <div class="w-0 h-0 border-l-[10px] border-l-white border-y-[6px] border-y-transparent ml-1"></div>
                        </div>
                      </div>
                    </div>
                  {/if}

                  <div class="p-4 space-y-1.5">
                    {#if post.title}<h4 class="text-sm font-bold text-white line-clamp-1">{post.title}</h4>{/if}
                    <p class="text-[11px] text-slate-400 leading-relaxed line-clamp-2">{@html renderDecoratedText(post.content)}</p>
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
          <div class="flex items-center gap-4 sm:gap-6 border-b border-cyan-500/10 overflow-x-auto pb-1">
            <button
              type="button"
              onclick={() => libraryStore.setActiveCollection('all')}
              class="pb-3 text-sm font-bold transition-colors cursor-pointer relative shrink-0 {$libraryStore.activeCollectionId === 'all' ? 'text-cyan-300' : 'text-slate-400 hover:text-white'}"
            >
              Усі ігри ({totalCount})
              {#if $libraryStore.activeCollectionId === 'all'}<span class="absolute -bottom-px left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>{/if}
            </button>
            <button
              type="button"
              onclick={() => libraryStore.setActiveCollection('favorites')}
              class="pb-3 text-sm font-bold transition-colors cursor-pointer relative shrink-0 flex items-center gap-1.5 {$libraryStore.activeCollectionId === 'favorites' ? 'text-cyan-300' : 'text-slate-400 hover:text-white'}"
            >
              <Heart class="w-3.5 h-3.5 {$libraryStore.activeCollectionId === 'favorites' ? 'fill-cyan-300' : ''}" />
              <span>Обране ({favoritesCount})</span>
              {#if $libraryStore.activeCollectionId === 'favorites'}<span class="absolute -bottom-px left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>{/if}
            </button>

            {#each $libraryStore.collections as col (col.id)}
              {@const isActive = $libraryStore.activeCollectionId === col.id}
              <div class="relative group/col flex items-center shrink-0">
                <button
                  type="button"
                  onclick={() => libraryStore.setActiveCollection(col.id)}
                  class="pb-3 text-sm font-bold transition-colors cursor-pointer relative flex items-center gap-1.5 {isActive ? 'text-cyan-300' : 'text-slate-400 hover:text-white'}"
                >
                  <Folder class="w-3.5 h-3.5 {isActive ? 'text-cyan-400' : 'text-slate-500'}" />
                  <span>{col.name}</span>
                  <span class="text-[10px] px-1.5 py-0.2 rounded-full {isActive ? 'bg-cyan-400 text-black font-extrabold' : 'bg-slate-800 text-slate-400'}">{col.items?.length ?? col.gamesCount ?? 0}</span>
                  {#if isActive}<span class="absolute -bottom-px left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>{/if}
                </button>
                {#if isActive}
                  <div class="flex items-center mb-2.5 ml-1 gap-0.5">
                    <button
                      type="button"
                      onclick={() => { editingCollection = col; showCollectionModal = true; }}
                      class="p-1 rounded text-slate-400 hover:text-cyan-300 hover:bg-white/10 transition-colors cursor-pointer"
                      title="Редагувати колекцію"
                    >
                      <Edit2 class="w-3 h-3" />
                    </button>
                    <button
                      type="button"
                      onclick={() => handleDeleteCollection(col)}
                      class="p-1 rounded text-slate-400 hover:text-red-400 hover:bg-red-500/15 transition-colors cursor-pointer"
                      title="Видалити колекцію"
                    >
                      <Trash2 class="w-3 h-3" />
                    </button>
                  </div>
                {/if}
              </div>
            {/each}

            <button
              type="button"
              onclick={() => { editingCollection = null; showCollectionModal = true; }}
              class="pb-3 text-sm font-bold transition-colors cursor-pointer relative flex items-center gap-1.5 text-cyan-400 hover:text-cyan-300 shrink-0 ml-auto"
            >
              <Plus class="w-3.5 h-3.5" />
              <span>Створити колекцію</span>
            </button>
          </div>

          {#if $libraryStore.isLoading}
            <div class="text-center py-20 text-slate-400 text-xs flex items-center justify-center gap-2">
              <span class="animate-spin text-cyan-400 text-lg">⟳</span>
              <span>Завантаження бібліотеки...</span>
            </div>
          {:else}
            {#if activeCustomCollection}
              <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-4 rounded-2xl bg-[#061e27]/80 border border-cyan-500/20 shadow-lg shadow-cyan-950/30 mb-2 animate-in fade-in">
                <div class="flex items-center gap-3 min-w-0">
                  <div class="w-10 h-10 rounded-xl bg-cyan-500/10 border border-cyan-500/30 flex items-center justify-center text-cyan-300 shrink-0">
                    <Folder class="w-5 h-5" />
                  </div>
                  <div class="min-w-0">
                    <div class="flex items-center gap-2 flex-wrap">
                      <h3 class="text-base font-extrabold text-white truncate">{activeCustomCollection.name}</h3>
                      <span class="text-xs px-2.5 py-0.5 rounded-full bg-cyan-500/20 text-cyan-300 font-mono font-bold">
                        {filteredItems.length} {filteredItems.length === 1 ? 'гра' : 'ігор'}
                      </span>
                    </div>
                    {#if activeCustomCollection.description}
                      <p class="text-xs text-slate-400 mt-0.5 line-clamp-1">{activeCustomCollection.description}</p>
                    {/if}
                  </div>
                </div>

                <div class="flex items-center gap-2 shrink-0">
                  <button
                    type="button"
                    onclick={() => { editingCollection = activeCustomCollection; showCollectionModal = true; }}
                    class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-white/5 hover:bg-white/10 text-xs font-bold text-slate-300 hover:text-white border border-white/10 transition-colors cursor-pointer"
                  >
                    <Edit2 class="w-3.5 h-3.5" />
                    <span>Редагувати</span>
                  </button>
                  <button
                    type="button"
                    onclick={() => handleDeleteCollection(activeCustomCollection!)}
                    class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-red-500/10 hover:bg-red-500/20 text-xs font-bold text-red-400 hover:text-red-300 border border-red-500/30 transition-colors cursor-pointer"
                  >
                    <Trash2 class="w-3.5 h-3.5" />
                    <span>Видалити колекцію</span>
                  </button>
                </div>
              </div>
            {/if}

            {#if filteredItems.length > 0}
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
                    class="group flex flex-col sm:flex-row items-start sm:items-center gap-3 sm:gap-6 p-3 rounded-2xl bg-[#061820]/95 hover:bg-[#092530] border border-cyan-500/20 hover:border-cyan-400/60 shadow-lg transition-all duration-300 cursor-pointer"
                  >
                    <img
                      src={g.headerImageUrl || g.coverImageUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600'}
                      alt={g.title}
                      class="w-full sm:w-44 h-32 sm:h-20 rounded-xl object-cover shrink-0"
                    />

                    <div class="flex-1 min-w-0 w-full flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                      <div class="flex items-center justify-between sm:justify-start gap-3 min-w-0">
                        <h4 class="text-base font-bold text-white truncate">{g.title}</h4>
                        <button
                          onclick={(e) => { e.stopPropagation(); uiStore.addToast({ title: 'Завантаження', message: `Завантаження '${g.title}' розпочато.`, type: 'success' }); }}
                          class="px-4 py-1.5 rounded-full bg-cyan-500 hover:bg-cyan-400 text-black font-bold text-xs transition-all cursor-pointer shrink-0"
                        >
                          Скачати
                        </button>
                      </div>

                      <div class="flex items-center justify-between sm:justify-end gap-4 shrink-0 pt-2 sm:pt-0 border-t sm:border-t-0 border-cyan-950/40 w-full sm:w-auto">
                        <div class="flex sm:flex-col items-center sm:items-end justify-between text-xs text-slate-400 gap-2 sm:gap-0">
                          <span>Час у грі</span>
                          <strong class="text-white text-xs sm:text-sm">{formatPlayTime(item.playTimeMinutes)}</strong>
                        </div>

                        <div class="flex items-center gap-2 shrink-0">
                          <button
                            onclick={(e) => handleToggleFavorite(e, item.gameId)}
                            class="w-8 h-8 sm:w-9 sm:h-9 rounded-full flex items-center justify-center transition-colors cursor-pointer
                              {item.isFavorite ? 'bg-cyan-500/20 text-cyan-300' : 'bg-[#112833] text-slate-300 hover:text-cyan-300'}"
                            title={item.isFavorite ? 'Видалити з обраного' : 'Додати до обраного'}
                          >
                            <Heart class="w-4 h-4 {item.isFavorite ? 'fill-cyan-300' : ''}" />
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                {/each}
              </div>
            {/if}
          {:else if $libraryStore.activeCollectionId === 'favorites'}
            <div class="text-center py-20 bg-[#061820]/60 rounded-3xl border border-cyan-500/20 shadow-xl space-y-3">
              <div class="w-16 h-16 rounded-2xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center text-cyan-400 mx-auto">
                <Heart class="w-8 h-8 text-cyan-400/70" />
              </div>
              <h4 class="text-lg font-bold text-white">Немає обраних ігор</h4>
              <p class="text-xs text-slate-400 max-w-sm mx-auto">
                Позначайте улюблені ігри сердечком ♥, щоб швидко знаходити їх тут.
              </p>
            </div>
          {:else if $libraryStore.activeCollectionId !== 'all'}
            <div class="text-center py-20 bg-[#061820]/60 rounded-3xl border border-cyan-500/20 shadow-xl space-y-3">
              <div class="w-16 h-16 rounded-2xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center text-cyan-400 mx-auto">
                <Folder class="w-8 h-8 text-cyan-400/70" />
              </div>
              <h4 class="text-lg font-bold text-white">Колекція порожня</h4>
              <p class="text-xs text-slate-400 max-w-sm mx-auto">
                У цій колекції поки немає ігор. Натисніть іконку олівця біля вкладки, щоб додати ігри.
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
        {/if}
      </section>
    {/if}
  {/if}
    </div>
  </main>
</div>

{#if showCollectionModal}
  <CreateCollectionModal
    collection={editingCollection}
    onClose={() => { showCollectionModal = false; editingCollection = null; }}
  />
{/if}
