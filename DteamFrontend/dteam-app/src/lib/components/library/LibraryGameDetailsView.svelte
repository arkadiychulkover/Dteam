<script lang="ts">
  import { onMount } from 'svelte';
  import type { Game, UserGame } from '../../types';
  import type { FriendDto } from '../../types/friend';
  import { libraryStore } from '../../stores/libraryStore';
  import { friendsStore } from '../../stores/friendsStore';
  import { uiStore } from '../../stores/uiStore';
  import { profileStore } from '../../stores/profileStore';
  import { currentUser } from '../../stores/authStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { userService, type PublicProfile } from '../../services/userService';
  import { communityService, type CommunityPost } from '../../services/communityService';
  import { gamesService } from '../../services/gamesService';
  import type { Review } from '../../types';
  import { formatBytes, formatDate, formatPrice } from '../../utils/formatters';
  import { Star, Info, MoreHorizontal, ThumbsUp, MessageSquare, Share2, Loader2, Users, Heart, ChevronRight } from 'lucide-svelte';

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

  let allReviews = $state<Review[]>([]);
  let isLoadingReviews = $state(false);
  const myReviews = $derived(
    $currentUser ? allReviews.filter((r) => r.userId === $currentUser!.id) : []
  );

  let dlcs = $state<Game[]>([]);
  let isLoadingDlcs = $state(false);

  async function loadReviews(gameId: string) {
    isLoadingReviews = true;
    try {
      allReviews = await gamesService.getReviews(gameId);
    } catch (e) {
      console.warn('[LibraryGameDetails] Не вдалося завантажити рецензії:', e);
      allReviews = [];
    } finally {
      isLoadingReviews = false;
    }
  }

  async function loadDlcs(gameId: string) {
    isLoadingDlcs = true;
    try {
      dlcs = await gamesService.getDlcs(gameId);
    } catch (e) {
      console.warn('[LibraryGameDetails] Не вдалося завантажити DLC:', e);
      dlcs = [];
    } finally {
      isLoadingDlcs = false;
    }
  }

  function formatReviewDate(dateStr: string): string {
    return formatDate(dateStr);
  }

  function openDlc(dlc: Game) {
    gamesStore.selectGame(dlc);
    uiStore.setTab('game');
  }

  async function loadNews() {
    isLoadingNews = true;
    try {
      const res = await communityService.getPosts(game.id, 'news');
      gameNews = res.posts.slice(0, 3);
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

  $effect(() => {
    if (game?.id) {
      loadReviews(game.id);
      loadDlcs(game.id);
    }
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
    {#if activeTab === 'store'}
    <div class="lg:col-span-8 space-y-6">
      <section class="space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-extrabold text-white">Моя рецензія</h2>
          <button
            onclick={() => uiStore.setTab('game')}
            class="px-4 py-2 rounded-xl bg-transparent border border-cyan-400/60 text-cyan-300 hover:bg-cyan-500/10 text-xs font-bold transition-all cursor-pointer"
          >
            Написати рецензію
          </button>
        </div>

        {#if isLoadingReviews}
          <div class="flex items-center justify-center py-10 text-slate-400 text-xs gap-2">
            <Loader2 class="w-4 h-4 animate-spin text-cyan-400" />
            <span>Завантаження рецензій...</span>
          </div>
        {:else if myReviews.length > 0}
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            {#each myReviews as review}
              <div class="bg-[#061820]/90 border border-cyan-500/25 rounded-3xl p-6 shadow-xl flex flex-col justify-between space-y-4 group hover:border-cyan-400/60 transition-all">
                <div class="space-y-3">

                  <div class="flex items-center justify-between">
                    <div class="flex items-center gap-3">
                      <div class="w-10 h-10 rounded-full bg-[#0a232c] border border-cyan-400/40 flex items-center justify-center text-cyan-300 font-bold overflow-hidden">
                        {#if review.userAvatarUrl || review.user?.avatarUrl}
                          <img
                            src={review.userAvatarUrl || review.user?.avatarUrl || ''}
                            alt={review.username || review.user?.username || 'User'}
                            class="w-full h-full object-cover"
                          />
                        {:else}
                          {(review.username || review.user?.username || 'U').charAt(0).toUpperCase()}
                        {/if}
                      </div>
                      <div>
                        <h4 class="text-sm font-extrabold text-white">
                          {review.username || review.user?.username || 'Користувач'}
                        </h4>
                        <div class="flex items-center gap-0.5 text-rose-500 mt-0.5">
                          {#each Array(review.rating) as _}
                            <Star class="w-3.5 h-3.5 fill-rose-500 text-rose-500" />
                          {/each}
                        </div>
                      </div>
                    </div>

                    <span class="text-[11px] text-slate-500 font-mono">
                      {formatReviewDate(review.createdAt)}
                    </span>
                  </div>

                  <p class="text-xs text-slate-300 leading-relaxed whitespace-pre-wrap">
                    {review.content}
                  </p>
                </div>

                <div class="flex items-center justify-between pt-3 border-t border-cyan-950/80 text-xs text-slate-400">
                  <span class="text-[11px] font-semibold {review.isRecommended ? 'text-emerald-400' : 'text-slate-500'}">
                    {review.isRecommended ? '✓ Рекомендує гру' : 'Не рекомендує'}
                  </span>
                </div>
              </div>
            {/each}
          </div>
        {:else}
          <div class="text-center py-10 bg-[#061820]/60 rounded-2xl border border-cyan-500/20">
            <p class="text-xs text-slate-400">Ви ще не залишали рецензію на цю гру.</p>
          </div>
        {/if}
      </section>

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
            <article class="bg-[#061820]/90 border border-cyan-500/20 rounded-2xl overflow-hidden shadow-lg mb-4">
              {#if post.media?.url}
                {#if post.media.type === 'video'}
                  <video src={post.media.url} class="w-full max-h-72" controls></video>
                {:else}
                  <img src={post.media.url} alt="" class="w-full max-h-72 object-cover" />
                {/if}
              {:else if post.gameBannerUrl}
                <img src={post.gameBannerUrl} alt="" class="w-full max-h-56 object-cover opacity-80" />
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
    {:else if activeTab === 'dlc'}
    <div class="lg:col-span-8 space-y-6">
      <h2 class="text-lg font-extrabold text-white">Доповнення до гри</h2>

      {#if isLoadingDlcs}
        <div class="flex items-center justify-center py-10 text-slate-400 text-xs gap-2">
          <Loader2 class="w-4 h-4 animate-spin text-cyan-400" />
          <span>Завантаження DLC...</span>
        </div>
      {:else if dlcs.length > 0}
        <div class="bg-[#061820]/90 border border-cyan-500/25 rounded-3xl p-4 sm:p-6 shadow-xl space-y-3">
          {#each dlcs as dlc}
            <div
              role="button"
              tabindex="0"
              onclick={() => openDlc(dlc)}
              onkeydown={(e) => e.key === 'Enter' && openDlc(dlc)}
              class="group flex items-center justify-between p-3.5 rounded-2xl bg-[#08222d] hover:bg-[#0c3140] border border-cyan-500/15 hover:border-cyan-400/60 transition-all cursor-pointer shadow-md"
            >
              <div class="flex items-center gap-3 min-w-0">
                {#if dlc.coverImageUrl || dlc.headerImageUrl}
                  <img
                    src={dlc.coverImageUrl || dlc.headerImageUrl}
                    alt={dlc.title}
                    class="w-12 h-7 rounded-lg object-cover border border-cyan-500/20 shrink-0 group-hover:scale-105 transition-transform"
                  />
                {/if}
                <span class="text-sm font-bold text-white group-hover:text-cyan-300 transition-colors truncate">
                  {dlc.title}
                </span>
              </div>
              <div class="flex items-center gap-3 shrink-0">
                <span class="text-xs font-bold {Number(dlc.priceInNanoTons) === 0 ? 'text-emerald-400' : 'text-cyan-300 font-mono'}">
                  {formatPrice(dlc.priceInNanoTons, dlc.discountPercentage)}
                </span>
                <ChevronRight class="w-4 h-4 text-slate-500 group-hover:text-cyan-400 group-hover:translate-x-0.5 transition-all" />
              </div>
            </div>
          {/each}
        </div>
      {:else}
        <div class="text-center py-10 bg-[#061820]/60 rounded-2xl border border-cyan-500/20">
          <p class="text-xs text-slate-400">Для цієї гри немає доповнень.</p>
        </div>
      {/if}
    </div>
    {/if}

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