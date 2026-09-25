<script lang="ts">
  import { onMount } from 'svelte';
  import { libraryStore } from '../../stores/libraryStore';
  import { uiStore } from '../../stores/uiStore';
  import { profileStore } from '../../stores/profileStore';
  import { communityService, type CommunityPost } from '../../services/communityService';
  import MediaLightboxModal from '../ui/MediaLightboxModal.svelte';
  import { renderDecoratedText, resolveMediaUrl } from '../../utils/textDecorator';
  import {
    Rss,
    Heart,
    MessageSquare,
    Sparkles,
    Gamepad2,
    Calendar,
    Image,
    Video,
    Share2,
    CheckCircle2,
    Filter,
  } from 'lucide-svelte';

  let posts = $state<CommunityPost[]>([]);
  let isLoading = $state(true);
  let activeCategory = $state<'all' | 'news' | 'screenshots' | 'forum'>('all');
  let lightboxMedia = $state<{ src: string; title?: string; author?: { username: string; avatarUrl?: string } } | null>(null);

  const ownedGameIds = $derived(
    new Set($libraryStore.items.map((i) => i.gameId.toLowerCase()))
  );

  async function loadFeed() {
    isLoading = true;
    try {

      const res = await communityService.getPosts(null, activeCategory === 'all' ? 'all' : activeCategory);
      let allPosts = res.posts || [];

      allPosts.sort((a, b) => {
        const aOwned = a.gameId && ownedGameIds.has(a.gameId.toLowerCase()) ? 1 : 0;
        const bOwned = b.gameId && ownedGameIds.has(b.gameId.toLowerCase()) ? 1 : 0;
        if (aOwned !== bOwned) return bOwned - aOwned;
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      });

      posts = allPosts;
    } catch (err) {
      console.warn('[LibraryMyFeedView] Помилка завантаження стрічки:', err);
      posts = [];
    } finally {
      isLoading = false;
    }
  }

  onMount(() => {
    loadFeed();
  });

  function setCategory(cat: 'all' | 'news' | 'screenshots' | 'forum') {
    activeCategory = cat;
    loadFeed();
  }

  async function toggleLike(post: CommunityPost) {
    const prevLiked = post.stats.isLiked;
    const prevCount = post.stats.likesCount;
    post.stats.isLiked = !prevLiked;
    post.stats.likesCount += prevLiked ? -1 : 1;

    try {
      await communityService.toggleLikePost(post.id);
    } catch {
      post.stats.isLiked = prevLiked;
      post.stats.likesCount = prevCount;
    }
  }
</script>

