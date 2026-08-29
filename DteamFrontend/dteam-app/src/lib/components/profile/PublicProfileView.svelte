<script lang="ts">
  import { onMount } from 'svelte';
  import { profileStore } from '../../stores/profileStore';
  import { currentUser } from '../../stores/authStore';
  import { friendsService } from '../../services/friendsService';
  import { communityService, type CommunityPost } from '../../services/communityService';
  import { uiStore } from '../../stores/uiStore';
  import { formatPrice, formatDate } from '../../utils/formatters';
  import { UserStatus } from '../../types';
  import {
    UserPlus, UserCheck, Clock, MessageSquare, MoreHorizontal,
    ThumbsUp, Loader2, Gamepad2, Users, ArrowLeft
  } from 'lucide-svelte';

  type TabId = 'ігри' | 'друзі' | 'обговорення' | 'скріншоти' | 'відео' | 'гайди';
  let activeTab = $state<TabId>('обговорення');

  const profile = $derived($profileStore.profile);
  const friends = $derived($profileStore.friends);
  const uniqueFriends = $derived(
    Array.from(new Map(friends.map((f) => [f.id, f])).values())
  );
  const isLoading = $derived($profileStore.isLoading);
  const error = $derived($profileStore.error);

  const menuItems: { id: TabId; label: string; count: (() => number | null) }[] = [
    { id: 'ігри', label: 'Ігри', count: () => profile?.gamesCount ?? null },
    { id: 'друзі', label: 'Друзі', count: () => uniqueFriends.length },
    { id: 'обговорення', label: 'Обговорення', count: () => null },
    { id: 'скріншоти', label: 'Скріншоти', count: () => null },
    { id: 'відео', label: 'Відео', count: () => null },
    { id: 'гайди', label: 'Гайди', count: () => null },
  ];

  let allPosts = $state<CommunityPost[]>([]);
  let isLoadingPosts = $state(false);

  async function loadUserPosts() {
    if (!profile) return;
    isLoadingPosts = true;
    try {
      const res = await communityService.getPosts(null as any, 'all');
      allPosts = res.posts.filter((p) => p.author.id === profile.id);
    } catch (e) {
      console.warn('[PublicProfileView] Не вдалося завантажити пости користувача:', e);
    } finally {
      isLoadingPosts = false;
    }
  }

  const discussionPosts = $derived(allPosts.filter((p) => p.category === 'forum'));
  const screenshotPosts = $derived(allPosts.filter((p) => p.category === 'screenshots'));
  const videoPosts = $derived(allPosts.filter((p) => p.category === 'videos'));
  const guidePosts = $derived(allPosts.filter((p) => p.category === 'guides'));

  $effect(() => {
    if (profile) loadUserPosts();
  });

  let isSendingRequest = $state(false);

  async function handleAddFriend() {
    if (!profile || isSendingRequest) return;
    isSendingRequest = true;
    try {
      await friendsService.sendFriendRequest(profile.username);
      uiStore.addToast({
        title: 'Запит надіслано',
        message: `Запит у друзі надіслано користувачеві ${profile.username}.`,
        type: 'success',
      });
      profileStore.reload();
    } catch (e: any) {
      uiStore.addToast({
        title: 'Помилка',
        message: e?.message || 'Не вдалося надіслати запит у друзі.',
        type: 'error',
      });
    } finally {
      isSendingRequest = false;
    }
  }

  function statusLabel(status?: number) {
    switch (status) {
      case UserStatus.Online: return { text: 'онлайн', color: 'text-emerald-400' };
      case UserStatus.InGame: return { text: 'у грі', color: 'text-cyan-400' };
      case UserStatus.Away: return { text: 'відійшов', color: 'text-amber-400' };
      default: return { text: 'офлайн', color: 'text-slate-500' };
    }
  }
</script>

