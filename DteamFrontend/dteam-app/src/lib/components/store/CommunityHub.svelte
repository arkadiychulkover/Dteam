<script lang="ts">
  import { onMount } from 'svelte';
  import { communityService, type CommunityPost, type CommunityComment } from '../../services/communityService';
  import { currentUser } from '../../stores/authStore';
  import { uiStore } from '../../stores/uiStore';
  import { profileStore } from '../../stores/profileStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { 
    Users, Search, Filter, MessageSquare, ThumbsUp, Share2, 
    Play, Plus, ArrowLeft, X, Send, CornerDownRight, Loader2,
    BookOpen, Newspaper, Image, Film, MessageCircle, Bell, MoreHorizontal
  } from 'lucide-svelte';

  const categoryLabels = {
    all: 'Усі розділи',
    forum: 'Форум',
    screenshots: 'Скріншоти',
    videos: 'Відео',
    guides: 'Гайди',
    news: 'Новини'
  };

  // Props
  let { gameId } = $props<{ gameId: string }>();

  // State
  let posts = $state<CommunityPost[]>([]);
  let otherPosts = $derived(posts.filter(p => p.id !== selectedPostId).slice(0, 5));
  
  let gameTitle = $state('');
  let subscribersCount = $state(0);
  let onlineCount = $state(0);

  let activeCategory = $state<'all' | 'forum' | 'screenshots' | 'videos' | 'guides' | 'news'>('all');
  let searchQuery = $state('');
  let sortBy = $state<'newest' | 'popular' | 'rating'>('newest');
  let searchPlaceholder = $derived(
    activeCategory === 'all' ? 'Пошук: Усі розділи' : `Пошук: ${categoryLabels[activeCategory]}`
  );
  
  let isLoading = $state(false);
  let isCreateModalOpen = $state(false);

  // Detail View State
  let selectedPostId = $state<string | null>(null);
  let selectedPost = $state<CommunityPost | null>(null);
  let comments = $state<CommunityComment[]>([]);
  let isLoadingPostDetails = $state(false);
  let commentSortBy = $state<'newest' | 'oldest' | 'popular'>('newest');
  let newCommentText = $state('');
  let replyText = $state<Record<string, string>>({});
  let activeReplyCommentId = $state<string | null>(null);

  // Creation State
  let targetGameId = $state(gameId || '');
  let newPostCategory = $state<'forum' | 'screenshots' | 'videos' | 'guides'>('forum');
  let newPostTitle = $state('');
  let newPostContent = $state('');
  let newPostMediaType = $state<'image' | 'video' | 'none'>('none');
  let newPostMediaUrl = $state('');
  let isSubmittingPost = $state(false);

  $effect(() => {
    if (gameId) targetGameId = gameId;
  });

  onMount(() => {
    gamesStore.loadGames();
  });

  // Fetch posts
  async function loadPosts() {
    isLoading = true;
    try {
      const res = await communityService.getPosts(gameId, activeCategory, searchQuery, sortBy);
      posts = res.posts;
      gameTitle = res.gameTitle;
      subscribersCount = res.subscribersCount;
      onlineCount = res.onlineCount;
    } catch (e) {
      console.warn('[CommunityHub] Failed to fetch posts:', e);
    } finally {
      isLoading = false;
    }
  }

  // Fetch post details & comments
  async function loadPostDetails(postId: string) {
    isLoadingPostDetails = true;
    try {
      const res = await communityService.getPostDetails(postId);
      selectedPost = res.post;
      comments = res.comments;
    } catch (e) {
      console.warn('[CommunityHub] Failed to fetch post details:', e);
    } finally {
      isLoadingPostDetails = false;
    }
  }

  // React to tab/sort/search changes
  $effect(() => {
    // Depend on filter state so this re-runs when they change
    const _cat = activeCategory;
    const _sort = sortBy;
    const _search = searchQuery;
    if (!selectedPostId) {
      loadPosts();
    }
  });

  // Handle post click
  function handleSelectPost(postId: string) {
    selectedPostId = postId;
    loadPostDetails(postId);
  }

  function handleBackToList() {
    selectedPostId = null;
    selectedPost = null;
    comments = [];
    loadPosts();
  }

  // Post Actions
  async function handleToggleLikePost(postId: string, isDetail = false) {
    if (!$currentUser) {
      uiStore.addToast({ title: 'Потрібна авторизація', message: 'Будь ласка, увійдіть в акаунт, щоб ставити вподобайки.', type: 'warning' });
      uiStore.setLoginModal(true);
      return;
    }

    try {
      const res = await communityService.toggleLikePost(postId);
      
      // Update local state
      posts = posts.map(p => p.id === postId ? { ...p, stats: { ...p.stats, likesCount: res.likesCount, isLiked: res.liked } } : p);
      if (isDetail && selectedPost && selectedPost.id === postId) {
        selectedPost = { ...selectedPost, stats: { ...selectedPost.stats, likesCount: res.likesCount, isLiked: res.liked } };
      }
    } catch (e) {
      console.warn('[CommunityHub] Like action failed:', e);
    }
  }

  function handleSharePost(postId: string) {
    if (typeof navigator !== 'undefined' && navigator.clipboard) {
      navigator.clipboard.writeText(`${window.location.origin}/post/${postId}`);
      uiStore.addToast({
        title: 'Посилання скопійовано',
        message: 'Посилання на публікацію збережено в буфер обміну.',
        type: 'success'
      });
    }
  }

  // Create Post Submit
  async function handleCreatePostSubmit(e: SubmitEvent) {
    e.preventDefault();
    if (!newPostTitle.trim() && newPostCategory !== 'screenshots') {
      uiStore.addToast({ title: 'Помилка', message: 'Заголовок обов’язковий для цієї категорії.', type: 'warning' });
      return;
    }
    if (!newPostContent.trim()) {
      uiStore.addToast({ title: 'Помилка', message: 'Вміст публікації не може бути порожнім.', type: 'warning' });
      return;
    }

    isSubmittingPost = true;
    try {
      await communityService.createPost(targetGameId || gameId, {
        category: newPostCategory,
        title: newPostTitle,
        content: newPostContent,
        mediaType: newPostMediaType,
        mediaUrl: newPostMediaUrl
      });

      uiStore.addToast({
        title: 'Опубліковано! 🎉',
        message: 'Ваш новий пост успішно додано до спільноти.',
        type: 'success'
      });

      // Clear form
      newPostTitle = '';
      newPostContent = '';
      newPostMediaType = 'none';
      newPostMediaUrl = '';
      isCreateModalOpen = false;

      // Reload list
      loadPosts();
    } catch (e: any) {
      uiStore.addToast({ title: 'Помилка публікації', message: e?.message || 'Не вдалося створити пост.', type: 'error' });
    } finally {
      isSubmittingPost = false;
    }
  }

  // Add Comment
  async function handleAddCommentSubmit() {
    if (!$currentUser) {
      uiStore.addToast({ title: 'Потрібна авторизація', message: 'Будь ласка, увійдіть в акаунт, щоб залишати коментарі.', type: 'warning' });
      uiStore.setLoginModal(true);
      return;
    }
    if (!newCommentText.trim() || !selectedPostId) return;

    try {
      const newComment = await communityService.addComment(selectedPostId, newCommentText.trim());
      comments = [
        {
          ...newComment,
          createdAt: 'Щойно',
          replies: []
        },
        ...comments
      ];
      newCommentText = '';
      uiStore.addToast({ title: 'Коментар додано', message: 'Ваш коментар успішно опубліковано.', type: 'success' });
    } catch (e: any) {
      uiStore.addToast({ title: 'Помилка', message: e?.message || 'Не вдалося надіслати коментар.', type: 'error' });
    }
  }

  // Add Nested Reply
  async function handleAddReplySubmit(commentId: string) {
    if (!$currentUser) {
      uiStore.addToast({ title: 'Потрібна авторизація', message: 'Будь ласка, увійдіть в акаунт, щоб залишати відповіді.', type: 'warning' });
      uiStore.setLoginModal(true);
      return;
    }
    const text = replyText[commentId];
    if (!text || !text.trim()) return;

    try {
      const newReply = await communityService.addReply(commentId, text.trim());
      comments = comments.map(c => {
        if (c.id === commentId) {
          return {
            ...c,
            replies: [
              ...c.replies,
              {
                id: newReply.id,
                author: newReply.author,
                createdAt: 'Щойно',
                content: text.trim()
              }
            ]
          };
        }
        return c;
      });
      replyText[commentId] = '';
      activeReplyCommentId = null;
    } catch (e: any) {
      uiStore.addToast({ title: 'Помилка', message: e?.message || 'Не вдалося надіслати відповідь.', type: 'error' });
    }
  }

  // Category Icon Match
  function getCategoryIcon(cat: string) {
    switch (cat) {
      case 'forum': return MessageCircle;
      case 'screenshots': return Image;
      case 'videos': return Film;
      case 'guides': return BookOpen;
      case 'news': return Newspaper;
      default: return MessageSquare;
    }
  }

  // Sorted Comments based on selection
  const sortedComments = $derived.by(() => {
    const list = [...comments];
    if (commentSortBy === 'oldest') {
      return list.reverse();
    }
    return list;
  });
