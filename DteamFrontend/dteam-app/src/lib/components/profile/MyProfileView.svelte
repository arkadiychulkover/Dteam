<script lang="ts">
  import { onMount } from 'svelte';
  import { currentUser, authStore } from '../../stores/authStore';
  import { myProfileStore } from '../../stores/myProfileStore';
  import { libraryStore } from '../../stores/libraryStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { friendsStore } from '../../stores/friendsStore';
  import { profileStore } from '../../stores/profileStore';
  import { communityService } from '../../services/communityService';
  import { userService } from '../../services/userService';
  import { mediaService, MAX_IMAGE_SIZE_BYTES, MAX_VIDEO_SIZE_BYTES } from '../../services/mediaService';
  import { uiStore } from '../../stores/uiStore';
  import { formatDate } from '../../utils/formatters';
  import { UserStatus } from '../../types';
  import {
    Edit3, ThumbsUp, MessageSquare, Loader2, Plus, X, Star, Camera, ImagePlus, Gamepad2, Activity
  } from 'lucide-svelte';
  import SelectGameModal from '../community/SelectGameModal.svelte';
  import ActivityCard from '../activity/ActivityCard.svelte';
  import { activityStore } from '../../stores/activityStore';

  type TabId = 'головна' | 'значки' | 'ігри' | 'бажане' | 'обговорення' | 'скріншоти' | 'відео' | 'гайди' | 'рецензії';
  let activeTab = $state<TabId>('головна');
  let showCreateDropdown = $state(false);

  const uniqueFriends = $derived($friendsStore.friends);

  const menuItems: { id: TabId; label: string; count: () => number | null }[] = [
    { id: 'головна', label: 'Головна', count: () => null },
    { id: 'ігри', label: 'Ігри', count: () => $libraryStore.items.length },
    { id: 'бажане', label: 'Бажане', count: () => $wishlistStore.items.length },
    { id: 'обговорення', label: 'Обговорення', count: () => myDiscussionPosts.length },
    { id: 'скріншоти', label: 'Скріншоти', count: () => myScreenshotPosts.length },
    { id: 'відео', label: 'Відео', count: () => myVideoPosts.length },
    { id: 'гайди', label: 'Гайди', count: () => myGuidePosts.length },
    { id: 'рецензії', label: 'Рецензії', count: () => $myProfileStore.reviews.length },
  ];

  const dlcCount = $derived($libraryStore.items.filter((i) => i.game?.isDlc).length);
  const gamesCount = $derived($libraryStore.items.filter((i) => !i.game?.isDlc).length);

  const myDiscussionPosts = $derived($myProfileStore.posts.filter((p) => p.category === 'forum'));
  const myScreenshotPosts = $derived($myProfileStore.posts.filter((p) => p.category === 'screenshots'));
  const myVideoPosts = $derived($myProfileStore.posts.filter((p) => p.category === 'videos'));
  const myGuidePosts = $derived($myProfileStore.posts.filter((p) => p.category === 'guides'));
  const myActivities = $derived(
    $currentUser ? ($activityStore.userActivities[$currentUser.id.toLowerCase()] || []) : []
  );

  onMount(() => {
    myProfileStore.reload();
    libraryStore.loadLibrary();
    wishlistStore.loadWishlist();
    friendsStore.loadFriends();
    if ($currentUser) {
      activityStore.loadUserActivities($currentUser.id);
    }
  });

  let isCreatingPost = $state(false);
  let createPostType = $state<'forum' | 'screenshots' | 'videos' | 'guides'>('forum');
  let postTitle = $state('');
  let postContent = $state('');
  let postMediaUrl = $state('');
  let postMediaThumbnailUrl = $state('');
  let postMediaPreviewUrl = $state('');
  let selectedPostGame = $state<{ id: string; title: string; bannerUrl?: string } | null>(null);
  let isSelectGameModalOpen = $state(false);
  let isUploadingMedia = $state(false);
  let selectedPostFile = $state<File | null>(null);
  let isSubmittingPost = $state(false);
  let postFileInput: HTMLInputElement | undefined = $state();

  const createTypeLabels: Record<typeof createPostType, string> = {
    forum: 'Обговорення',
    screenshots: 'Скріншот',
    videos: 'Відео',
    guides: 'Гайд',
  };

  function triggerCreatePost(type: typeof createPostType) {
    showCreateDropdown = false;
    createPostType = type;
    postTitle = '';
    postContent = '';
    resetPostMedia();
    isCreatingPost = true;
  }

  function resetPostMedia() {
    selectedPostGame = null;
    if (postMediaPreviewUrl) URL.revokeObjectURL(postMediaPreviewUrl);
    postMediaUrl = '';
    postMediaThumbnailUrl = '';
    postMediaPreviewUrl = '';
    selectedPostFile = null;
    if (postFileInput) postFileInput.value = '';
  }

  async function handlePostMediaChange(e: Event) {
    const input = e.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const isVideo = createPostType === 'videos';
    const ext = file.name.split('.').pop()?.toLowerCase() || '';
    const isAllowedVideo = file.type.startsWith('video/') || ['mp4', 'webm', 'mov', 'm4v'].includes(ext);
    const isAllowedImage = file.type.startsWith('image/') || ['jpg', 'jpeg', 'png', 'webp', 'gif'].includes(ext);
    if (isVideo && !isAllowedVideo) {
      uiStore.addToast({ title: 'Невірний формат', message: 'Оберіть відеофайл.', type: 'error' });
      input.value = '';
      return;
    }
    if (!isVideo && !isAllowedImage) {
      uiStore.addToast({ title: 'Невірний формат', message: 'Оберіть файл зображення.', type: 'error' });
      input.value = '';
      return;
    }

    const maxSize = isVideo ? MAX_VIDEO_SIZE_BYTES : MAX_IMAGE_SIZE_BYTES;
    if (file.size > maxSize) {
      uiStore.addToast({ title: 'Файл завеликий', message: `Максимальний розмір: ${Math.round(maxSize / (1024 * 1024))} МБ.`, type: 'error' });
      input.value = '';
      return;
    }

    if (postMediaPreviewUrl) URL.revokeObjectURL(postMediaPreviewUrl);
    postMediaPreviewUrl = URL.createObjectURL(file);
    postMediaUrl = '';
    postMediaThumbnailUrl = '';
    isUploadingMedia = true;
    try {
      selectedPostFile = file;
      const uploaded = await mediaService.upload(file);
      postMediaUrl = uploaded.url;
      if (isVideo) {
        postMediaThumbnailUrl = await mediaService.generateAndUploadVideoThumbnail(file);
      }
    } catch (err: any) {
      uiStore.addToast({ title: 'Помилка завантаження', message: err?.message || 'Не вдалося завантажити файл.', type: 'error' });
      resetPostMedia();
    } finally {
      isUploadingMedia = false;
    }
  }

  function removePostMedia() {
    resetPostMedia();
  }

  async function submitPost() {
    if (!selectedPostGame?.id) {
      uiStore.addToast({ title: 'Оберіть гру', message: 'Для створення публікації обов’язково оберіть гру.', type: 'warning' });
      isSelectGameModalOpen = true;
      return;
    }
    if (!postTitle.trim() || !postContent.trim()) {
      uiStore.addToast({ title: 'Заповніть поля', message: 'Вкажіть заголовок і текст публікації.', type: 'error' });
      return;
    }
    if (isUploadingMedia) {
      uiStore.addToast({ title: 'Зачекайте', message: 'Файл ще завантажується.', type: 'error' });
      return;
    }
    isSubmittingPost = true;
    try {
      await communityService.createPost(selectedPostGame.id, {
        category: createPostType,
        title: postTitle.trim(),
        content: postContent.trim(),
        gameId: selectedPostGame.id,
        mediaType: createPostType === 'videos' ? 'video' : (postMediaUrl ? 'image' : 'none'),
        mediaUrl: postMediaUrl,
        mediaThumbnailUrl: postMediaThumbnailUrl || undefined,
        file: selectedPostFile || null,
      });
      uiStore.addToast({ title: 'Успіх! 🎉', message: 'Публікацію створено.', type: 'success' });
      isCreatingPost = false;
      resetPostMedia();
      myProfileStore.reload();
    } catch (e: any) {
      uiStore.addToast({ title: 'Помилка', message: e?.message || 'Не вдалося опублікувати.', type: 'error' });
    } finally {
      isSubmittingPost = false;
    }
  }

  let isUploadingBanner = $state(false);
  let bannerFileInput: HTMLInputElement | undefined = $state();

  async function handleBannerFileChange(e: Event) {
    const input = e.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const bannerExt = file.name.split('.').pop()?.toLowerCase() || '';
    if (!file.type.startsWith('image/') && !['jpg', 'jpeg', 'png', 'webp', 'gif'].includes(bannerExt)) {
      uiStore.addToast({ title: 'Невірний формат', message: 'Банер має бути зображенням.', type: 'error' });
      input.value = '';
      return;
    }
    if (file.size > MAX_IMAGE_SIZE_BYTES) {
      uiStore.addToast({ title: 'Файл завеликий', message: `Максимальний розмір: ${Math.round(MAX_IMAGE_SIZE_BYTES / (1024 * 1024))} МБ.`, type: 'error' });
      input.value = '';
      return;
    }

    isUploadingBanner = true;
    try {
      const uploaded = await mediaService.upload(file);
      const updated = await userService.updateMyProfile({ bannerUrl: uploaded.url });
      authStore.patchUser({ bannerUrl: updated.bannerUrl });
      uiStore.addToast({ title: 'Банер оновлено', message: 'Новий банер збережено.', type: 'success' });
    } catch (err: any) {
      uiStore.addToast({ title: 'Помилка', message: err?.message || 'Не вдалося завантажити банер.', type: 'error' });
    } finally {
      isUploadingBanner = false;
      input.value = '';
    }
  }

  let isEditingProfile = $state(false);
  let editBio = $state('');
  let editAvatarUrl = $state('');
  let isSavingProfile = $state(false);

  function openEditProfile() {
    editBio = $currentUser?.bio || '';
    editAvatarUrl = $currentUser?.avatarUrl || '';
    isEditingProfile = true;
  }

  async function saveProfile() {
    isSavingProfile = true;
    try {
      const updated = await userService.updateMyProfile({ bio: editBio, avatarUrl: editAvatarUrl });
      authStore.patchUser({ bio: updated.bio, avatarUrl: updated.avatarUrl });
      uiStore.addToast({ title: 'Збережено', message: 'Профіль оновлено.', type: 'success' });
      isEditingProfile = false;
    } catch (e: any) {
      uiStore.addToast({ title: 'Помилка', message: e?.message || 'Не вдалося зберегти профіль.', type: 'error' });
    } finally {
      isSavingProfile = false;
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

{#if $currentUser}
<div class="min-h-screen bg-[#05181e] text-slate-200 font-sans pb-12">
  
  <div
    class="w-full h-48 md:h-64 relative bg-gradient-to-br from-[#0b4e63] via-[#03232c] to-[#05181e] bg-cover bg-center"
    style={$currentUser.bannerUrl ? `background-image: url('${$currentUser.bannerUrl}')` : ''}
  >
    <input
      bind:this={bannerFileInput}
      type="file"
      accept="image/*"
      class="hidden"
      onchange={handleBannerFileChange}
    />
    <button
      onclick={() => bannerFileInput?.click()}
      disabled={isUploadingBanner}
      class="absolute bottom-3 right-3 flex items-center gap-2 bg-black/50 hover:bg-black/70 text-white text-xs font-medium px-3 py-2 rounded-full backdrop-blur-sm transition-colors cursor-pointer disabled:opacity-60"
    >
      {#if isUploadingBanner}
        <Loader2 class="w-3.5 h-3.5 animate-spin" /> Завантаження...
      {:else}
        <Camera class="w-3.5 h-3.5" /> Змінити банер
      {/if}
    </button>
  </div>

  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
    
    <div class="flex flex-col md:flex-row justify-between items-start md:items-end -mt-16 md:-mt-20 mb-8 relative z-10 gap-4">
      <div class="flex flex-col md:flex-row gap-6 items-start md:items-end">
        <div class="w-32 h-32 md:w-40 md:h-40 rounded-full border-4 border-[#05181e] overflow-hidden bg-[#03232c] shrink-0">
          {#if $currentUser.avatarUrl}
            <img src={$currentUser.avatarUrl} alt={$currentUser.username} class="w-full h-full object-cover" />
          {:else}
            <div class="w-full h-full flex items-center justify-center text-4xl font-black text-white bg-gradient-to-tr from-cyan-500 to-blue-600">
              {$currentUser.username.charAt(0).toUpperCase()}
            </div>
          {/if}
        </div>
        <div class="pb-2">
          <h1 class="text-2xl font-bold text-white mb-1">{$currentUser.username}</h1>
          <p class="text-sm mb-3 {statusLabel($currentUser.status).color}">{statusLabel($currentUser.status).text}</p>
          <p class="text-sm text-slate-400 max-w-2xl leading-relaxed">
            {$currentUser.bio || 'Розкажіть про себе — додайте опис у налаштуваннях профілю.'}
          </p>
        </div>
      </div>

      
      <div class="pb-2 w-full md:w-auto">
        <button
          onclick={openEditProfile}
          class="flex items-center justify-center gap-2 bg-[#0b4e63] hover:bg-[#0d627a] text-white px-5 py-2.5 rounded-full text-sm font-medium transition-colors w-full md:w-auto cursor-pointer"
        >
          <Edit3 class="w-4 h-4" />
          Редагувати профіль
        </button>
      </div>
    </div>

    
    <div class="grid grid-cols-1 lg:grid-cols-[1fr_320px] gap-6">
      
      <div class="space-y-6">

        
        {#if activeTab === 'головна'}

          <div class="bg-[#03232c] border border-cyan-900/40 rounded-2xl p-6">
            <h2 class="text-lg font-bold text-white mb-4">Колекція ігор</h2>
            <div class="grid grid-cols-3 gap-4 mb-4">
              <div class="bg-[#02171d] p-4 rounded-xl text-center border border-cyan-900/30">
                <span class="text-xl font-bold text-white block">{gamesCount}</span>
                <span class="text-xs text-slate-400">Ігор</span>
              </div>
              <div class="bg-[#02171d] p-4 rounded-xl text-center border border-cyan-900/30">
                <span class="text-xl font-bold text-white block">{dlcCount}</span>
                <span class="text-xs text-slate-400">DLC</span>
              </div>
              <div class="bg-[#02171d] p-4 rounded-xl text-center border border-cyan-900/30">
                <span class="text-xl font-bold text-white block">{$wishlistStore.items.length}</span>
                <span class="text-xs text-slate-400">Бажаних</span>
              </div>
            </div>
            {#if $libraryStore.items.length === 0}
              <p class="text-sm text-slate-500 text-center py-6">Бібліотека поки порожня.</p>
            {:else}
              <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
                {#each $libraryStore.items.slice(0, 4) as item}
                  {#if item.game?.coverImageUrl}
                    <img src={item.game.coverImageUrl} alt={item.game.title} class="rounded-xl object-cover w-full h-24" />
                  {/if}
                {/each}
              </div>
            {/if}
          </div>

          <div class="bg-[#03232c] border border-cyan-900/40 rounded-2xl p-6 space-y-4">
            <div class="flex items-center justify-between gap-2">
              <div class="flex items-center gap-2">
                <Activity class="w-4 h-4 text-cyan-400" />
                <h2 class="text-lg font-bold text-white">Нещодавня активність</h2>
              </div>
              <span class="text-xs text-slate-400 font-mono">{myActivities.length} подій</span>
            </div>

            {#if myActivities.length === 0}
              <p class="text-sm text-slate-500 text-center py-6">У вас поки немає збережених активностей.</p>
            {:else}
              <div class="space-y-3">
                {#each myActivities as act (act.id)}
                  <ActivityCard activity={act} />
                {/each}
              </div>
            {/if}
          </div>
        {/if}

        
        {#if activeTab === 'ігри'}
          <div class="bg-[#03232c] border border-cyan-900/40 rounded-2xl p-6">
            {#if $libraryStore.items.length === 0}
              <p class="text-sm text-slate-500 text-center py-10">Бібліотека поки порожня.</p>
            {:else}
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {#each $libraryStore.items as item}
                  {#if item.game}
                    <div class="flex items-center gap-4 p-3 rounded-2xl bg-[#02171d] border border-cyan-900/30">
                      <img src={item.game.coverImageUrl || undefined} alt={item.game.title} class="w-16 h-20 object-cover rounded-xl bg-slate-800 shrink-0" />
                      <div class="min-w-0">
                        <h4 class="font-bold text-white text-sm truncate">{item.game.title}</h4>
                        {#if item.game.isDlc}
                          <span class="text-[10px] px-1.5 py-0.5 rounded bg-blue-500/20 text-blue-300 font-bold uppercase">DLC</span>
                        {/if}
                      </div>
                    </div>
                  {/if}
                {/each}
              </div>
            {/if}
          </div>
        {/if}

        
        {#if activeTab === 'бажане'}
          <div class="bg-[#03232c] border border-cyan-900/40 rounded-2xl p-6">
            {#if $wishlistStore.items.length === 0}
              <p class="text-sm text-slate-500 text-center py-10">Список бажаного порожній.</p>
            {:else}
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {#each $wishlistStore.items as item}
                  <div class="flex items-center gap-4 p-3 rounded-2xl bg-[#02171d] border border-cyan-900/30">
                    <img src={item.game.coverImageUrl || undefined} alt={item.game.title} class="w-16 h-20 object-cover rounded-xl bg-slate-800 shrink-0" />
                    <h4 class="font-bold text-white text-sm truncate">{item.game.title}</h4>
                  </div>
                {/each}
              </div>
            {/if}
          </div>
        {/if}

        
        {#if ['обговорення', 'скріншоти', 'відео', 'гайди', 'рецензії'].includes(activeTab)}
          <div class="bg-[#03232c] border border-cyan-900/40 rounded-2xl p-6">
            <div class="flex flex-col md:flex-row justify-between items-center gap-4 mb-6 text-sm text-slate-400">
              <h2 class="text-base font-bold text-white">
                {menuItems.find((m) => m.id === activeTab)?.label}
              </h2>

              {#if activeTab !== 'рецензії'}
                <div class="relative">
                  <button
                    onclick={() => showCreateDropdown = !showCreateDropdown}
                    class="bg-[#21e6c1] hover:bg-[#1cd1af] text-[#03232c] px-4 py-2 rounded-full font-bold text-xs flex items-center gap-1.5 transition-colors shadow-lg shadow-cyan-500/10 cursor-pointer"
                  >
                    <Plus class="w-3.5 h-3.5" /> Створити пост
                  </button>

                  {#if showCreateDropdown}
                    <div class="absolute right-0 mt-2 w-48 bg-[#02171d] border border-cyan-900/80 rounded-xl shadow-2xl py-2 z-50 text-xs text-slate-200">
                      <button onclick={() => triggerCreatePost('forum')} class="w-full text-left px-4 py-2 hover:bg-[#0b4e63] transition-colors flex items-center gap-2 cursor-pointer">💬 Обговорення</button>
                      <button onclick={() => triggerCreatePost('screenshots')} class="w-full text-left px-4 py-2 hover:bg-[#0b4e63] transition-colors flex items-center gap-2 cursor-pointer">🖼️ Скріншот</button>
                      <button onclick={() => triggerCreatePost('videos')} class="w-full text-left px-4 py-2 hover:bg-[#0b4e63] transition-colors flex items-center gap-2 cursor-pointer">🎬 Відео</button>
                      <button onclick={() => triggerCreatePost('guides')} class="w-full text-left px-4 py-2 hover:bg-[#0b4e63] transition-colors flex items-center gap-2 cursor-pointer">📖 Гайд</button>
                    </div>
                  {/if}
                </div>
              {/if}
            </div>

            {#if $myProfileStore.isLoadingPosts && activeTab !== 'рецензії'}
              <div class="flex justify-center py-16"><Loader2 class="w-8 h-8 text-cyan-400 animate-spin" /></div>
            {:else if activeTab === 'обговорення'}
              {#if myDiscussionPosts.length === 0}
                <p class="text-sm text-slate-500 text-center py-10">Ви ще не публікували обговорень.</p>
              {:else}
                <div class="space-y-4">
                  {#each myDiscussionPosts as post (post.id)}
                    <div class="bg-[#02171d] rounded-2xl p-5 border border-cyan-900/30">
                      <div class="flex items-center justify-between gap-2 mb-2">
                        <span class="text-xs text-slate-500">{formatDate(post.createdAt)}</span>
                        {#if (post as any).gameTitle}
                          <span class="text-[11px] font-bold px-2 py-0.5 rounded-md bg-cyan-950/60 text-cyan-300 border border-cyan-800/40 flex items-center gap-1">
                            <Gamepad2 class="w-3 h-3 text-cyan-400" />
                            {(post as any).gameTitle}
                          </span>
                        {/if}
                      </div>
                      <h3 class="text-lg font-bold text-white mb-2">{post.title}</h3>
                      <p class="text-sm text-slate-400 mb-3 whitespace-pre-line">{post.content}</p>
                      {#if post.media?.url && post.media?.type !== 'video'}
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
            {:else if activeTab === 'скріншоти'}
              {#if myScreenshotPosts.length === 0}
                <p class="text-sm text-slate-500 text-center py-10">Ви ще не публікували скріншотів.</p>
              {:else}
                <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
                  {#each myScreenshotPosts as post (post.id)}
                    <div class="aspect-[16/10] rounded-xl overflow-hidden cursor-pointer hover:opacity-80 transition-opacity bg-slate-800">
                      {#if post.media?.url}
                        <img src={post.media.url} alt={post.title} class="w-full h-full object-cover" />
                      {/if}
                    </div>
                  {/each}
                </div>
              {/if}
            {:else if activeTab === 'відео'}
              {#if myVideoPosts.length === 0}
                <p class="text-sm text-slate-500 text-center py-10">Ви ще не публікували відео.</p>
              {:else}
                <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
                  {#each myVideoPosts as post (post.id)}
                    <div class="aspect-[16/10] rounded-xl overflow-hidden relative cursor-pointer group bg-slate-800">
                      {#if post.media?.thumbnailUrl || post.media?.url}
                        <img src={post.media.thumbnailUrl || post.media.url} alt={post.title} class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />
                      {/if}
                      <div class="absolute inset-0 bg-black/30 flex items-center justify-center">
                        <div class="w-10 h-10 bg-white rounded-full flex items-center justify-center">
                          <svg class="w-5 h-5 text-black ml-1" viewBox="0 0 24 24" fill="currentColor"><path d="M8 5v14l11-7z"/></svg>
                        </div>
                      </div>
                    </div>
                  {/each}
                </div>
              {/if}
            {:else if activeTab === 'гайди'}
              {#if myGuidePosts.length === 0}
                <p class="text-sm text-slate-500 text-center py-10">Ви ще не публікували гайдів.</p>
              {:else}
                <div class="space-y-4">
                  {#each myGuidePosts as post (post.id)}
                    <div class="bg-[#02171d] rounded-2xl p-5 border border-cyan-900/30 flex gap-4">
                      {#if post.media?.url}
                        <img src={post.media.url} alt={post.title} class="w-40 h-28 object-cover rounded-xl shrink-0 bg-slate-800" />
                      {/if}
                      <div>
                        <h3 class="text-base font-bold text-white mb-2">{post.title}</h3>
                        <p class="text-xs text-slate-400 line-clamp-3 whitespace-pre-line">{post.content}</p>
                      </div>
                    </div>
                  {/each}
                </div>
              {/if}
            {:else if activeTab === 'рецензії'}
              {#if $myProfileStore.isLoadingReviews}
                <div class="flex justify-center py-16"><Loader2 class="w-8 h-8 text-cyan-400 animate-spin" /></div>
              {:else if $myProfileStore.reviews.length === 0}
                <p class="text-sm text-slate-500 text-center py-10">Ви ще не залишали рецензій.</p>
              {:else}
                <div class="space-y-4">
                  {#each $myProfileStore.reviews as review (review.id)}
                    <div class="bg-[#02171d] rounded-2xl p-5 border border-cyan-900/30">
                      {#if review.gameCoverImageUrl}
                        <img src={review.gameCoverImageUrl} alt={review.gameTitle} class="w-full h-48 object-cover rounded-xl mb-4" />
                      {/if}
                      <h3 class="text-lg font-bold text-white mb-1">{review.gameTitle}</h3>
                      <div class="flex gap-1 text-rose-500 mb-3">
                        {#each Array(5) as _, i}
                          <Star class="w-4 h-4 {i < review.rating ? 'fill-rose-500' : 'text-slate-600'}" />
                        {/each}
                      </div>
                      <p class="text-xs text-slate-400 leading-relaxed whitespace-pre-line">{review.content}</p>
                      <span class="block text-[11px] text-slate-500 mt-3">{formatDate(review.createdAt)}</span>
                    </div>
                  {/each}
                </div>
              {/if}
            {/if}
          </div>
        {/if}

      </div>

      
      <div class="space-y-6">
        
        <div class="bg-[#03232c] border border-cyan-900/40 rounded-2xl p-4">
          <nav class="space-y-1">
            {#each menuItems as item}
              <button
                onclick={() => { activeTab = item.id; showCreateDropdown = false; }}
                class="w-full flex items-center justify-between px-4 py-2.5 rounded-xl text-sm transition-colors cursor-pointer {activeTab === item.id ? 'bg-[#0b4e63] text-white font-medium' : 'text-slate-300 hover:bg-cyan-900/30'}"
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
            <span class="font-medium text-white">Друзі</span>
            <span class="bg-[#0b4e63] px-2.5 py-0.5 rounded-full text-xs text-white">{uniqueFriends.length}</span>
          </div>
          {#if uniqueFriends.length === 0}
            <p class="text-xs text-slate-500 px-2 py-2">Список друзів порожній.</p>
          {:else}
            <div class="space-y-2">
              {#each uniqueFriends as f (f.id)}
                <button
                  onclick={() => profileStore.viewProfile(f.id)}
                  class="w-full flex items-center justify-between px-2 cursor-pointer hover:bg-cyan-900/20 p-2 rounded-xl transition-colors text-left"
                >
                  <div class="flex items-center gap-3">
                    {#if f.avatarUrl}
                      <img src={f.avatarUrl} alt={f.username} class="w-8 h-8 rounded-full object-cover bg-slate-800 border border-cyan-900/60" />
                    {:else}
                      <div class="w-8 h-8 rounded-full bg-cyan-950/80 text-cyan-300 font-bold flex items-center justify-center text-xs border border-cyan-500/30">
                        {f.username.charAt(0).toUpperCase()}
                      </div>
                    {/if}
                    <span class="text-xs font-medium text-slate-200 truncate max-w-[110px]">{f.username}</span>
                  </div>
                </button>
              {/each}
            </div>
          {/if}
        </div>
      </div>
    </div>
  </div>
</div>


{#if isCreatingPost}
  <div class="fixed inset-0 bg-black/70 flex items-center justify-center z-[100] p-4" onclick={(e) => { if (e.target === e.currentTarget) { isCreatingPost = false; resetPostMedia(); } }}>
    <div class="bg-[#092635] border border-cyan-500/30 rounded-3xl p-6 w-full max-w-lg space-y-4">
      <div class="flex items-center justify-between">
        <h3 class="text-lg font-black text-white">Створити: {createTypeLabels[createPostType]}</h3>
        <button onclick={() => { isCreatingPost = false; resetPostMedia(); }} class="text-slate-400 hover:text-white cursor-pointer"><X class="w-5 h-5" /></button>
      </div>

      <!-- Game Selector for Profile Post -->
      <div class="p-3 rounded-2xl bg-[#02171d] border border-cyan-900/60 flex items-center justify-between gap-3 shadow-inner">
        <div class="flex items-center gap-3 min-w-0">
          {#if selectedPostGame}
            <div class="w-11 h-8 rounded-lg overflow-hidden bg-slate-900 shrink-0 border border-cyan-400/40 relative">
              {#if selectedPostGame.bannerUrl}
                <img src={selectedPostGame.bannerUrl} alt={selectedPostGame.title} class="w-full h-full object-cover" />
              {:else}
                <div class="w-full h-full bg-gradient-to-tr from-cyan-950 to-slate-900 flex items-center justify-center text-cyan-400">
                  <Gamepad2 class="w-4 h-4" />
                </div>
              {/if}
            </div>
            <div class="min-w-0">
              <span class="text-[10px] text-cyan-400 font-bold uppercase tracking-wider block">Підв'язано до гри</span>
              <span class="text-xs font-bold text-white truncate block">{selectedPostGame.title}</span>
            </div>
          {:else}
            <div class="w-8 h-8 rounded-lg bg-cyan-950/60 border border-amber-500/40 flex items-center justify-center text-amber-400 shrink-0">
              <Gamepad2 class="w-4 h-4" />
            </div>
            <div>
              <span class="text-xs font-bold text-amber-300 block">Гру не обрано *</span>
              <span class="text-[10px] text-slate-400">Оберіть гру для створення допису</span>
            </div>
          {/if}
        </div>

        <button
          type="button"
          onclick={() => isSelectGameModalOpen = true}
          class="px-3 py-1.5 rounded-xl bg-cyan-500/15 hover:bg-cyan-500/25 border border-cyan-400/40 hover:border-cyan-400 text-cyan-300 hover:text-white text-xs font-bold transition-all cursor-pointer shrink-0"
        >
          {selectedPostGame ? 'Змінити' : 'Обрати'}
        </button>
      </div>

      <input
        type="text"
        bind:value={postTitle}
        placeholder="Заголовок"
        class="w-full bg-[#02171d] border border-cyan-900/60 rounded-xl px-4 py-2.5 text-sm text-white focus:outline-none focus:border-cyan-500"
      />
      <textarea
        bind:value={postContent}
        placeholder="Текст публікації..."
        rows="4"
        class="w-full bg-[#02171d] border border-cyan-900/60 rounded-xl px-4 py-2.5 text-sm text-white focus:outline-none focus:border-cyan-500 resize-none"
      ></textarea>
      {#if createPostType !== 'forum'}
        <div>
          <input
            bind:this={postFileInput}
            type="file"
            accept={createPostType === 'videos' ? 'video/*' : 'image/*'}
            class="hidden"
            onchange={handlePostMediaChange}
          />
          {#if postMediaPreviewUrl}
            <div class="relative rounded-xl overflow-hidden border border-cyan-900/60 bg-[#02171d]">
              {#if createPostType === 'videos'}
                
                <video src={postMediaPreviewUrl} class="w-full max-h-56 object-cover" muted controls></video>
              {:else}
                <img src={postMediaPreviewUrl} alt="" class="w-full max-h-56 object-cover" />
              {/if}
              {#if isUploadingMedia}
                <div class="absolute inset-0 bg-black/60 flex items-center justify-center gap-2 text-white text-sm">
                  <Loader2 class="w-4 h-4 animate-spin" /> Завантаження...
                </div>
              {/if}
              <button
                type="button"
                onclick={removePostMedia}
                class="absolute top-2 right-2 bg-black/60 hover:bg-black/80 text-white p-1.5 rounded-full transition-colors cursor-pointer"
              >
                <X class="w-3.5 h-3.5" />
              </button>
            </div>
          {:else}
            <button
              type="button"
              onclick={() => postFileInput?.click()}
              class="w-full flex flex-col items-center justify-center gap-2 border-2 border-dashed border-cyan-900/60 hover:border-cyan-500/60 rounded-xl py-6 text-slate-400 hover:text-cyan-300 transition-colors cursor-pointer"
            >
              <ImagePlus class="w-6 h-6" />
              <span class="text-xs font-medium">
                {createPostType === 'videos' ? 'Обрати відеофайл' : 'Обрати зображення'}
              </span>
            </button>
          {/if}
        </div>
      {/if}
      <button
        onclick={submitPost}
        disabled={isSubmittingPost || isUploadingMedia}
        class="w-full bg-[#21e6c1] hover:bg-[#1cd1af] text-[#03232c] font-bold py-2.5 rounded-xl transition-colors disabled:opacity-50 cursor-pointer"
      >
        {isSubmittingPost ? 'Публікація...' : isUploadingMedia ? 'Зачекайте...' : 'Опублікувати'}
      </button>
    </div>
  </div>
{/if}


{#if isEditingProfile}
  <div class="fixed inset-0 bg-black/70 flex items-center justify-center z-[100] p-4" onclick={(e) => { if (e.target === e.currentTarget) isEditingProfile = false; }}>
    <div class="bg-[#092635] border border-cyan-500/30 rounded-3xl p-6 w-full max-w-lg space-y-4">
      <div class="flex items-center justify-between">
        <h3 class="text-lg font-black text-white">Редагувати профіль</h3>
        <button onclick={() => isEditingProfile = false} class="text-slate-400 hover:text-white cursor-pointer"><X class="w-5 h-5" /></button>
      </div>
      <div>
        <label for="edit-avatar" class="block text-xs text-slate-400 mb-1.5">URL аватара</label>
        <input
          id="edit-avatar"
          type="text"
          bind:value={editAvatarUrl}
          placeholder="https://..."
          class="w-full bg-[#02171d] border border-cyan-900/60 rounded-xl px-4 py-2.5 text-sm text-white focus:outline-none focus:border-cyan-500"
        />
      </div>
      <div>
        <label for="edit-bio" class="block text-xs text-slate-400 mb-1.5">Про себе</label>
        <textarea
          id="edit-bio"
          bind:value={editBio}
          rows="3"
          maxlength="300"
          placeholder="Розкажіть щось про себе..."
          class="w-full bg-[#02171d] border border-cyan-900/60 rounded-xl px-4 py-2.5 text-sm text-white focus:outline-none focus:border-cyan-500 resize-none"
        ></textarea>
      </div>
      <button
        onclick={saveProfile}
        disabled={isSavingProfile}
        class="w-full bg-[#0b4e63] hover:bg-[#0d627a] text-white font-bold py-2.5 rounded-xl transition-colors disabled:opacity-50 cursor-pointer"
      >
        {isSavingProfile ? 'Збереження...' : 'Зберегти'}
      </button>
    </div>
  </div>
{/if}
{/if}

<SelectGameModal
  isOpen={isSelectGameModalOpen}
  selectedGameId={selectedPostGame?.id}
  onSelect={(game) => selectedPostGame = game}
  onClose={() => isSelectGameModalOpen = false}
/>