<div class="min-h-screen bg-[#05181e] text-slate-200 font-sans pb-12">
  {#if isLoading}
    <div class="flex items-center justify-center py-32">
      <Loader2 class="w-10 h-10 text-cyan-400 animate-spin" />
    </div>
  {:else if error || !profile}
    <div class="max-w-2xl mx-auto px-4 py-24 text-center">
      <h2 class="text-2xl font-bold text-white mb-2">Профіль не знайдено</h2>
      <p class="text-sm text-slate-400 mb-6">{error || 'Такого користувача не існує.'}</p>
      <button
        onclick={() => uiStore.setTab('store')}
        class="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-[#0b4e63] hover:bg-[#0d627a] text-white text-sm font-bold transition-colors"
      >
        <ArrowLeft class="w-4 h-4" /> На головну
      </button>
    </div>
  {:else}
    
    <div
      class="w-full h-48 md:h-64 bg-cover bg-center bg-gradient-to-br from-[#0b4e63] via-[#03232c] to-[#05181e]"
      style={profile.bannerUrl ? `background-image: url('${profile.bannerUrl}')` : ''}
    ></div>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">

      <div class="flex flex-col md:flex-row justify-between items-start md:items-end -mt-16 md:-mt-20 mb-8 relative z-10 gap-4">
        <div class="flex flex-col md:flex-row gap-6 items-start md:items-end">
          
          <div class="w-32 h-32 md:w-40 md:h-40 rounded-full border-4 border-[#05181e] overflow-hidden bg-[#03232c] shrink-0">
            {#if profile.avatarUrl}
              <img src={profile.avatarUrl} alt={profile.username} class="w-full h-full object-cover" />
            {:else}
              <div class="w-full h-full flex items-center justify-center text-4xl font-black text-white bg-gradient-to-tr from-cyan-500 to-blue-600">
                {profile.username.charAt(0).toUpperCase()}
              </div>
            {/if}
          </div>

          <div class="pb-2">
            <div class="flex flex-wrap items-center gap-2 mb-1">
              <h1 class="text-2xl font-bold text-white">{profile.username}</h1>
              {#if profile.isAdmin}
                <span class="px-2 py-0.5 rounded-md bg-amber-500/20 text-amber-300 border border-amber-500/40 text-[10px] font-bold uppercase">
                  Admin
                </span>
              {/if}
              {#if profile.isInFamily}
                <span class="px-2 py-0.5 rounded-md bg-blue-500/20 text-blue-300 border border-blue-500/40 text-[10px] font-bold uppercase">
                  Family
                </span>
              {/if}
            </div>
            <p class="text-sm mb-4 {statusLabel(profile.status).color}">{statusLabel(profile.status).text}</p>
            <p class="text-sm text-slate-400 max-w-2xl leading-relaxed">
              {profile.bio || 'Цей користувач ще не додав опис профілю.'}
            </p>
          </div>
        </div>

        {#if !profile.isOwnProfile}
          <div class="flex items-center gap-3 pb-2 w-full md:w-auto">
            {#if profile.friendshipStatus === 'friends'}
              <span class="flex items-center gap-2 bg-emerald-500/15 text-emerald-300 border border-emerald-500/30 px-5 py-2.5 rounded-full text-sm font-medium">
                <UserCheck class="w-4 h-4" /> У друзях
              </span>
            {:else if profile.friendshipStatus === 'pending'}
              <span class="flex items-center gap-2 bg-slate-800/80 text-slate-300 border border-slate-700 px-5 py-2.5 rounded-full text-sm font-medium">
                <Clock class="w-4 h-4" /> {profile.isIncomingRequest ? 'Запит вам' : 'Запит надіслано'}
              </span>
            {:else}
              <button
                onclick={handleAddFriend}
                disabled={isSendingRequest}
                class="flex items-center gap-2 bg-[#0b4e63] hover:bg-[#0d627a] text-white px-5 py-2.5 rounded-full text-sm font-medium transition-colors disabled:opacity-50 cursor-pointer"
              >
                <UserPlus class="w-4 h-4" />
                {isSendingRequest ? 'Надсилання...' : 'Додати в друзі'}
              </button>
            {/if}
            <button class="bg-[#0b4e63] hover:bg-[#0d627a] text-white p-2.5 rounded-full transition-colors cursor-pointer" title="Написати повідомлення">
              <MessageSquare class="w-5 h-5" />
            </button>
            <button class="bg-[#0b4e63] hover:bg-[#0d627a] text-white p-2.5 rounded-full transition-colors cursor-pointer" title="Ще">
              <MoreHorizontal class="w-5 h-5" />
            </button>
          </div>
        {/if}
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-[1fr_320px] gap-6">

        <div class="bg-[#03232c] border border-cyan-900/40 rounded-2xl p-6">

          {#if activeTab === 'ігри'}
            {#if profile.libraryGames.length === 0 && profile.publishedGames.length === 0}
              <div class="text-center py-16 text-slate-500 text-sm">
                У користувача поки немає ігор.
              </div>
            {:else}
              <div class="space-y-6">
                {#if profile.libraryGames.length > 0}
                  <div>
                    <h3 class="text-sm font-bold text-slate-300 mb-3">Бібліотека ({profile.libraryGames.length})</h3>
                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      {#each profile.libraryGames as game}
                        <div class="flex items-center gap-4 p-3 rounded-2xl bg-[#02171d] border border-cyan-900/30">
                          <img src={game.coverImageUrl || undefined} alt={game.title} class="w-16 h-20 object-cover rounded-xl bg-slate-800 shrink-0" />
                          <div class="min-w-0">
                            <h4 class="font-bold text-white text-sm truncate">{game.title}</h4>
                            {#if game.isDlc}
                              <span class="text-[10px] px-1.5 py-0.5 rounded bg-blue-500/20 text-blue-300 font-bold uppercase">DLC</span>
                            {:else}
                              <span class="text-xs text-cyan-400 font-mono">
                                {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                              </span>
                            {/if}
                          </div>
                        </div>
                      {/each}
                    </div>
                  </div>
                {/if}

                {#if profile.publishedGames.length > 0}
                  <div>
                    <h3 class="text-sm font-bold text-slate-300 mb-3">Опубліковані ігри ({profile.publishedGames.length})</h3>
                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      {#each profile.publishedGames as game}
                        <div class="flex items-center gap-4 p-3 rounded-2xl bg-[#02171d] border border-cyan-900/30">
                          <img src={game.coverImageUrl || undefined} alt={game.title} class="w-16 h-20 object-cover rounded-xl bg-slate-800 shrink-0" />
                          <div class="min-w-0">
                            <h4 class="font-bold text-white text-sm truncate">{game.title}</h4>
                            <span class="text-xs text-cyan-400 font-mono">
                              {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                            </span>
                          </div>
                        </div>
                      {/each}
                    </div>
                  </div>
                {/if}
              </div>
            {/if}
          {/if}

          {#if activeTab === 'друзі'}
            {#if uniqueFriends.length === 0}
              <div class="text-center py-16 text-slate-500 text-sm">
                У користувача поки немає друзів.
              </div>
            {:else}
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {#each uniqueFriends as f}
                  <button
                    onclick={() => profileStore.viewProfile(f.id)}
                    class="flex items-center gap-3 p-3 rounded-2xl bg-[#02171d] border border-cyan-900/30 hover:border-cyan-600/60 transition-colors text-left cursor-pointer"
                  >
                    <img src={f.avatarUrl || undefined} alt={f.username} class="w-10 h-10 rounded-full object-cover bg-slate-800" />
                    <span class="text-sm font-bold text-slate-200 truncate">{f.username}</span>
                  </button>
                {/each}
              </div>
            {/if}
          {/if}

          {#if activeTab === 'обговорення'}
            {#if isLoadingPosts}
              <div class="flex justify-center py-16"><Loader2 class="w-8 h-8 text-cyan-400 animate-spin" /></div>
            {:else if discussionPosts.length === 0}
              <div class="text-center py-16 text-slate-500 text-sm">Користувач ще не публікував обговорень.</div>
            {:else}
              <div class="space-y-4">
                {#each discussionPosts as post (post.id)}
                  <div class="bg-[#02171d] rounded-2xl p-5 border border-cyan-900/30">
                    <div class="flex items-center justify-between mb-3">
                      <span class="text-xs text-slate-500">{formatDate(post.createdAt)}</span>
                    </div>
                    <h3 class="text-lg font-bold text-white mb-2">{post.title}</h3>
                    <p class="text-sm text-slate-400 mb-3 whitespace-pre-line">{post.content}</p>
                    {#if post.media?.type === 'image' && post.media.url}
                      <img src={post.media.url} alt="" class="w-full h-auto rounded-xl mb-3 object-cover max-h-96" />
                    {/if}
                    <div class="flex gap-4 text-xs font-medium text-slate-400">
                      <span class="flex items-center gap-1.5 bg-[#03232c] px-3 py-1.5 rounded-full border border-cyan-900/40">
                        <ThumbsUp class="w-3.5 h-3.5 {post.stats.isLiked ? 'fill-cyan-400 text-cyan-400' : ''}" /> {post.stats.likesCount}
                      </span>
                      <span class="flex items-center gap-1.5 bg-[#03232c] px-3 py-1.5 rounded-full border border-cyan-900/40">
                        <MessageSquare class="w-3.5 h-3.5" /> {post.stats.commentsCount}
                      </span>
                    </div>
                  </div>
                {/each}
              </div>
            {/if}
          {/if}

          {#if activeTab === 'скріншоти'}
            {#if isLoadingPosts}
              <div class="flex justify-center py-16"><Loader2 class="w-8 h-8 text-cyan-400 animate-spin" /></div>
            {:else if screenshotPosts.length === 0}
              <div class="text-center py-16 text-slate-500 text-sm">Користувач ще не публікував скріншотів.</div>
            {:else}
              <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
                {#each screenshotPosts as post (post.id)}
                  <div class="aspect-[16/10] rounded-xl overflow-hidden cursor-pointer hover:opacity-80 transition-opacity bg-slate-800">
                    {#if post.media?.url}
                      <img src={post.media.url} alt={post.title} class="w-full h-full object-cover" />
                    {/if}
                  </div>
                {/each}
              </div>
            {/if}
          {/if}

          {#if activeTab === 'відео'}
            {#if isLoadingPosts}
              <div class="flex justify-center py-16"><Loader2 class="w-8 h-8 text-cyan-400 animate-spin" /></div>
            {:else if videoPosts.length === 0}
              <div class="text-center py-16 text-slate-500 text-sm">Користувач ще не публікував відео.</div>
            {:else}
              <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
                {#each videoPosts as post (post.id)}
                  <div class="aspect-[16/10] rounded-xl overflow-hidden relative cursor-pointer group bg-slate-800">
                    {#if post.media?.thumbnailUrl || post.media?.url}
                      <img src={post.media.thumbnailUrl || post.media.url} alt={post.title} class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />
                    {/if}
                    <div class="absolute inset-0 bg-black/30 flex items-center justify-center">
                      <div class="w-10 h-10 bg-white rounded-full flex items-center justify-center shadow-lg">
                        <svg class="w-5 h-5 text-black ml-1" viewBox="0 0 24 24" fill="currentColor"><path d="M8 5v14l11-7z"/></svg>
                      </div>
                    </div>
                  </div>
                {/each}
              </div>
            {/if}
          {/if}

          {#if activeTab === 'гайди'}
            {#if isLoadingPosts}
              <div class="flex justify-center py-16"><Loader2 class="w-8 h-8 text-cyan-400 animate-spin" /></div>
            {:else if guidePosts.length === 0}
              <div class="text-center py-16 text-slate-500 text-sm">Користувач ще не публікував гайдів.</div>
            {:else}
              <div class="space-y-4">
                {#each guidePosts as post (post.id)}
                  <div class="bg-[#02171d] rounded-2xl p-5 border border-cyan-900/30">
                    <div class="flex items-center justify-between mb-4">
                      <span class="text-xs text-slate-500">{formatDate(post.createdAt)}</span>
                    </div>
                    <div class="flex flex-col sm:flex-row gap-5 mb-4">
                      {#if post.media?.url}
                        <img src={post.media.url} alt={post.title} class="w-full sm:w-48 h-32 object-cover rounded-xl shrink-0 bg-slate-800" />
                      {/if}
                      <div>
                        <h3 class="text-lg font-bold text-white mb-2">{post.title}</h3>
                        <p class="text-sm text-slate-400 line-clamp-3 whitespace-pre-line">{post.content}</p>
                      </div>
                    </div>
                    <div class="flex gap-4 text-xs font-medium text-slate-400">
                      <span class="flex items-center gap-1.5 bg-[#03232c] px-3 py-1.5 rounded-full border border-cyan-900/40">
                        <ThumbsUp class="w-3.5 h-3.5 {post.stats.isLiked ? 'fill-cyan-400 text-cyan-400' : ''}" /> {post.stats.likesCount}
                      </span>
                      <span class="flex items-center gap-1.5 bg-[#03232c] px-3 py-1.5 rounded-full border border-cyan-900/40">
                        <MessageSquare class="w-3.5 h-3.5" /> {post.stats.commentsCount}
                      </span>
                    </div>
                  </div>
                {/each}
              </div>
            {/if}
          {/if}

        </div>

        <div class="space-y-6">

          <div class="bg-[#03232c] border border-cyan-900/40 rounded-2xl p-4">
            <div class="flex items-center gap-3 px-4 mb-4">
              <span class="text-base font-medium">Приєднався</span>
              <span class="text-xs text-slate-400 font-mono">{formatDate(profile.createdAt)}</span>
            </div>

            <nav class="space-y-1">
              {#each menuItems as item}
                <button
                  onclick={() => activeTab = item.id}
                  class="w-full flex items-center justify-between px-4 py-2.5 rounded-xl text-sm transition-colors cursor-pointer {activeTab === item.id ? 'bg-[#0b4e63] text-white' : 'text-slate-300 hover:bg-cyan-900/30'}"
                >
                  <span>{item.label}</span>
                  {#if item.count() !== null}
                    <span class="bg-[#02171d] px-2 py-0.5 rounded-full text-xs text-slate-400">{item.count()}</span>
                  {/if}
                </button>
              {/each}
            </nav>
          </div>

          <div class="bg-[#03232c] border border-cyan-900/40 rounded-2xl p-4">
            <div class="flex items-center justify-between px-2 mb-4">
              <span class="font-medium text-white flex items-center gap-1.5"><Users class="w-4 h-4 text-cyan-400" /> Друзі</span>
              <span class="bg-[#0b4e63] px-2.5 py-0.5 rounded-full text-xs text-white">{uniqueFriends.length}</span>
            </div>
            <div class="space-y-2">
              {#each uniqueFriends.slice(0, 5) as f}
                <button
                  onclick={() => profileStore.viewProfile(f.id)}
                  class="w-full flex items-center gap-3 px-2 py-2 rounded-xl hover:bg-cyan-900/20 transition-colors text-left cursor-pointer"
                >
                  <div class="relative shrink-0">
                    <img src={f.avatarUrl || undefined} alt={f.username} class="w-9 h-9 rounded-full object-cover bg-slate-800 border border-cyan-900/60" />
                    <div class="absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 rounded-full border-2 border-[#03232c]
                      {f.status === UserStatus.Online ? 'bg-emerald-500' : ''}
                      {f.status === UserStatus.InGame ? 'bg-cyan-400' : ''}
                      {f.status === UserStatus.Away ? 'bg-amber-500' : ''}
                      {f.status === UserStatus.Offline ? 'bg-slate-600' : ''}"
                    ></div>
                  </div>
                  <span class="text-sm font-medium text-slate-200 truncate">{f.username}</span>
                </button>
              {:else}
                <p class="text-xs text-slate-500 px-2 py-2">Список друзів порожній.</p>
              {/each}
            </div>
          </div>

        </div>
      </div>
    </div>
  {/if}
</div>