</script>

<!-- Subheader metrics -->
<div class="p-5 sm:p-6 rounded-3xl bg-[#092635] border border-cyan-500/25 shadow-xl flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
  <div class="space-y-1">
    <span class="text-[10px] uppercase font-black tracking-widest text-cyan-400">Спільнота гри</span>
    <h2 class="text-xl sm:text-2xl font-black text-white font-display">{gameTitle || 'Завантаження...'}</h2>
    <div class="flex items-center gap-4 text-xs pt-1">
      <span class="text-slate-300 flex items-center">
        <span class="inline-block w-2 h-2 rounded-full bg-pink-500 animate-pulse mr-1.5 shadow-[0_0_8px_#ec4899]"></span>
        {subscribersCount.toLocaleString()} підписників
      </span>
      <span class="text-slate-300 flex items-center">
        <span class="inline-block w-2 h-2 rounded-full bg-cyan-400 animate-pulse mr-1.5 shadow-[0_0_8px_#22d3ee]"></span>
        {onlineCount.toLocaleString()} онлайн
      </span>
    </div>
  </div>
  
  <!-- Header Controls -->
  <div class="flex items-center gap-2.5 w-full md:w-auto justify-end">
    <button
      onclick={() => {
        if (!$currentUser) {
          uiStore.addToast({ title: 'Потрібна авторизація', message: 'Будь ласка, увійдіть в акаунт, щоб створювати публікації.', type: 'warning' });
          uiStore.setLoginModal(true);
        } else {
          isCreateModalOpen = true;
        }
      }}
      class="px-4 py-2.5 rounded-xl bg-[#22d3ee] hover:bg-cyan-300 text-black font-black text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all flex items-center gap-1.5 cursor-pointer shrink-0"
    >
      <Plus class="w-4 h-4" />
      <span>Створити пост</span>
    </button>
    
    <button class="p-2.5 rounded-xl bg-[#041219] hover:bg-slate-900 border border-cyan-500/20 text-slate-300 hover:text-white transition-colors cursor-pointer" title="Сповіщення">
      <Bell class="w-4 h-4" />
    </button>
    
    <button class="p-2.5 rounded-xl bg-[#041219] hover:bg-slate-900 border border-cyan-500/20 text-slate-300 hover:text-white transition-colors cursor-pointer" title="Більше">
      <MoreHorizontal class="w-4 h-4" />
    </button>
  </div>
