<script lang="ts">
  import { onMount } from 'svelte';
  import type { Game, UserGame } from '../../types';
  import type { FriendDto } from '../../types/friend';
  import { libraryStore } from '../../stores/libraryStore';
  import { friendsStore } from '../../stores/friendsStore';
  import { uiStore } from '../../stores/uiStore';
  import { profileStore } from '../../stores/profileStore';
  import { userService, type PublicProfile } from '../../services/userService';
  import { communityService, type CommunityPost } from '../../services/communityService';
  import { formatBytes, formatDate } from '../../utils/formatters';
  import { Star, Info, MoreHorizontal, ThumbsUp, MessageSquare, Share2, Loader2, Users, Heart } from 'lucide-svelte';

  interface Props {
    game: Game;
    onBack: () => void;
  }

  let { game, onBack }: Props = $props();

  const libraryItem = $derived(
    $libraryStore.items.find((i) => i.gameId === game.id) as UserGame | undefined
  );
  const isFavorite = $derived(libraryItem?.isFavorite ?? false);

  let activeTab = $state<'store' | 'dlc' | 'developer' | 'community'>('store');

  let gameNews = $state<CommunityPost[]>([]);
  let isLoadingNews = $state(false);

  let communityFeed = $state<CommunityPost[]>([]);
  let isLoadingCommunity = $state(false);

  let friendsWithGame = $state<{ friend: FriendDto; profile: PublicProfile }[]>([]);
  let isLoadingFriends = $state(false);

  async function loadNews() {
    isLoadingNews = true;
    try {
      const res = await communityService.getPosts(game.id, 'news');
      gameNews = res.posts.slice(0, 1);
    } catch (e) {
      console.warn('[LibraryGameDetails] Не вдалося завантажити новини гри:', e);
      gameNews = [];
    } finally {
      isLoadingNews = false;
    }
  }

  async function loadCommunityFeed() {
    isLoadingCommunity = true;
    try {
      const res = await communityService.getPosts(game.id, 'all');
      communityFeed = res.posts.slice(0, 4);
    } catch (e) {
      console.warn('[LibraryGameDetails] Не вдалося завантажити стрічку спільноти:', e);
      communityFeed = [];
    } finally {
      isLoadingCommunity = false;
    }
  }

  async function loadFriendsWithGame() {
    const allFriends = $friendsStore.friends;
    if (allFriends.length === 0) {
      friendsWithGame = [];
      return;
    }

    isLoadingFriends = true;
    try {
      const results = await Promise.all(
        allFriends.map(async (friend) => {
          try {
            const profile = await userService.getPublicProfile(friend.id);
            return { friend, profile };
          } catch {
            return null;
          }
        })
      );

      friendsWithGame = results.filter(
        (r): r is { friend: FriendDto; profile: PublicProfile } =>
          !!r && r.profile.libraryGames.some((g) => g.id === game.id)
      );
    } catch (e) {
      console.warn('[LibraryGameDetails] Не вдалося перевірити бібліотеки друзів:', e);
      friendsWithGame = [];
    } finally {
      isLoadingFriends = false;
    }
  }

  function handleDownload() {
    uiStore.addToast({
      title: 'Завантаження',
      message: `Завантаження '${game.title}' розпочато.`,
      type: 'success',
    });
  }

  function handleToggleFavorite() {
    libraryStore.toggleFavorite(game.id);
  }

  function handleShare(post: CommunityPost) {
    uiStore.addToast({
      title: 'Поділитись',
      message: 'Посилання на пост скопійовано.',
      type: 'info',
    });
  }

  async function handleToggleLike(post: CommunityPost) {
    post.stats.isLiked = !post.stats.isLiked;
    post.stats.likesCount += post.stats.isLiked ? 1 : -1;
    try {
      await communityService.toggleLikePost(post.id);
    } catch (e) {
      post.stats.isLiked = !post.stats.isLiked;
      post.stats.likesCount += post.stats.isLiked ? 1 : -1;
    }
  }

  onMount(() => {
    loadNews();
    loadCommunityFeed();
    loadFriendsWithGame();
  });
</script>