<div class="space-y-6">

  <div class="p-6 rounded-2xl bg-gradient-to-r from-[#061e2a] via-[#04121a] to-[#061820] border border-cyan-500/20 shadow-xl flex flex-col sm:flex-row sm:items-center justify-between gap-4">
    <div class="space-y-1">
      <div class="flex items-center gap-2 text-cyan-400 text-xs font-bold uppercase tracking-wider">
        <Rss class="w-4 h-4 animate-pulse" />
        <span>Персоналізована стрічка</span>
      </div>
      <h2 class="text-xl font-black text-white">Моя стрічка оновлень</h2>
      <p class="text-xs text-slate-400">
        Усі патчі, новини, скриншоти та активності спільноти за іграми з вашої колекції ({$libraryStore.items.length} ігор).
      </p>
    </div>

    <div class="flex items-center gap-1.5 p-1 bg-black/40 rounded-xl border border-cyan-500/15 shrink-0 overflow-x-auto">
      <button
        type="button"
        onclick={() => setCategory('all')}
        class="px-3 py-1.5 rounded-lg text-xs font-bold transition-colors cursor-pointer {activeCategory === 'all' ? 'bg-cyan-500/20 text-cyan-300 border border-cyan-500/40' : 'text-slate-400 hover:text-white'}"
      >
        Усе
      </button>
      <button
        type="button"
        onclick={() => setCategory('news')}
        class="px-3 py-1.5 rounded-lg text-xs font-bold transition-colors cursor-pointer {activeCategory === 'news' ? 'bg-cyan-500/20 text-cyan-300 border border-cyan-500/40' : 'text-slate-400 hover:text-white'}"
      >
        Новини та патчі
      </button>
      <button
        type="button"
        onclick={() => setCategory('screenshots')}
        class="px-3 py-1.5 rounded-lg text-xs font-bold transition-colors cursor-pointer {activeCategory === 'screenshots' ? 'bg-cyan-500/20 text-cyan-300 border border-cyan-500/40' : 'text-slate-400 hover:text-white'}"
      >
        Скриншоти
      </button>
      <button
        type="button"
        onclick={() => setCategory('forum')}
        class="px-3 py-1.5 rounded-lg text-xs font-bold transition-colors cursor-pointer {activeCategory === 'forum' ? 'bg-cyan-500/20 text-cyan-300 border border-cyan-500/40' : 'text-slate-400 hover:text-white'}"
      >
        Обговорення
      </button>
    </div>
  </div>

  {#if isLoading}
    <div class="flex flex-col items-center justify-center py-24 text-slate-400 text-xs gap-3">
      <div class="w-8 h-8 rounded-full border-2 border-cyan-400 border-t-transparent animate-spin"></div>
      <span>Складання вашої персональної стрічки...</span>
    </div>
  {:else if posts.length === 0}
    <div class="text-center py-20 bg-[#061820]/60 rounded-2xl border border-cyan-500/20 space-y-3">
      <Gamepad2 class="w-10 h-10 text-cyan-500/40 mx-auto" />
      <h3 class="text-sm font-bold text-white">У вашій стрічці поки тихо</h3>
      <p class="text-xs text-slate-400 max-w-sm mx-auto">
        Додайте більше ігор до бібліотеки або приєднайтеся до активностей у Спільноті!
      </p>
    </div>
  {:else}
    <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
      {#each posts as post (post.id)}
        {@const isOwned = post.gameId && ownedGameIds.has(post.gameId.toLowerCase())}
        <article class="bg-[#061820]/90 border {isOwned ? 'border-cyan-500/40 shadow-cyan-950/20 shadow-lg' : 'border-cyan-500/20'} rounded-2xl overflow-hidden hover:border-cyan-400/60 transition-all duration-300 flex flex-col justify-between group">

          <div class="p-4 space-y-3">
            <div class="flex items-center justify-between gap-3">
              <button
                type="button"
                onclick={() => profileStore.viewProfile(post.author.id)}
                class="flex items-center gap-2.5 text-left group/author cursor-pointer"
              >
                <img
                  src={post.author.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(post.author.username)}`}
                  alt={post.author.username}
                  class="w-8 h-8 rounded-full object-cover border border-cyan-500/30 group-hover/author:border-cyan-400 transition-colors"
                />
                <div>
                  <span class="text-xs font-bold text-slate-200 group-hover/author:text-cyan-300 transition-colors block">
                    {post.author.username}
                  </span>
                  <span class="text-[10px] text-slate-500 block">
                    {new Date(post.createdAt).toLocaleDateString('uk-UA', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' })}
                  </span>
                </div>
              </button>

              <div class="flex items-center gap-2">
                {#if isOwned}
                  <span class="flex items-center gap-1 text-[10px] font-extrabold px-2 py-0.5 rounded-full bg-cyan-500/20 text-cyan-300 border border-cyan-500/30">
                    <CheckCircle2 class="w-3 h-3" />
                    У вашій бібліотеці
                  </span>
                {/if}
                {#if post.gameTitle}
                  <span class="text-[10px] font-bold px-2.5 py-0.5 rounded-full bg-slate-800/80 text-slate-300 border border-slate-700/60 truncate max-w-[140px]">
                    {post.gameTitle}
                  </span>
                {/if}
              </div>
            </div>

            {#if post.title}
              <h3 class="text-sm font-bold text-white group-hover:text-cyan-200 transition-colors">
                {post.title}
              </h3>
            {/if}
            <p class="text-xs text-slate-300 leading-relaxed whitespace-pre-line line-clamp-4">
              {@html renderDecoratedText(post.content)}
            </p>
          </div>

          {#if post.media?.url && post.media.type === 'image'}
            <div
              class="relative overflow-hidden cursor-pointer max-h-64 bg-black/40"
              role="button"
              tabindex="0"
              onclick={() => (lightboxMedia = { src: post.media.url, title: post.title, author: post.author })}
              onkeydown={(e) => e.key === 'Enter' && (lightboxMedia = { src: post.media.url, title: post.title, author: post.author })}
            >
              <img
                src={post.media.url}
                alt={post.title || 'Зображення'}
                class="w-full h-full object-cover group-hover:scale-102 transition-transform duration-300"
                loading="lazy"
              />
              <div class="absolute bottom-2 right-2 px-2 py-1 rounded-lg bg-black/60 backdrop-blur-md text-[10px] text-slate-300 flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                <Image class="w-3 h-3" />
                <span>Збільшити</span>
              </div>
            </div>
          {:else if post.media?.url && post.media.type === 'video'}
            <div class="relative max-h-64 bg-black">
              <video src={resolveMediaUrl(post.media.url)} controls playsinline class="w-full h-full object-contain"></video>
            </div>
          {/if}

          <div class="px-4 py-3 bg-black/20 border-t border-cyan-500/10 flex items-center justify-between text-xs">
            <div class="flex items-center gap-4">
              <button
                type="button"
                onclick={() => toggleLike(post)}
                class="flex items-center gap-1.5 text-xs font-semibold transition-colors cursor-pointer {post.stats.isLiked ? 'text-red-400' : 'text-slate-400 hover:text-red-300'}"
              >
                <Heart class="w-4 h-4 {post.stats.isLiked ? 'fill-red-400' : ''}" />
                <span>{post.stats.likesCount}</span>
              </button>

              <button
                type="button"
                onclick={() => uiStore.setTab('community')}
                class="flex items-center gap-1.5 text-xs font-semibold text-slate-400 hover:text-cyan-300 transition-colors cursor-pointer"
              >
                <MessageSquare class="w-4 h-4" />
                <span>{post.stats.commentsCount}</span>
              </button>
            </div>

            {#if post.gameId}
              <button
                type="button"
                onclick={() => {
                  if (isOwned) {
                    libraryStore.selectGame(post.gameId);
                    libraryStore.setActivePrimaryTab('games');
                  } else {
                    uiStore.openGameDetails(post.gameId);
                  }
                }}
                class="text-[11px] font-bold text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer"
              >
                {isOwned ? 'Грати в гру →' : 'У крамницю →'}
              </button>
            {/if}
          </div>
        </article>
      {/each}
    </div>
  {/if}
</div>

{#if lightboxMedia}
  <MediaLightboxModal
    src={lightboxMedia.src}
    title={lightboxMedia.title}
    author={lightboxMedia.author}
    onClose={() => (lightboxMedia = null)}
  />
{/if}