</div>

{#if !selectedPostId}
  <!-- LIST VIEW -->
  <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start pt-2">
    
    <!-- Left Column: Feed -->
    <div class="lg:col-span-8 space-y-6">

      <!-- Feed Grid -->
      {#if isLoading}
        <div class="flex items-center justify-center py-24">
          <Loader2 class="w-10 h-10 text-cyan-400 animate-spin" />
        </div>
      {:else if posts.length > 0}
        <div class="space-y-4">
          {#each posts as post (post.id)}
            {@const Icon = getCategoryIcon(post.category)}
            <div 
              role="button"
              tabindex="0"
              onclick={() => handleSelectPost(post.id)}
              onkeydown={(e) => e.key === 'Enter' && handleSelectPost(post.id)}
              class="p-5 rounded-3xl bg-[#092635] border border-cyan-500/20 hover:border-cyan-500/50 shadow-lg transition-all text-left cursor-pointer group space-y-4"
            >
              <!-- Author header -->
              <div class="flex items-center justify-between">
                <button
                  type="button"
                  onclick={(e) => { e.stopPropagation(); profileStore.viewProfile(post.author.id); }}
                  class="flex items-center gap-3 cursor-pointer text-left"
                >
                  <div class="w-8 h-8 rounded-full bg-[#041219] border border-cyan-500/30 overflow-hidden flex items-center justify-center text-cyan-300 font-bold text-xs">
                    {#if post.author.avatarUrl}
                      <img src={post.author.avatarUrl} alt={post.author.username} class="w-full h-full object-cover" />
                    {:else}
                      {post.author.username.charAt(0).toUpperCase()}
                    {/if}
                  </div>
                  <div>
                    <span class="block text-xs font-bold text-white hover:text-cyan-300 transition-colors">
                      {post.author.username}
                    </span>
                    <span class="block text-[9px] text-slate-500">{post.createdAt}</span>
                  </div>
                </button>

                <span class="px-2.5 py-0.5 rounded-lg bg-[#041219] text-cyan-300 font-bold text-[9px] border border-cyan-500/25 uppercase flex items-center gap-1">
                  <Icon class="w-3 h-3" />
                  {categoryLabels[post.category]}
                </span>
              </div>

              <!-- Content Layout mapping specifically per Category specs -->
              {#if post.category === 'guides'}
                <!-- Guides layout: wide horizontal thumbnail preview on left + title/description on right -->
                <div class="flex flex-col sm:flex-row gap-4 items-stretch">
                  {#if post.media.url}
                    <div class="w-full sm:w-44 h-24 rounded-2xl overflow-hidden shrink-0 border border-cyan-500/25">
                      <img src={post.media.url} alt={post.title} class="w-full h-full object-cover" />
                    </div>
                  {/if}
                  <div class="space-y-2 flex-1 min-w-0">
                    <h3 class="text-base font-black text-white leading-snug group-hover:text-cyan-400 transition-colors">
                      {post.title}
                    </h3>
                    <p class="text-xs text-slate-400 line-clamp-2 leading-relaxed">
                      {post.content}
                    </p>
                  </div>
                </div>
              {:else if post.category === 'news'}
                <!-- News layout: Prominent big banner, header/title and rich text layout -->
                <div class="space-y-3">
                  {#if post.media.url}
                    <div class="w-full aspect-[21/9] rounded-2xl overflow-hidden border border-cyan-500/20 shadow-inner">
                      <img src={post.media.url} alt={post.title} class="w-full h-full object-cover" />
                    </div>
                  {/if}
                  <h3 class="text-lg font-black text-white group-hover:text-cyan-400 transition-colors">
                    {post.title}
                  </h3>
                  <p class="text-xs text-slate-400 line-clamp-3 leading-relaxed">
                    {post.content}
                  </p>
                </div>
              {:else}
                <!-- Default (Forum, Screenshot, Video) -->
                <div class="space-y-3">
                  {#if post.title}
                    <h3 class="text-base font-black text-white group-hover:text-cyan-400 transition-colors">
                      {post.title}
                    </h3>
                  {/if}
                  <p class="text-xs text-slate-300 line-clamp-3 leading-relaxed whitespace-pre-wrap">
                    {post.content}
                  </p>

                  <!-- Media Display -->
                  {#if post.media.type === 'image' && post.media.url}
                    <div class="w-full max-h-96 rounded-2xl overflow-hidden border border-cyan-500/20">
                      <img src={post.media.url} alt={post.title} class="w-full h-full object-cover" />
                    </div>
                  {:else if post.media.type === 'video' && post.media.url}
                    <div class="relative w-full aspect-video rounded-2xl overflow-hidden bg-slate-950 border border-cyan-500/20">
                      <img 
                        src={post.media.thumbnailUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=800'} 
                        alt={post.title} 
                        class="w-full h-full object-cover opacity-70" 
                      />
                      <div class="absolute inset-0 flex items-center justify-center">
                        <div class="w-14 h-14 rounded-full bg-cyan-400 hover:bg-cyan-300 text-black flex items-center justify-center shadow-lg shadow-cyan-400/40 transition-all">
                          <Play class="w-6 h-6 fill-black text-black ml-1" />
                        </div>
                      </div>
                    </div>
                  {/if}
                </div>
              {/if}

              <!-- Footer Statistics interaction bar -->
              <div class="flex items-center justify-between pt-3 border-t border-cyan-950/60 text-[11px] font-semibold text-slate-400">
                <div class="flex items-center gap-4">
                  <button
                    type="button"
                    onclick={(e) => { e.stopPropagation(); handleToggleLikePost(post.id); }}
                    class="flex items-center gap-1.5 hover:text-cyan-300 transition-colors
                      {post.stats.isLiked ? 'text-cyan-400' : ''}"
                  >
                    <ThumbsUp class="w-3.5 h-3.5 {post.stats.isLiked ? 'fill-cyan-400' : ''}" />
                    <span>{post.stats.likesCount}</span>
                  </button>

                  <div class="flex items-center gap-1.5">
                    <MessageSquare class="w-3.5 h-3.5" />
                    <span>{post.stats.commentsCount}</span>
                  </div>
                </div>

                <button
                  type="button"
                  onclick={(e) => { e.stopPropagation(); handleSharePost(post.id); }}
                  class="flex items-center gap-1.5 hover:text-cyan-300 transition-colors"
                >
                  <Share2 class="w-3.5 h-3.5" />
                  <span>Поділитись</span>
                </button>
              </div>
            </div>
          {/each}
        </div>
      {:else}
        <div class="text-center py-24 bg-[#092635]/60 rounded-3xl border border-cyan-500/20 shadow-xl space-y-3">
          <MessageSquare class="w-12 h-12 text-cyan-500/40 mx-auto" />
          <h4 class="text-base font-bold text-white">У спільноті поки немає обговорень</h4>
          <p class="text-xs text-slate-400 max-w-sm mx-auto">
            Оберіть інший розділ або розпочніть нове обговорення першим!
          </p>
        </div>
      {/if}
    </div>

    <!-- Right Sidebar: Filters & Navigation Panel -->
    <div class="lg:col-span-4 sticky top-36 space-y-4">
      <div class="bg-[#092635] border border-cyan-500/25 rounded-3xl p-5 sm:p-6 shadow-2xl space-y-5">
        
        <!-- In-Section Search Input -->
        <div class="space-y-2">
          <label for="search-community" class="block text-[11px] font-black text-slate-400 uppercase tracking-wider">
            Пошук у розділі
          </label>
          <div class="relative">
            <input
              id="search-community"
              type="text"
              placeholder={searchPlaceholder}
              bind:value={searchQuery}
              oninput={() => loadPosts()}
              class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#041219] border border-cyan-500/30 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400"
            />
            <Search class="w-3.5 h-3.5 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
          </div>
        </div>

        <!-- Sorting dropdown -->
        <div class="space-y-2">
          <label for="sort-community" class="block text-[11px] font-black text-slate-400 uppercase tracking-wider">
            Сортування
          </label>
          <div class="relative">
            <select
              id="sort-community"
              bind:value={sortBy}
              onchange={() => loadPosts()}
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#041219] border border-cyan-500/30 text-xs text-white focus:outline-none focus:border-cyan-400 appearance-none cursor-pointer"
            >
              <option value="newest">Спочатку нові</option>
              <option value="popular">Популярні</option>
              <option value="rating">За рейтингом</option>
            </select>
            <div class="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none text-slate-500">▼</div>
          </div>
        </div>

        <!-- Section Switcher Category List -->
        <div class="space-y-2">
          <span class="block text-[11px] font-black text-slate-400 uppercase tracking-wider">
            Категорії
          </span>
          <nav class="space-y-1.5">
            {#each Object.entries(categoryLabels) as [key, label]}
              <button
                onclick={() => { activeCategory = key as any; loadPosts(); }}
                class="w-full text-left px-3.5 py-2.5 rounded-xl text-xs font-bold transition-all flex items-center justify-between cursor-pointer
                  {activeCategory === key 
                    ? 'bg-[#088395] text-white shadow-lg font-black' 
                    : 'text-slate-400 hover:text-white hover:bg-slate-900/60'}"
              >
                <span>{label}</span>
              </button>
            {/each}
          </nav>
        </div>

      </div>
    </div>
  </div>
{:else}
  <!-- DETAILED POST VIEW -->
  <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start pt-2">
    
    <!-- Left Column: Main Post details & Comments -->
    <div class="lg:col-span-8 space-y-6">
      
      <!-- Back button -->
      <button
        onclick={handleBackToList}
        class="inline-flex items-center gap-2 text-xs font-extrabold text-slate-400 hover:text-cyan-300 transition-colors cursor-pointer"
      >
        <ArrowLeft class="w-4 h-4" />
        <span>Назад до списку</span>
      </button>

      {#if isLoadingPostDetails || !selectedPost}
        <div class="flex items-center justify-center py-24">
          <Loader2 class="w-10 h-10 text-cyan-400 animate-spin" />
        </div>
      {:else}
        {@const Icon = getCategoryIcon(selectedPost.category)}
        <!-- Main post card -->
        <div class="p-6 rounded-3xl bg-[#092635] border border-cyan-500/25 shadow-xl space-y-5">
          
          <!-- Author header -->
          <div class="flex items-center justify-between">
            <button
              type="button"
              onclick={() => profileStore.viewProfile(selectedPost.author.id)}
              class="flex items-center gap-3 cursor-pointer text-left"
            >
              <div class="w-10 h-10 rounded-full bg-[#041219] border border-cyan-500/30 overflow-hidden flex items-center justify-center text-cyan-300 font-bold">
                {#if selectedPost.author.avatarUrl}
                  <img src={selectedPost.author.avatarUrl} alt={selectedPost.author.username} class="w-full h-full object-cover" />
                {:else}
                  {selectedPost.author.username.charAt(0).toUpperCase()}
                {/if}
              </div>
              <div>
                <span class="block text-sm font-black text-white hover:text-cyan-300 transition-colors">{selectedPost.author.username}</span>
                <span class="block text-xs text-slate-400">{selectedPost.createdAt}</span>
              </div>
            </button>

            <span class="px-2.5 py-0.5 rounded-lg bg-[#041219] text-cyan-300 font-bold text-[10px] border border-cyan-500/25 uppercase flex items-center gap-1">
              <Icon class="w-3.5 h-3.5" />
              {categoryLabels[selectedPost.category]}
            </span>
          </div>

          <!-- Post Content -->
          <div class="space-y-4">
            {#if selectedPost.title}
              <h1 class="text-xl sm:text-2xl font-black text-white leading-snug">{selectedPost.title}</h1>
            {/if}
            <p class="text-xs sm:text-sm text-slate-200 leading-relaxed whitespace-pre-wrap">{selectedPost.content}</p>

            <!-- Full-size Hero Media -->
            {#if selectedPost.media.type === 'image' && selectedPost.media.url}
              <div class="w-full rounded-2xl overflow-hidden border border-cyan-500/20">
                <img src={selectedPost.media.url} alt={selectedPost.title} class="w-full h-auto max-h-[500px] object-cover" />
              </div>
            {:else if selectedPost.media.type === 'video' && selectedPost.media.url}
              <div class="relative w-full aspect-video rounded-2xl overflow-hidden bg-slate-950 border border-cyan-500/25">
                <!-- Check if youtube to embed iframe, else standard video -->
                {#if selectedPost.media.url.includes('youtube.com') || selectedPost.media.url.includes('youtu.be')}
                  {@const ytId = selectedPost.media.url.split('v=')[1]?.split('&')[0] || selectedPost.media.url.split('/').pop()}
                  <iframe
                    width="100%"
                    height="100%"
                    src="https://www.youtube.com/embed/{ytId}"
                    title={selectedPost.title}
                    frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                    allowfullscreen
                  ></iframe>
                {:else}
                  <video src={selectedPost.media.url} controls class="w-full h-full"></video>
                {/if}
              </div>
            {/if}
          </div>

          <!-- Interaction bar -->
          <div class="flex items-center justify-between pt-4 border-t border-cyan-950/60 text-xs font-semibold text-slate-400">
            <div class="flex items-center gap-4">
              <button
                type="button"
                onclick={() => handleToggleLikePost(selectedPost!.id, true)}
                class="flex items-center gap-2 hover:text-cyan-300 transition-colors
                  {selectedPost.stats.isLiked ? 'text-cyan-400' : ''}"
              >
                <ThumbsUp class="w-4 h-4 {selectedPost.stats.isLiked ? 'fill-cyan-400' : ''}" />
                <span>{selectedPost.stats.likesCount}</span>
              </button>

              <div class="flex items-center gap-2">
                <MessageSquare class="w-4 h-4" />
                <span>{selectedPost.stats.commentsCount} коментарів</span>
              </div>
            </div>

            <button
              type="button"
              onclick={() => handleSharePost(selectedPost!.id)}
              class="flex items-center gap-2 hover:text-cyan-300 transition-colors"
            >
              <Share2 class="w-4 h-4" />
              <span>Поділитись</span>
            </button>
          </div>
        </div>

        <!-- COMMENT SECTION (Bottom Area) -->
        <div class="p-6 rounded-3xl bg-[#092635] border border-cyan-500/25 shadow-xl space-y-6">
          <div class="flex items-center justify-between">
            <h3 class="text-base font-black text-white uppercase tracking-wider font-display">Обговорення</h3>
            
            <div class="flex items-center gap-2 text-xs">
              <span class="text-slate-400">Сортування:</span>
              <select
                bind:value={commentSortBy}
                class="px-2 py-1 rounded-lg bg-[#041219] border border-cyan-500/30 text-xs text-white focus:outline-none"
              >
                <option value="newest">Спочатку нові</option>
                <option value="oldest">Спочатку старі</option>
              </select>
            </div>
          </div>

          <!-- Active Comment Input Box -->
          <div class="flex gap-3">
            <div class="w-8 h-8 rounded-full bg-gradient-to-br from-cyan-400 to-emerald-500 flex items-center justify-center text-black font-black text-xs shrink-0 overflow-hidden">
              {#if $currentUser?.avatarUrl}
                <img src={$currentUser.avatarUrl} alt={$currentUser.username} class="w-full h-full object-cover" />
              {:else if $currentUser}
                {$currentUser.username.charAt(0).toUpperCase()}
              {:else}
                G
              {/if}
            </div>

            <div class="flex-1 space-y-2">
              <textarea
                placeholder={$currentUser ? "Написати коментар..." : "Будь ласка, увійдіть в акаунт, щоб прокоментувати..."}
                bind:value={newCommentText}
                disabled={!$currentUser}
                rows="3"
                class="w-full p-3 rounded-xl bg-[#041219] border border-cyan-500/20 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400"
              ></textarea>

              {#if $currentUser}
                <div class="flex items-center justify-end gap-2">
                  <button
                    type="button"
                    onclick={() => newCommentText = ''}
                    class="px-3.5 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-[10px] font-bold text-slate-300 transition-colors cursor-pointer"
                  >
                    Відхилити
                  </button>
                  <button
                    type="button"
                    onclick={handleAddCommentSubmit}
                    disabled={!newCommentText.trim()}
                    class="px-4 py-1.5 rounded-lg bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black text-[10px] font-black tracking-wide shadow-md transition-all cursor-pointer flex items-center gap-1 disabled:opacity-50"
                  >
                    <Send class="w-3 h-3" />
                    <span>Надіслати</span>
                  </button>
                </div>
              {/if}
            </div>
          </div>

          <!-- Comments List -->
          <div class="space-y-4 pt-2 border-t border-cyan-950/60">
            {#each sortedComments as c (c.id)}
              <div class="space-y-3">
                <div class="flex gap-3">
                  <button
                    type="button"
                    onclick={() => profileStore.viewProfile(c.author.id)}
                    class="w-8 h-8 rounded-full bg-[#041219] border border-cyan-500/25 overflow-hidden flex items-center justify-center text-cyan-300 font-black text-xs shrink-0 cursor-pointer"
                  >
                    {#if c.author.avatarUrl}
                      <img src={c.author.avatarUrl} alt={c.author.username} class="w-full h-full object-cover" />
                    {:else}
                      {c.author.username.charAt(0).toUpperCase()}
                    {/if}
                  </button>

                  <div class="flex-1 min-w-0 space-y-1">
                    <div class="flex items-baseline justify-between gap-2">
                      <button
                        type="button"
                        onclick={() => profileStore.viewProfile(c.author.id)}
                        class="text-xs font-bold text-slate-200 hover:text-cyan-300 transition-colors cursor-pointer text-left"
                      >
                        {c.author.username}
                      </button>
                      <span class="text-[9px] text-slate-500">{c.createdAt}</span>
                    </div>
                    <p class="text-xs text-slate-300 leading-relaxed whitespace-pre-wrap">{c.content}</p>

                    <!-- Reply trigger and comment likes -->
                    <div class="flex items-center gap-4 pt-1.5 text-[10px] font-bold text-slate-400">
                      <button
                        type="button"
                        onclick={() => activeReplyCommentId = activeReplyCommentId === c.id ? null : c.id}
                        class="hover:text-cyan-300 transition-colors flex items-center gap-1"
                      >
                        <span>Відповісти</span>
                      </button>
                    </div>
                  </div>
                </div>

                <!-- Nested Replies list -->
                {#if c.replies && c.replies.length > 0}
                  <div class="pl-8 space-y-3 border-l border-cyan-500/20 ml-4">
                    {#each c.replies as reply (reply.id)}
                      <div class="flex gap-2">
                        <button
                          type="button"
                          onclick={() => profileStore.viewProfile(reply.author.id)}
                          class="w-6 h-6 rounded-full bg-[#041219] border border-cyan-500/20 overflow-hidden flex items-center justify-center text-cyan-300 font-bold text-[10px] shrink-0 cursor-pointer"
                        >
                          {#if reply.author.avatarUrl}
                            <img src={reply.author.avatarUrl} alt={reply.author.username} class="w-full h-full object-cover" />
                          {:else}
                            {reply.author.username.charAt(0).toUpperCase()}
                          {/if}
                        </button>
                        <div class="flex-1 min-w-0">
                          <div class="flex items-baseline justify-between gap-2">
                            <button
                              type="button"
                              onclick={() => profileStore.viewProfile(reply.author.id)}
                              class="text-[11px] font-bold text-slate-300 hover:text-cyan-300 transition-colors cursor-pointer text-left"
                            >
                              {reply.author.username}
                            </button>
                            <span class="text-[8px] text-slate-500">{reply.createdAt}</span>
                          </div>
                          <p class="text-[11px] text-slate-400 leading-relaxed whitespace-pre-wrap">{reply.content}</p>
                        </div>
                      </div>
                    {/each}
                  </div>
                {/if}

                <!-- Active reply input box -->
                {#if activeReplyCommentId === c.id && $currentUser}
                  <div class="pl-8 ml-4 flex gap-3">
                    <textarea
                      placeholder="Написати відповідь..."
                      bind:value={replyText[c.id]}
                      rows="2"
                      class="flex-1 p-2 rounded-xl bg-[#041219] border border-cyan-500/20 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400"
                    ></textarea>
                    <div class="flex flex-col gap-1.5">
                      <button
                        type="button"
                        onclick={() => handleAddReplySubmit(c.id)}
                        disabled={!replyText[c.id]?.trim()}
                        class="px-3 py-1.5 rounded-lg bg-[#22d3ee] text-black text-[10px] font-bold disabled:opacity-50 cursor-pointer"
                      >
                        Надіслати
                      </button>
                      <button
                        type="button"
                        onclick={() => activeReplyCommentId = null}
                        class="text-[9px] text-slate-500 hover:text-slate-300 cursor-pointer text-center"
                      >
                        Скасувати
                      </button>
                    </div>
                  </div>
                {/if}
              </div>
            {/each}
          </div>
        </div>

      {/if}
    </div>

    <!-- Right Sidebar ("Інші обговорення") -->
    <div class="lg:col-span-4 sticky top-36 space-y-4">
      <div class="bg-[#092635] border border-cyan-500/25 rounded-3xl p-5 sm:p-6 shadow-2xl space-y-4">
        <h3 class="text-xs font-black text-slate-300 uppercase tracking-wider">
          Інші обговорення
        </h3>

        {#if otherPosts.length > 0}
          <div class="space-y-3.5">
            {#each otherPosts as op (op.id)}
              <button
                type="button"
                onclick={() => handleSelectPost(op.id)}
                class="w-full text-left p-3 rounded-2xl bg-[#041219]/60 hover:bg-cyan-500/10 border border-cyan-500/10 hover:border-cyan-500/30 transition-all flex gap-3 group cursor-pointer"
              >
                {#if op.media.url}
                  <div class="w-16 h-12 rounded-xl overflow-hidden bg-slate-950 shrink-0 border border-cyan-500/20">
                    <img src={op.media.url} alt={op.title} class="w-full h-full object-cover" />
                  </div>
                {/if}
                <div class="min-w-0 flex-1 space-y-1">
                  <h4 class="text-xs font-bold text-white group-hover:text-cyan-300 transition-colors truncate">
                    {op.title || op.content}
                  </h4>
                  <div class="flex items-center gap-3 text-[9px] text-slate-400 font-medium">
                    <span class="flex items-center gap-0.5"><ThumbsUp class="w-2.5 h-2.5" /> {op.stats.likesCount}</span>
                    <span class="flex items-center gap-0.5"><MessageSquare class="w-2.5 h-2.5" /> {op.stats.commentsCount}</span>
                  </div>
                </div>
              </button>
            {/each}
          </div>
        {:else}
          <p class="text-[11px] text-slate-500 font-medium py-3 text-center">Інших обговорень немає.</p>
        {/if}
      </div>
    </div>

  </div>
{/if}

<!-- CREATE POST MODAL -->
{#if isCreateModalOpen}
  <div class="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center p-4 animate-in fade-in">
    <div class="relative w-full max-w-lg bg-[#061820] border border-cyan-500/30 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/80 space-y-5">
      <div class="flex items-center justify-between">
        <h3 class="text-lg font-black text-white font-display">Створити публікацію</h3>
        <button
          onclick={() => isCreateModalOpen = false}
          class="p-1 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800/60"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <form onsubmit={handleCreatePostSubmit} class="space-y-4">
        
        <!-- Explicit Game Selector Field -->
        <div class="space-y-1.5">
          <label for="new-post-game" class="block text-xs font-bold text-slate-300">Оберіть гру</label>
          <select
            id="new-post-game"
            bind:value={targetGameId}
            class="w-full px-3 py-2 rounded-xl bg-[#041219] border border-cyan-500/20 text-xs text-white focus:outline-none focus:border-cyan-400 cursor-pointer"
          >
            {#each $gamesStore.games as g}
              <option value={g.id}>{g.title}</option>
            {:else}
              <option value={gameId}>{gameTitle || 'Поточна гра'}</option>
            {/each}
          </select>
        </div>

        <!-- Category picker -->
        <div class="space-y-1.5">
          <label for="new-post-cat" class="block text-xs font-bold text-slate-300">Категорія публікації</label>
          <select
            id="new-post-cat"
            bind:value={newPostCategory}
            class="w-full px-3 py-2 rounded-xl bg-[#041219] border border-cyan-500/20 text-xs text-white focus:outline-none focus:border-cyan-400"
          >
            <option value="forum">Форум</option>
            <option value="screenshots">Скріншоти</option>
            <option value="videos">Відео</option>
            <option value="guides">Гайди</option>
          </select>
        </div>

        <!-- Title -->
        {#if newPostCategory !== 'screenshots'}
          <div class="space-y-1.5">
            <label for="new-post-title" class="block text-xs font-bold text-slate-300">Заголовок</label>
            <input
              type="text"
              id="new-post-title"
              placeholder="Введіть заголовок публікації..."
              bind:value={newPostTitle}
              class="w-full px-3 py-2.5 rounded-xl bg-[#041219] border border-cyan-500/20 text-xs text-white focus:outline-none focus:border-cyan-400"
            />
          </div>
        {/if}

        <!-- Text Description Area -->
        <div class="space-y-1.5">
          <label for="new-post-desc" class="block text-xs font-bold text-slate-300">Опис / Вміст</label>
          <textarea
            id="new-post-desc"
            placeholder="Напишіть текст вашої публікації тут..."
            bind:value={newPostContent}
            rows="5"
            class="w-full p-3 rounded-xl bg-[#041219] border border-cyan-500/20 text-xs text-white focus:outline-none focus:border-cyan-400"
          ></textarea>
        </div>

        <!-- Media Linking options -->
        <div class="space-y-2 p-3.5 rounded-2xl bg-[#041219]/60 border border-cyan-500/10">
          <span class="block text-xs font-bold text-slate-300 mb-1.5">Медіафайли</span>
          
          <div class="flex gap-3 text-[11px] font-bold text-slate-400">
            <label class="flex items-center gap-1 cursor-pointer">
              <input type="radio" name="media-type" value="none" bind:group={newPostMediaType} />
              <span>Без медіа</span>
            </label>
            <label class="flex items-center gap-1 cursor-pointer">
              <input type="radio" name="media-type" value="image" bind:group={newPostMediaType} />
              <span>Зображення (URL)</span>
            </label>
            <label class="flex items-center gap-1 cursor-pointer">
              <input type="radio" name="media-type" value="video" bind:group={newPostMediaType} />
              <span>Відео (YouTube URL)</span>
            </label>
          </div>

          {#if newPostMediaType !== 'none'}
            <div class="pt-2">
              <input
                type="text"
                placeholder={newPostMediaType === 'image' ? "Вставте посилання на зображення (наприклад, Unsplash)..." : "Вставте посилання на YouTube відео..."}
                bind:value={newPostMediaUrl}
                class="w-full px-3 py-2 rounded-xl bg-[#030d12] border border-cyan-500/20 text-[11px] text-white focus:outline-none focus:border-cyan-400"
              />
            </div>
          {/if}
        </div>

        <!-- Buttons -->
        <div class="flex items-center justify-end gap-3 pt-2">
          <button
            type="button"
            onclick={() => isCreateModalOpen = false}
            disabled={isSubmittingPost}
            class="px-4 py-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-xs font-bold text-slate-300 cursor-pointer disabled:opacity-50"
          >
            Скасувати
          </button>
          <button
            type="submit"
            disabled={isSubmittingPost || !newPostContent.trim()}
            class="px-5 py-2.5 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 text-black font-black text-xs cursor-pointer shadow-md disabled:opacity-50 flex items-center gap-2"
          >
            {#if isSubmittingPost}
              <Loader2 class="w-3.5 h-3.5 animate-spin" />
            {/if}
            <span>Опублікувати</span>
          </button>
        </div>

      </form>
    </div>
  </div>
{/if}