<div class="space-y-6 animate-in fade-in duration-300">
  <div class="relative w-full aspect-[16/6] min-h-[280px] rounded-3xl overflow-hidden bg-slate-950 border border-cyan-500/20 shadow-2xl shadow-cyan-950/40">
    <img
      src={game.headerImageUrl || game.coverImageUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1600&auto=format&fit=crop&q=80'}
      alt={game.title}
      class="w-full h-full object-cover"
    />
    <div class="absolute inset-0 bg-gradient-to-t from-[#030d12] via-[#030d12]/10 to-transparent"></div>

    <div class="absolute inset-x-0 bottom-0 p-5 sm:p-6 space-y-4">
      <button
        onclick={onBack}
        class="inline-flex items-center gap-2 text-xl sm:text-2xl font-black text-white font-display tracking-wide hover:text-cyan-300 transition-colors cursor-pointer"
      >
        <span>←</span>
        <span>{game.title}</span>
      </button>

      <div class="flex flex-wrap items-center gap-4">
        <button
          onclick={handleDownload}
          class="px-6 py-2.5 rounded-2xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-sm shadow-lg shadow-cyan-500/25 transition-all cursor-pointer"
        >
          Скачати
        </button>

        <div class="flex flex-col text-xs text-slate-300">
          <span class="text-slate-400">Розмір на диску</span>
          <strong class="text-white text-sm">{formatBytes(game.sizeInBytes)}</strong>
        </div>

        <div class="flex items-center gap-2 ml-auto">
          <button
            onclick={handleToggleFavorite}
            title={isFavorite ? 'Видалити з обраного' : 'Додати до обраного'}
            class="w-10 h-10 rounded-full flex items-center justify-center border transition-all cursor-pointer backdrop-blur-md
              {isFavorite
                ? 'bg-cyan-500/20 border-cyan-400/60 text-cyan-300'
                : 'bg-black/50 border-white/20 text-slate-300 hover:text-cyan-300 hover:border-cyan-400/60'}"
          >
            <Star class="w-4 h-4 {isFavorite ? 'fill-cyan-300' : ''}" />
          </button>
          <button
            class="w-10 h-10 rounded-full flex items-center justify-center bg-black/50 border border-white/20 text-slate-300 hover:text-white transition-all cursor-pointer backdrop-blur-md"
            title="Про гру"
          >
            <Info class="w-4 h-4" />
          </button>
          <button
            class="w-10 h-10 rounded-full flex items-center justify-center bg-black/50 border border-white/20 text-slate-300 hover:text-white transition-all cursor-pointer backdrop-blur-md"
            title="Більше"
          >
            <MoreHorizontal class="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  </div>

  <nav class="flex items-center gap-6 border-b border-cyan-500/10">
    <button
      onclick={() => (activeTab = 'store')}
      class="pb-3 text-sm font-extrabold transition-colors cursor-pointer relative {activeTab === 'store' ? 'text-cyan-300' : 'text-slate-400 hover:text-white'}"
    >
      Сторінка крамниці
      {#if activeTab === 'store'}<span class="absolute -bottom-px left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>{/if}
    </button>
    <button
      onclick={() => (activeTab = 'dlc')}
      class="pb-3 text-sm font-extrabold transition-colors cursor-pointer relative {activeTab === 'dlc' ? 'text-cyan-300' : 'text-slate-400 hover:text-white'}"
    >
      DLC
      {#if activeTab === 'dlc'}<span class="absolute -bottom-px left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>{/if}
    </button>
  </nav>

  <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
    <div class="lg:col-span-8 space-y-6">
      <div class="bg-[#061820]/90 border border-cyan-500/20 rounded-2xl p-5 shadow-lg flex items-center justify-between">
        <span class="text-sm font-bold text-white">Моя рецензія</span>
        <button
          onclick={() => uiStore.setTab('game')}
          class="px-4 py-2 rounded-xl bg-transparent border border-cyan-400/60 text-cyan-300 hover:bg-cyan-500/10 text-xs font-bold transition-all cursor-pointer"
        >
          Написати рецензію
        </button>
      </div>

      <section class="space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-extrabold text-white">Що нового</h2>
          <button
            onclick={() => uiStore.addToast({ title: 'Новини', message: `Усі новини для '${game.title}' скоро зʼявляться.`, type: 'info' })}
            class="text-xs font-bold text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer"
          >
            Усі новини &gt;
          </button>
        </div>

        {#if isLoadingNews}
          <div class="flex items-center justify-center py-10 text-slate-400 text-xs gap-2">
            <Loader2 class="w-4 h-4 animate-spin text-cyan-400" />
            <span>Завантаження новин...</span>
          </div>
        {:else if gameNews.length > 0}
          {#each gameNews as post (post.id)}
            <article class="bg-[#061820]/90 border border-cyan-500/20 rounded-2xl overflow-hidden shadow-lg">
              {#if post.media?.type === 'image' && post.media.url}
                <img src={post.media.url} alt="" class="w-full max-h-72 object-cover" />
              {:else if post.media?.type === 'video' && post.media.url}
                <video src={post.media.url} class="w-full max-h-72" controls></video>
              {/if}

              <div class="p-5 space-y-3">
                <button
                  onclick={() => profileStore.viewProfile(post.author.id)}
                  class="flex items-center gap-2 group cursor-pointer"
                >
                  <img
                    src={post.author.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(post.author.username)}`}
                    alt={post.author.username}
                    class="w-7 h-7 rounded-full object-cover border border-cyan-500/30"
                  />
                  <span class="text-sm font-bold text-slate-200 group-hover:text-cyan-300 transition-colors">{post.author.username}</span>
                  <span class="text-[11px] text-slate-500">{formatDate(post.createdAt)}</span>
                </button>

                <h3 class="text-base font-bold text-white">{post.title}</h3>
                <p class="text-xs text-slate-400 leading-relaxed line-clamp-3">{post.content}</p>

                <div class="flex items-center gap-4 pt-2">
                  <button
                    onclick={() => handleToggleLike(post)}
                    class="flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-[#0a2733] hover:bg-[#0d3344] text-xs font-bold transition-colors cursor-pointer {post.stats.isLiked ? 'text-rose-400' : 'text-slate-300'}"
                  >
                    <ThumbsUp class="w-3.5 h-3.5 {post.stats.isLiked ? 'fill-rose-400' : ''}" />
                    {post.stats.likesCount}
                  </button>
                  <button class="flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-[#0a2733] hover:bg-[#0d3344] text-xs font-bold text-slate-300 transition-colors cursor-pointer">
                    <MessageSquare class="w-3.5 h-3.5" />
                    {post.stats.commentsCount}
                  </button>
                  <button
                    onclick={() => handleShare(post)}
                    class="flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-[#0a2733] hover:bg-[#0d3344] text-xs font-bold text-slate-300 transition-colors cursor-pointer ml-auto"
                  >
                    <Share2 class="w-3.5 h-3.5" />
                    Поділитись
                  </button>
                </div>
              </div>
            </article>
          {/each}
        {:else}
          <div class="text-center py-10 bg-[#061820]/60 rounded-2xl border border-cyan-500/20">
            <p class="text-xs text-slate-400">Новин для цієї гри поки немає.</p>
          </div>
        {/if}
      </section>

      <section class="space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-extrabold text-white">Цікаве від Спільноти</h2>
          <button
            onclick={() => { uiStore.setTab('community'); }}
            class="text-xs font-bold text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer"
          >
            Моя стрічка &gt;
          </button>
        </div>

        {#if isLoadingCommunity}
          <div class="flex items-center justify-center py-10 text-slate-400 text-xs gap-2">
            <Loader2 class="w-4 h-4 animate-spin text-cyan-400" />
            <span>Завантаження стрічки...</span>
          </div>
        {:else if communityFeed.length > 0}
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {#each communityFeed as post (post.id)}
              <article class="bg-[#061820]/90 border border-cyan-500/20 rounded-2xl overflow-hidden shadow-lg">
                <div class="p-4 pb-0 flex items-center gap-2">
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
                  <img src={post.media.url} alt="" class="w-full h-32 object-cover mt-3" />
                {:else if post.media?.type === 'video' && post.media.url}
                  <video src={post.media.url} class="w-full h-32 object-cover mt-3" muted></video>
                {/if}

                <div class="p-4 space-y-2">
                  {#if post.title}<h4 class="text-sm font-bold text-white line-clamp-1">{post.title}</h4>{/if}
                  <p class="text-[11px] text-slate-400 leading-relaxed line-clamp-2">{post.content}</p>
                  <div class="flex items-center gap-3 text-[11px] text-slate-500 pt-1">
                    <span class="flex items-center gap-1"><Heart class="w-3 h-3" />{post.stats.likesCount}</span>
                    <span class="flex items-center gap-1"><MessageSquare class="w-3 h-3" />{post.stats.commentsCount}</span>
                    <span class="ml-auto">{formatDate(post.createdAt)}</span>
                  </div>
                </div>
              </article>
            {/each}
          </div>
        {:else}
          <div class="text-center py-10 bg-[#061820]/60 rounded-2xl border border-cyan-500/20">
            <p class="text-xs text-slate-400">Постів спільноти для цієї гри поки немає.</p>
          </div>
        {/if}
      </section>
    </div>

    <aside class="lg:col-span-4 space-y-4">
      <div class="bg-[#061820]/90 border border-cyan-500/20 rounded-2xl p-5 shadow-lg">
        <h4 class="text-sm font-bold text-white mb-4 flex items-center gap-2">
          <Users class="w-4 h-4 text-cyan-400" />
          Друзів мають цю гру: {isLoadingFriends ? '…' : friendsWithGame.length}
        </h4>

        {#if isLoadingFriends}
          <div class="flex items-center justify-center py-6 text-slate-400 text-xs gap-2">
            <Loader2 class="w-4 h-4 animate-spin text-cyan-400" />
            <span>Перевірка бібліотек друзів...</span>
          </div>
        {:else if friendsWithGame.length > 0}
          <div class="flex flex-wrap gap-2">
            {#each friendsWithGame as { friend } (friend.id)}
              <button
                onclick={() => profileStore.viewProfile(friend.id)}
                class="flex items-center gap-2 bg-[#0a2733] hover:bg-[#0d3344] pr-3 pl-1 py-1 rounded-full text-xs font-semibold text-slate-200 transition-colors cursor-pointer"
              >
                {#if friend.avatarUrl}
                  <img src={friend.avatarUrl} alt={friend.username} class="w-6 h-6 rounded-full object-cover" />
                {:else}
                  <span class="w-6 h-6 rounded-full bg-cyan-500/20 border border-cyan-400/40 flex items-center justify-center text-[10px] font-bold text-cyan-300">
                    {friend.username.charAt(0).toUpperCase()}
                  </span>
                {/if}
                <span>{friend.username}</span>
              </button>
            {/each}
          </div>
        {:else}
          <p class="text-xs text-slate-400">Серед ваших друзів ніхто ще не має цієї гри.</p>
        {/if}
      </div>
    </aside>
  </div>
</div>