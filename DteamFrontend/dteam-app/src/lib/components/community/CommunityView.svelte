<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { communityService, type CommunityPost } from '../../services/communityService';
  import { mediaService, ALLOWED_IMAGE_TYPES, ALLOWED_VIDEO_TYPES, MAX_IMAGE_SIZE_BYTES, MAX_VIDEO_SIZE_BYTES } from '../../services/mediaService';
  import { uiStore } from '../../stores/uiStore';
  import { profileStore } from '../../stores/profileStore';
  import { ThumbsUp, MessageSquare, Loader2, Gamepad2, CornerDownRight, Send, X, ZoomIn, Palette } from 'lucide-svelte';
  import { onlineHubService } from '../../services/onlineHubService';
  import SelectGameModal from './SelectGameModal.svelte';
  import MediaLightboxModal from '../ui/MediaLightboxModal.svelte';
  import type { CommunityComment } from '../../services/communityService';
  import { renderDecoratedText, resolveMediaUrl } from '../../utils/textDecorator';

  interface Props {
    gameId?: string | null;
    gameName?: string;
  }

  let {
    gameId = null,
    gameName = "Онлайн людей на сайті"
  }: Props = $props();

  let selectedGame = $state<{ id: string; title: string; bannerUrl?: string } | null>(null);
  let isSelectGameModalOpen = $state(false);

  $effect(() => {
    if (gameId && (!selectedGame || selectedGame.id !== gameId)) {
      selectedGame = { id: gameId, title: gameName || 'Обрана гра' };
    }
  });

  let onlineCount = $state(onlineHubService.getOnlineCount());

  type TabType = 'discussion' | 'artwork' | 'screenshot' | 'video' | 'guide';
  let activeTab = $state<TabType>('discussion');

  let title = $state('');
  let description = $state('');
  let content = $state('');
  let caption = $state('');
  let mediaUrl = $state('');
  let mediaThumbnailUrl = $state('');
  let mediaPreviewUrl = $state('');
  let isSubmitting = $state(false);

  let isUploadingMedia = $state(false);
  let isDraggingOver = $state(false);
  let fileInputEl: HTMLInputElement | undefined = $state();
  let selectedFile = $state<File | null>(null);

  let contentTextareaEl: HTMLTextAreaElement | undefined = $state();

  let openCommentsPostId = $state<string | null>(null);
  let postCommentsMap = $state<Record<string, CommunityComment[]>>({});
  let loadingCommentsPostId = $state<string | null>(null);
  let newCommentText = $state<Record<string, string>>({});
  let replyingToCommentId = $state<string | null>(null);
  let replyText = $state('');
  let isSubmittingReply = $state(false);
  let isSubmittingComment = $state(false);
  let lightboxMedia = $state<{ src: string; title?: string; author?: { username: string; avatarUrl?: string } } | null>(null);

  function setTab(tab: TabType) {
    activeTab = tab;
    title = '';
    description = '';
    content = '';
    caption = '';
    mediaUrl = '';
    mediaThumbnailUrl = '';
    if (mediaPreviewUrl) URL.revokeObjectURL(mediaPreviewUrl);
    mediaPreviewUrl = '';
    selectedFile = null;
    isDraggingOver = false;
  }

  function expectedMediaKind(): 'image' | 'video' {
    return activeTab === 'video' ? 'video' : 'image';
  }

  function openFilePicker() {
    if (isUploadingMedia) return;
    fileInputEl?.click();
  }

  function validateFile(file: File, kind: 'image' | 'video'): string | null {
    const ext = file.name.split('.').pop()?.toLowerCase() || '';
    if (kind === 'image') {
      const allowedExts = ['jpg', 'jpeg', 'png', 'webp', 'gif'];
      const isValid = allowedExts.includes(ext) || file.type.startsWith('image/') || ALLOWED_IMAGE_TYPES.includes(file.type);
      if (!isValid) {
        return 'Непідтримуваний формат зображення. Дозволені: JPG, PNG, WEBP, GIF.';
      }
      if (file.size > MAX_IMAGE_SIZE_BYTES) {
        return `Розмір зображення перевищує ліміт ${MAX_IMAGE_SIZE_BYTES / (1024 * 1024)} МБ.`;
      }
    } else {
      const allowedExts = ['mp4', 'webm', 'mov', 'm4v'];
      const isValid = allowedExts.includes(ext) || file.type.startsWith('video/') || ALLOWED_VIDEO_TYPES.includes(file.type);
      if (!isValid) {
        return 'Непідтримуваний формат відео. Дозволені: MP4, WEBM, MOV.';
      }
      if (file.size > MAX_VIDEO_SIZE_BYTES) {
        return `Розмір відео перевищує ліміт ${MAX_VIDEO_SIZE_BYTES / (1024 * 1024)} МБ.`;
      }
    }
    return null;
  }

  async function uploadFile(file: File) {
    const kind = expectedMediaKind();
    const validationError = validateFile(file, kind);
    if (validationError) {
      uiStore.addToast({ title: 'Помилка файлу', message: validationError, type: 'warning' });
      return;
    }

    if (mediaPreviewUrl) URL.revokeObjectURL(mediaPreviewUrl);
    mediaPreviewUrl = URL.createObjectURL(file);
    selectedFile = file;
    isUploadingMedia = true;
    try {
      const uploaded = await communityService.uploadMedia(file);
      mediaUrl = uploaded.url;

      if (kind === 'video') {
        try {
          mediaThumbnailUrl = await mediaService.generateAndUploadVideoThumbnail(file);
        } catch {
          mediaThumbnailUrl = '';
        }
      } else {
        mediaThumbnailUrl = uploaded.url;
      }

      uiStore.addToast({
        title: 'Завантаження',
        message: kind === 'image' ? 'Зображення завантажено успішно!' : 'Відео завантажено успішно!',
        type: 'success'
      });
    } catch (err: any) {
      console.error('[CommunityView] Помилка завантаження файлу:', err);
      uiStore.addToast({
        title: 'Помилка завантаження',
        message: err?.message || 'Не вдалося завантажити файл.',
        type: 'error'
      });
      if (mediaPreviewUrl) URL.revokeObjectURL(mediaPreviewUrl);
      mediaPreviewUrl = '';
      mediaUrl = '';
      selectedFile = null;
    } finally {
      isUploadingMedia = false;
    }
  }

  function handleFileInputChange(e: Event) {
    const input = e.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      uploadFile(file);
    }
    input.value = '';
  }

  function handleDrop(e: DragEvent) {
    e.preventDefault();
    isDraggingOver = false;
    const file = e.dataTransfer?.files?.[0];
    if (file) {
      uploadFile(file);
    }
  }

  function handleDragOver(e: DragEvent) {
    e.preventDefault();
    isDraggingOver = true;
  }

  function handleDragLeave(e: DragEvent) {
    e.preventDefault();
    isDraggingOver = false;
  }

  const feedCategoryLabels: Record<'all' | 'forum' | 'artwork' | 'screenshots' | 'videos' | 'guides', string> = {
    all: 'Усі',
    forum: 'Дискусія',
    artwork: 'Творчі роботи',
    screenshots: 'Скріншот',
    videos: 'Відео',
    guides: 'Гайд',
  };
  const feedCategories = Object.keys(feedCategoryLabels) as Array<keyof typeof feedCategoryLabels>;

  let posts = $state<CommunityPost[]>([]);
  let isLoadingPosts = $state(false);
  let activeFeedCategory = $state<keyof typeof feedCategoryLabels>('all');

  async function loadPosts() {
    isLoadingPosts = true;
    try {
      const res = await communityService.getPosts(gameId, activeFeedCategory);
      posts = res.posts;
    } catch (e) {
      console.warn('[CommunityView] Не вдалося завантажити пости спільноти:', e);
    } finally {
      isLoadingPosts = false;
    }
  }

  function setFeedCategory(category: keyof typeof feedCategoryLabels) {
    activeFeedCategory = category;
    loadPosts();
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
    loadPosts();

    const unsubscribeOnline = onlineHubService.onOnlineCountChanged((count) => {
      onlineCount = count;
    });

    onDestroy(unsubscribeOnline);
  });

  function escapeHtml(text: string): string {
    return text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function renderPostContent(raw: string): string {
    return renderDecoratedText(raw);
  }

  function applyFormatting(format: 'bold' | 'italic' | 'underline' | 'image') {
    if (!contentTextareaEl) return;

    const formats = {
      bold: { start: '**', end: '**' },
      italic: { start: '*', end: '*' },
      underline: { start: '<u>', end: '</u>' },
      image: { start: '![опис](', end: ')' }
    };
    const chunk = formats[format];

    const start = contentTextareaEl.selectionStart;
    const end = contentTextareaEl.selectionEnd;
    const selectedText = content.substring(start, end);

    const replacement = `${chunk.start}${selectedText || 'текст'}${chunk.end}`;

    content = content.substring(0, start) + replacement + content.substring(end);

    setTimeout(() => {
      if (!contentTextareaEl) return;
      contentTextareaEl.focus();
      const newCursorStart = start + chunk.start.length;
      const newCursorEnd = selectedText ? newCursorStart + selectedText.length : newCursorStart + 5;
      contentTextareaEl.setSelectionRange(newCursorStart, newCursorEnd);
    }, 0);
  }

  async function handleSubmit(e?: Event) {
    if (e) e.preventDefault();

    if (!selectedGame?.id) {
      uiStore.addToast({
        title: 'Оберіть гру',
        message: 'Для публікації допису обов’язково оберіть гру зі списку.',
        type: 'warning'
      });
      isSelectGameModalOpen = true;
      return;
    }

    if ((activeTab === 'discussion' || activeTab === 'guide') && !title.trim()) {
      uiStore.addToast({
        title: 'Помилка валідації',
        message: 'Заголовок обов’язковий для заповнення.',
        type: 'warning'
      });
      return;
    }

    isSubmitting = true;

    const categoryMap: Record<TabType, string> = {
      discussion: 'forum',
      artwork: 'artwork',
      screenshot: 'screenshots',
      video: 'videos',
      guide: 'guides'
    };

    const finalTitle = activeTab === 'screenshot' || activeTab === 'video'
      ? (caption || `${activeTab.toUpperCase()} post`)
      : (title || caption || 'Творча робота');

    const finalContent = activeTab === 'guide'
      ? `${description}\n\n${content}`
      : (content || caption || 'Без опису');

    const postPayload = {
      category: categoryMap[activeTab],
      title: finalTitle,
      content: finalContent,
      gameId: selectedGame.id,
      mediaType: mediaUrl ? (activeTab === 'video' ? 'video' : 'image') : 'none',
      mediaUrl,
      mediaThumbnailUrl: mediaThumbnailUrl || undefined,
      file: selectedFile || null
    };

    try {
      await communityService.createPost(selectedGame.id, postPayload);
      uiStore.addToast({
        title: 'Успіх! 🎉',
        message: 'Пост успішно опубліковано!',
        type: 'success'
      });

      setTab('discussion');
      loadPosts();
    } catch (error: any) {
      console.error('Failed to create post:', error);
      uiStore.addToast({
        title: 'Помилка при публікації',
        message: error?.message || 'Не вдалося створити пост.',
        type: 'error'
      });
    } finally {
      isSubmitting = false;
    }
  }

  function handleCancel() {
    setTab('discussion');
  }
</script>

<input
  bind:this={fileInputEl}
  type="file"
  accept={expectedMediaKind() === 'video' ? 'video/*,video/mp4,video/webm,video/quicktime' : 'image/*,image/jpeg,image/png,image/webp,image/gif'}
  class="hidden"
  onchange={handleFileInputChange}
/>

<div class="min-h-screen bg-[#05181e] text-slate-100 p-4 md:p-8 flex flex-col items-center w-full">

  <h1 class="text-3xl font-black mb-6 tracking-wide text-white font-display">Створення публікації</h1>

  <div class="w-full max-w-7xl grid grid-cols-1 lg:grid-cols-4 gap-6 items-start">

    <div class="lg:col-span-3 bg-[#03232c] border border-cyan-900/60 rounded-2xl p-6 shadow-2xl flex flex-col justify-between min-h-[600px]">

      <div>

        <div class="mb-6 p-4 rounded-2xl bg-[#02171d]/90 border border-cyan-900/60 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 shadow-inner">
          <div class="flex items-center gap-3.5 min-w-0">
            {#if selectedGame}
              <div class="w-14 h-10 rounded-xl overflow-hidden bg-slate-900 shrink-0 border border-cyan-400/40 relative">
                {#if selectedGame.bannerUrl}
                  <img src={selectedGame.bannerUrl} alt={selectedGame.title} class="w-full h-full object-cover" />
                {:else}
                  <div class="w-full h-full bg-gradient-to-tr from-cyan-950 to-slate-900 flex items-center justify-center text-cyan-400">
                    <Gamepad2 class="w-5 h-5" />
                  </div>
                {/if}
              </div>
              <div class="min-w-0">
                <span class="text-[10px] text-cyan-400 font-bold uppercase tracking-wider block">Підв'язано до гри</span>
                <h3 class="text-sm sm:text-base font-bold text-white truncate">{selectedGame.title}</h3>
              </div>
            {:else}
              <div class="w-11 h-11 rounded-2xl bg-cyan-950/60 border border-amber-500/40 flex items-center justify-center text-amber-400 shrink-0">
                <Gamepad2 class="w-5 h-5" />
              </div>
              <div>
                <span class="text-xs font-bold text-amber-300 flex items-center gap-1.5">
                  Оберіть гру для допису <span class="text-rose-400">*</span>
                </span>
                <span class="text-[11px] text-slate-400">Публікація обов'язково має належати до певної гри</span>
              </div>
            {/if}
          </div>

          <button
            type="button"
            onclick={() => isSelectGameModalOpen = true}
            class="w-full sm:w-auto px-4 py-2 rounded-xl bg-cyan-500/15 hover:bg-cyan-500/25 border border-cyan-400/50 hover:border-cyan-400 text-cyan-300 hover:text-white text-xs font-bold transition-all cursor-pointer shrink-0 shadow-sm text-center"
          >
            {selectedGame ? 'Змінити гру' : 'Обрати гру'}
          </button>
        </div>

        <div class="grid grid-cols-2 sm:grid-cols-5 gap-2 mb-6">
          <button
            type="button"
            onclick={() => setTab('discussion')}
            class="py-2.5 rounded-xl font-bold transition-all duration-200 text-center text-xs md:text-sm cursor-pointer {activeTab === 'discussion' ? 'bg-[#0b4e63] text-white shadow-md' : 'text-slate-400 hover:text-white hover:bg-[#0b4e63]/20'}"
          >
            Обговорення
          </button>

          <button
            type="button"
            onclick={() => setTab('artwork')}
            class="py-2.5 rounded-xl font-bold transition-all duration-200 text-center text-xs md:text-sm cursor-pointer {activeTab === 'artwork' ? 'bg-[#0b4e63] text-white shadow-md' : 'text-slate-400 hover:text-white hover:bg-[#0b4e63]/20'}"
          >
            Творчі роботи
          </button>

          <button
            type="button"
            onclick={() => setTab('screenshot')}
            class="py-2.5 rounded-xl font-bold transition-all duration-200 text-center text-xs md:text-sm cursor-pointer {activeTab === 'screenshot' ? 'bg-[#0b4e63] text-white shadow-md' : 'text-slate-400 hover:text-white hover:bg-[#0b4e63]/20'}"
          >
            Скріншот
          </button>

          <button
            type="button"
            onclick={() => setTab('video')}
            class="py-2.5 rounded-xl font-bold transition-all duration-200 text-center text-xs md:text-sm cursor-pointer {activeTab === 'video' ? 'bg-[#0b4e63] text-white shadow-md' : 'text-slate-400 hover:text-white hover:bg-[#0b4e63]/20'}"
          >
            Відео
          </button>

          <button
            type="button"
            onclick={() => setTab('guide')}
            class="py-2.5 rounded-xl font-bold transition-all duration-200 text-center text-xs md:text-sm cursor-pointer {activeTab === 'guide' ? 'bg-[#0b4e63] text-white shadow-md' : 'text-slate-400 hover:text-white hover:bg-[#0b4e63]/20'}"
          >
            Гайд
          </button>
        </div>

        {#if activeTab === 'discussion'}
          <div class="space-y-5">
            <div>
              <div class="flex justify-between text-xs text-slate-400 mb-1.5 font-bold">
                <label for="title">Заголовок</label>
                <span>{title.length}/160</span>
              </div>
              <input
                id="title"
                type="text"
                maxlength="160"
                bind:value={title}
                placeholder="Тема вашого обговорення..."
                class="w-full bg-[#02171d] border border-cyan-900/60 rounded-xl px-5 py-3 text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-cyan-500 transition-colors"
              />
            </div>

            <div>
              <label for="content" class="block text-xs text-slate-400 mb-1.5 font-bold">Текст</label>
              <div class="bg-[#02171d] border border-cyan-900/60 rounded-xl overflow-hidden focus-within:border-cyan-500 transition-colors">
                <div class="flex items-center gap-3 px-4 py-2.5 border-b border-cyan-900/40 text-slate-300">
                  <button type="button" onclick={() => applyFormatting('bold')} class="font-bold hover:text-white px-1 cursor-pointer">B</button>
                  <button type="button" onclick={() => applyFormatting('italic')} class="italic hover:text-white px-1 cursor-pointer">I</button>
                  <button type="button" onclick={() => applyFormatting('underline')} class="underline hover:text-white px-1 cursor-pointer">U</button>
                  <button type="button" onclick={() => applyFormatting('image')} aria-label="Вставити зображення" title="Вставити зображення" class="hover:text-white px-1 cursor-pointer">
                    <svg class="w-4 h-4 inline" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>
                  </button>
                </div>
                <textarea
                  bind:this={contentTextareaEl}
                  id="content"
                  rows="5"
                  bind:value={content}
                  placeholder="Що ви хочете обговорити?"
                  class="w-full bg-transparent px-4 py-3 text-sm text-slate-100 placeholder-slate-500 focus:outline-none resize-none"
                ></textarea>
              </div>
            </div>

            <button
              type="button"
              onclick={openFilePicker}
              ondragover={handleDragOver}
              ondragleave={handleDragLeave}
              ondrop={handleDrop}
              disabled={isUploadingMedia}
              class="w-full border-2 border-dashed rounded-2xl p-8 flex flex-col items-center justify-center bg-[#02171d]/50 hover:bg-[#02171d] transition-colors cursor-pointer disabled:cursor-wait
                {isDraggingOver ? 'border-cyan-400 bg-[#02171d]' : 'border-cyan-900/60'}"
            >
              {#if isUploadingMedia}
                <Loader2 class="w-6 h-6 text-cyan-400 animate-spin mb-2" />
                <p class="text-xs text-slate-400">Завантаження...</p>
              {:else if mediaPreviewUrl || mediaUrl}
                <img src={mediaPreviewUrl || mediaUrl} alt="Uploaded" class="max-h-32 rounded-lg object-cover mb-2" />
                <p class="text-xs text-cyan-400">Файл успішно обрано</p>
              {:else}
                <p class="text-sm text-slate-400 mb-3">Перетягніть файл сюди або</p>
                <span class="bg-[#0b4e63] hover:bg-[#0d6e8a] text-white font-bold text-xs px-5 py-2.5 rounded-xl transition-colors shadow-md">
                  Завантажити
                </span>
              {/if}
            </button>
          </div>
        {/if}

        {#if activeTab === 'artwork'}
          <div class="space-y-5">
            <button
              type="button"
              onclick={openFilePicker}
              ondragover={handleDragOver}
              ondragleave={handleDragLeave}
              ondrop={handleDrop}
              disabled={isUploadingMedia}
              class="w-full border-2 border-dashed rounded-2xl p-14 flex flex-col items-center justify-center bg-[#02171d]/50 hover:bg-[#02171d] transition-colors cursor-pointer disabled:cursor-wait
                {isDraggingOver ? 'border-cyan-400 bg-[#02171d]' : 'border-cyan-900/60'}"
            >
              {#if isUploadingMedia}
                <Loader2 class="w-6 h-6 text-cyan-400 animate-spin mb-2" />
                <p class="text-xs text-slate-400">Завантаження арту...</p>
              {:else if mediaPreviewUrl || mediaUrl}
                <img src={mediaPreviewUrl || mediaUrl} alt="Uploaded Artwork" class="max-h-48 rounded-lg object-cover mb-2" />
                <p class="text-xs text-cyan-400">Арт успішно обрано</p>
              {:else}
                <p class="text-sm text-slate-400 mb-3">Перетягніть файл арту сюди або</p>
                <span class="bg-[#0b4e63] hover:bg-[#0d6e8a] text-white font-bold text-xs px-6 py-2.5 rounded-xl transition-colors shadow-md">
                  Завантажити арт
                </span>
              {/if}
            </button>

            <div>
              <label for="artwork-title" class="block text-xs text-slate-400 mb-1.5 font-bold">Назва роботи</label>
              <input
                id="artwork-title"
                type="text"
                bind:value={title}
                placeholder="Наприклад: Концепт головного герою..."
                class="w-full bg-[#02171d] border border-cyan-900/60 rounded-xl px-4 py-2.5 text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-cyan-500 transition-colors"
              />
            </div>

            <div>
              <label for="artwork-desc" class="block text-xs text-slate-400 mb-1.5 font-bold">Опис роботи</label>
              <input
                id="artwork-desc"
                type="text"
                bind:value={caption}
                placeholder="Короткий опис, інструменти (Photoshop, Blender тощо)..."
                class="w-full bg-[#02171d] border border-cyan-900/60 rounded-xl px-4 py-2.5 text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-cyan-500 transition-colors"
              />
            </div>
          </div>
        {/if}

        {#if activeTab === 'screenshot'}
          <div class="space-y-5">

            <button
              type="button"
              onclick={openFilePicker}
              ondragover={handleDragOver}
              ondragleave={handleDragLeave}
              ondrop={handleDrop}
              disabled={isUploadingMedia}
              class="w-full border-2 border-dashed rounded-2xl p-16 flex flex-col items-center justify-center bg-[#02171d]/50 hover:bg-[#02171d] transition-colors cursor-pointer disabled:cursor-wait
                {isDraggingOver ? 'border-cyan-400 bg-[#02171d]' : 'border-cyan-900/60'}"
            >
              {#if isUploadingMedia}
                <Loader2 class="w-6 h-6 text-cyan-400 animate-spin mb-2" />
                <p class="text-xs text-slate-400">Завантаження...</p>
              {:else if mediaPreviewUrl || mediaUrl}
                <img src={mediaPreviewUrl || mediaUrl} alt="Uploaded Screenshot" class="max-h-48 rounded-lg object-cover mb-2" />
                <p class="text-xs text-cyan-400">Зображення успішно обрано</p>
              {:else}
                <p class="text-sm text-slate-400 mb-3">Перетягніть файл сюди або</p>
                <span class="bg-[#0b4e63] hover:bg-[#0d6e8a] text-white font-bold text-xs px-6 py-2.5 rounded-xl transition-colors shadow-md">
                  Завантажити
                </span>
              {/if}
            </button>

            <div>
              <label for="caption-ss" class="block text-xs text-slate-400 mb-1.5 font-bold">Підпис</label>
              <input
                id="caption-ss"
                type="text"
                bind:value={caption}
                placeholder="Ваш коментар до скріншота..."
                class="w-full bg-[#02171d] border border-cyan-900/60 rounded-full px-5 py-3 text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-cyan-500 transition-colors"
              />
            </div>
          </div>
        {/if}

        {#if activeTab === 'video'}
          <div class="space-y-5">

            <button
              type="button"
              onclick={openFilePicker}
              ondragover={handleDragOver}
              ondragleave={handleDragLeave}
              ondrop={handleDrop}
              disabled={isUploadingMedia}
              class="w-full border-2 border-dashed rounded-2xl p-16 flex flex-col items-center justify-center bg-[#02171d]/50 hover:bg-[#02171d] transition-colors cursor-pointer disabled:cursor-wait
                {isDraggingOver ? 'border-cyan-400 bg-[#02171d]' : 'border-cyan-900/60'}"
            >
              {#if isUploadingMedia}
                <Loader2 class="w-6 h-6 text-cyan-400 animate-spin mb-2" />
                <p class="text-xs text-slate-400">Завантаження...</p>
              {:else if mediaPreviewUrl || mediaUrl}
                <video src={mediaPreviewUrl || mediaUrl} class="max-h-48 rounded-lg object-cover mb-2" controls></video>
                <p class="text-xs text-cyan-400">Відео успішно обрано</p>
              {:else}
                <p class="text-sm text-slate-400 mb-3">Перетягніть файл сюди або</p>
                <span class="bg-[#0b4e63] hover:bg-[#0d6e8a] text-white font-bold text-xs px-6 py-2.5 rounded-xl transition-colors shadow-md">
                  Завантажити
                </span>
              {/if}
            </button>

            <div>
              <label for="caption-vid" class="block text-xs text-slate-400 mb-1.5 font-bold">Підпис</label>
              <input
                id="caption-vid"
                type="text"
                bind:value={caption}
                placeholder="Ваш коментар до відео..."
                class="w-full bg-[#02171d] border border-cyan-900/60 rounded-full px-5 py-3 text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-cyan-500 transition-colors"
              />
            </div>
          </div>
        {/if}

        {#if activeTab === 'guide'}
          <div class="space-y-5">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-5">

              <div>
                <span class="block text-xs text-slate-400 mb-1.5 font-bold">Обкладинка</span>
                <button
                  type="button"
                  onclick={openFilePicker}
                  ondragover={handleDragOver}
                  ondragleave={handleDragLeave}
                  ondrop={handleDrop}
                  disabled={isUploadingMedia}
                  class="w-full border-2 border-dashed rounded-2xl p-8 flex flex-col items-center justify-center bg-[#02171d]/50 hover:bg-[#02171d] transition-colors h-[180px] cursor-pointer disabled:cursor-wait
                    {isDraggingOver ? 'border-cyan-400 bg-[#02171d]' : 'border-cyan-900/60'}"
                >
                  {#if isUploadingMedia}
                    <Loader2 class="w-6 h-6 text-cyan-400 animate-spin mb-2" />
                    <p class="text-xs text-slate-400">Завантаження...</p>
                  {:else if mediaUrl}
                    <img src={mediaUrl} alt="Guide Cover" class="max-h-24 rounded-lg object-cover mb-1" />
                    <p class="text-xs text-cyan-400">Обкладинку завантажено</p>
                  {:else}
                    <p class="text-xs text-slate-400 mb-3 text-center">Перетягніть файл сюди або</p>
                    <span class="bg-[#0b4e63] hover:bg-[#0d6e8a] text-white font-bold text-xs px-4 py-2 rounded-xl transition-colors shadow-md">
                      Завантажити
                    </span>
                  {/if}
                </button>
              </div>

              <div class="space-y-3">
                <div>
                  <div class="flex justify-between text-xs text-slate-400 mb-1 font-bold">
                    <label for="guide-title">Заголовок</label>
                    <span>{title.length}/160</span>
                  </div>
                  <input
                    id="guide-title"
                    type="text"
                    maxlength="160"
                    bind:value={title}
                    placeholder="Про що ваш гайд?"
                    class="w-full bg-[#02171d] border border-cyan-900/60 rounded-xl px-4 py-2.5 text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-cyan-500 transition-colors"
                  />
                </div>

                <div>
                  <div class="flex justify-between text-xs text-slate-400 mb-1 font-bold">
                    <label for="guide-desc">Опис</label>
                    <span>{description.length}/300</span>
                  </div>
                  <textarea
                    id="guide-desc"
                    rows="3"
                    maxlength="300"
                    bind:value={description}
                    placeholder="Опишіть тему детальніше..."
                    class="w-full bg-[#02171d] border border-cyan-900/60 rounded-xl px-4 py-2.5 text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-cyan-500 transition-colors resize-none"
                  ></textarea>
                </div>
              </div>
            </div>

            <div>
              <label for="guide-content" class="block text-xs text-slate-400 mb-1.5 font-bold">Текст</label>
              <div class="bg-[#02171d] border border-cyan-900/60 rounded-xl overflow-hidden focus-within:border-cyan-500 transition-colors">
                <div class="flex items-center gap-3 px-4 py-2.5 border-b border-cyan-900/40 text-slate-300">
                  <button type="button" onclick={() => applyFormatting('bold')} class="font-bold hover:text-white px-1 cursor-pointer">B</button>
                  <button type="button" onclick={() => applyFormatting('italic')} class="italic hover:text-white px-1 cursor-pointer">I</button>
                  <button type="button" onclick={() => applyFormatting('underline')} class="underline hover:text-white px-1 cursor-pointer">U</button>
                  <button type="button" onclick={() => applyFormatting('image')} aria-label="Вставити зображення" title="Вставити зображення" class="hover:text-white px-1 cursor-pointer">
                    <svg class="w-4 h-4 inline" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>
                  </button>
                </div>
                <textarea
                  bind:this={contentTextareaEl}
                  id="guide-content"
                  rows="5"
                  bind:value={content}
                  placeholder="Текст вашого гайду..."
                  class="w-full bg-transparent px-4 py-3 text-sm text-slate-100 placeholder-slate-500 focus:outline-none resize-none"
                ></textarea>
              </div>
            </div>
          </div>
        {/if}
      </div>

      <div class="flex justify-end items-center gap-4 mt-8 pt-4">
        <button
          type="button"
          onclick={handleCancel}
          class="text-sm font-bold text-slate-300 hover:text-white px-4 py-2 transition-colors cursor-pointer"
        >
          Відхилити
        </button>

        <button
          type="button"
          onclick={() => handleSubmit()}
          disabled={isSubmitting || isUploadingMedia}
          class="bg-[#21e6c1] hover:bg-[#1cd4b0] text-[#03232c] font-black text-sm px-6 py-2.5 rounded-full shadow-lg transition-all transform active:scale-95 disabled:opacity-50 cursor-pointer"
        >
          {isSubmitting ? 'Публікація...' : 'Опублікувати'}
        </button>
      </div>

    </div>

    <div class="space-y-4">
      <div class="text-right">
        <div class="text-sm font-bold text-slate-200">{gameName}</div>
        <div class="text-xs text-slate-400 mt-0.5 flex items-center justify-end gap-1.5">
          <span class="font-bold text-slate-300">{onlineCount}</span> онлайн
          <span class="inline-block w-2 h-2 rounded-full bg-rose-500 shadow-[0_0_8px_rgba(244,63,94,0.6)]"></span>
        </div>
      </div>

      <div class="bg-[#03232c] border border-cyan-900/60 rounded-2xl p-5 shadow-xl">
        <h2 class="text-base font-bold text-white mb-4">Сортувати за розділом</h2>
        <nav class="space-y-1.5">
          {#each feedCategories as key}
            <button
              type="button"
              onclick={() => setFeedCategory(key)}
              class="w-full text-left px-3.5 py-2.5 rounded-xl text-xs font-bold transition-all cursor-pointer
                {activeFeedCategory === key
                  ? 'bg-[#0b4e63] text-white shadow-md'
                  : 'text-slate-400 hover:text-white hover:bg-[#0b4e63]/20'}"
            >
              {feedCategoryLabels[key]}
            </button>
          {/each}
        </nav>
      </div>

      <div class="bg-[#03232c] border border-cyan-900/60 rounded-2xl p-5 shadow-xl">
        <h2 class="text-base font-bold text-white mb-4">Правила спільноти</h2>

        <ol class="space-y-4 text-xs text-slate-300 leading-relaxed">
          <li class="pb-3 border-b border-cyan-900/40">
            <span class="font-bold text-slate-200">1.</span> Публікуйте тільки оригінальний контент.
          </li>
          <li class="pb-3 border-b border-cyan-900/40">
            <span class="font-bold text-slate-200">2.</span> Не допускайте образ та принижень на адресу інших гравців, розробників чи груп.
          </li>
          <li class="pb-3 border-b border-cyan-900/40">
            <span class="font-bold text-slate-200">3.</span> Не включайте погрози або заохочення до заподіяння шкоди.
          </li>
          <li class="pb-3 border-b border-cyan-900/40">
            <span class="font-bold text-slate-200">4.</span> Не завантажуйте контент, на який у вас немає прав.
          </li>
          <li class="pb-3 border-b border-cyan-900/40">
            <span class="font-bold text-slate-200">5.</span> Не рекламуйте комерційний контент.
          </li>
          <li>
            <span class="font-bold text-slate-200">6.</span> Переконайтеся, що контент, який ви публікуєте, відповідає місцю, де він розміщується.
          </li>
        </ol>
      </div>
    </div>

  </div>

  <div class="w-full max-w-7xl mt-8">
    <h2 class="text-xl font-black mb-4 text-white font-display">
      Публікації{activeFeedCategory !== 'all' ? `: ${feedCategoryLabels[activeFeedCategory]}` : ''}
    </h2>

    {#if isLoadingPosts}
      <div class="flex items-center justify-center py-16 bg-[#03232c] border border-cyan-900/60 rounded-2xl">
        <Loader2 class="w-8 h-8 text-cyan-400 animate-spin" />
      </div>
    {:else if posts.length === 0}
      <div class="text-center py-16 bg-[#03232c] border border-cyan-900/60 rounded-2xl">
        <p class="text-sm text-slate-400">Поки що немає публікацій у цьому розділі.</p>
      </div>
    {:else}
      <div class="space-y-4">
        {#each posts as post (post.id)}
          <div class="bg-[#03232c] border border-cyan-900/60 rounded-2xl p-5 shadow-xl">
            <div class="flex items-center gap-3 mb-3">
              <button
                type="button"
                onclick={() => profileStore.viewProfile(post.author.id)}
                class="flex items-center gap-3 cursor-pointer text-left group"
              >
                <img
                  src={post.author.avatarUrl}
                  alt={post.author.username}
                  class="w-9 h-9 rounded-full object-cover ring-1 ring-cyan-900/60"
                />
                <div>
                  <span class="block text-sm font-bold text-slate-200 group-hover:text-cyan-300 transition-colors">{post.author.username}</span>
                  <span class="block text-[11px] text-slate-500">
                    {new Date(post.createdAt).toLocaleString('uk-UA')}
                  </span>
                </div>
              </button>
              <div class="ml-auto flex items-center gap-2">
                {#if post.gameTitle}
                  <span class="text-[11px] font-bold px-2.5 py-1 rounded-md bg-cyan-950/60 text-cyan-300 border border-cyan-800/40 flex items-center gap-1.5 shadow-sm">
                    <Gamepad2 class="w-3 h-3 text-cyan-400" />
                    {post.gameTitle}
                  </span>
                {/if}
                {#if post.category === 'news'}
                  <span class="text-[11px] font-extrabold px-2.5 py-1 rounded-md bg-emerald-500/20 text-emerald-300 border border-emerald-500/30 uppercase tracking-wide flex items-center gap-1.5 shadow-sm">
                    <span class="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse"></span>
                    Офіційна новина
                  </span>
                {:else}
                  <span class="text-[11px] font-bold px-2.5 py-1 rounded-md bg-[#0b4e63]/50 text-cyan-300 uppercase tracking-wide">
                    {feedCategoryLabels[post.category as keyof typeof feedCategoryLabels] ?? post.category}
                  </span>
                {/if}
              </div>
            </div>

            {#if post.title}
              <h3 class="text-base font-bold text-white mb-1.5">{post.title}</h3>
            {/if}

            <p class="text-sm text-slate-300 leading-relaxed">{@html renderPostContent(post.content)}</p>

            {#if post.media?.type === 'video' && post.media.url}
              <video src={resolveMediaUrl(post.media.url)} class="mt-3 rounded-xl max-h-96 w-full" controls playsinline></video>
            {:else if post.media?.url}
              <div
                class="relative mt-3 rounded-xl overflow-hidden cursor-pointer group/img"
                role="button"
                tabindex="0"
                onclick={() => (lightboxMedia = { src: resolveMediaUrl(post.media.url), title: post.title, author: post.author })}
                onkeydown={(e) => e.key === 'Enter' && (lightboxMedia = { src: resolveMediaUrl(post.media.url), title: post.title, author: post.author })}
              >
                <img src={resolveMediaUrl(post.media.url)} alt="" class="max-h-96 w-full object-cover group-hover/img:scale-101 transition-transform" />
                <div class="absolute bottom-2 right-2 px-2.5 py-1 rounded-lg bg-black/70 backdrop-blur-md text-xs text-slate-200 flex items-center gap-1 opacity-0 group-hover/img:opacity-100 transition-opacity">
                  <ZoomIn class="w-3 h-3" />
                  <span>Збільшити</span>
                </div>
              </div>
            {/if}

            <div class="flex items-center gap-5 mt-4 pt-3 border-t border-cyan-900/40">
              <button
                type="button"
                onclick={() => handleToggleLike(post)}
                class="flex items-center gap-1.5 text-xs font-bold transition-colors cursor-pointer
                  {post.stats.isLiked ? 'text-cyan-400' : 'text-slate-400 hover:text-white'}"
              >
                <ThumbsUp class="w-3.5 h-3.5 {post.stats.isLiked ? 'fill-cyan-400' : ''}" />
                {post.stats.likesCount}
              </button>

              <button
                type="button"
                onclick={async () => {
                  if (openCommentsPostId === post.id) {
                    openCommentsPostId = null;
                  } else {
                    openCommentsPostId = post.id;
                    if (!postCommentsMap[post.id]) {
                      loadingCommentsPostId = post.id;
                      try {
                        const details = await communityService.getPostDetails(post.id);
                        postCommentsMap[post.id] = details.comments || [];
                      } catch {
                        postCommentsMap[post.id] = [];
                      } finally {
                        loadingCommentsPostId = null;
                      }
                    }
                  }
                }}
                class="flex items-center gap-1.5 text-xs font-bold transition-colors cursor-pointer {openCommentsPostId === post.id ? 'text-cyan-300' : 'text-slate-400 hover:text-white'}"
              >
                <MessageSquare class="w-3.5 h-3.5" />
                <span>{post.stats.commentsCount}</span>
                <span class="text-[10px] text-slate-500 font-normal">
                  {openCommentsPostId === post.id ? 'Сховати' : 'Коментарі'}
                </span>
              </button>
            </div>

            {#if openCommentsPostId === post.id}
              <div class="mt-4 pt-4 border-t border-cyan-900/40 space-y-4 animate-in fade-in">

                <div class="flex gap-2.5">
                  <input
                    type="text"
                    bind:value={newCommentText[post.id]}
                    placeholder="Написати коментар..."
                    onkeydown={async (e) => {
                      if (e.key === 'Enter') {
                        const text = (newCommentText[post.id] || '').trim();
                        if (!text) return;
                        isSubmittingComment = true;
                        try {
                          const c = await communityService.addComment(post.id, text);
                          postCommentsMap[post.id] = [...(postCommentsMap[post.id] || []), c];
                          newCommentText[post.id] = '';
                          post.stats.commentsCount++;
                        } catch (err: any) {
                          uiStore.addToast({ title: 'Помилка', message: err?.message || 'Не вдалося надіслати коментар', type: 'error' });
                        } finally {
                          isSubmittingComment = false;
                        }
                      }
                    }}
                    class="flex-1 bg-[#02171d] border border-cyan-900/60 focus:border-cyan-400 rounded-xl px-4 py-2 text-xs text-white placeholder:text-slate-500 outline-none transition-colors"
                  />
                  <button
                    type="button"
                    onclick={async () => {
                      const text = (newCommentText[post.id] || '').trim();
                      if (!text) return;
                      isSubmittingComment = true;
                      try {
                        const c = await communityService.addComment(post.id, text);
                        postCommentsMap[post.id] = [...(postCommentsMap[post.id] || []), c];
                        newCommentText[post.id] = '';
                        post.stats.commentsCount++;
                      } catch (err: any) {
                        uiStore.addToast({ title: 'Помилка', message: err?.message || 'Не вдалося надіслати коментар', type: 'error' });
                      } finally {
                        isSubmittingComment = false;
                      }
                    }}
                    disabled={isSubmittingComment || !newCommentText[post.id]?.trim()}
                    class="px-4 py-2 rounded-xl bg-cyan-500 hover:bg-cyan-400 text-black font-bold text-xs transition-colors cursor-pointer disabled:opacity-40 shrink-0 flex items-center gap-1.5"
                  >
                    <Send class="w-3.5 h-3.5" />
                    <span>Надіслати</span>
                  </button>
                </div>

                {#if loadingCommentsPostId === post.id}
                  <div class="flex items-center justify-center py-6 text-xs text-slate-400 gap-2">
                    <Loader2 class="w-4 h-4 text-cyan-400 animate-spin" />
                    <span>Завантаження коментарів...</span>
                  </div>
                {:else if !postCommentsMap[post.id] || postCommentsMap[post.id].length === 0}
                  <p class="text-xs text-slate-500 text-center py-4">Ще немає коментарів. Будьте першим!</p>
                {:else}
                  <div class="space-y-3">
                    {#each postCommentsMap[post.id] as comment (comment.id)}
                      <div class="p-3 rounded-xl bg-[#02171d]/80 border border-cyan-900/40 space-y-2">
                        <div class="flex items-center justify-between">
                          <div class="flex items-center gap-2">
                            <img
                              src={comment.author.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(comment.author.username)}`}
                              alt={comment.author.username}
                              class="w-6 h-6 rounded-full object-cover border border-cyan-500/20"
                            />
                            <span class="text-xs font-bold text-slate-200">{comment.author.username}</span>
                            <span class="text-[10px] text-slate-500">{new Date(comment.createdAt).toLocaleDateString('uk-UA')}</span>
                          </div>

                          <button
                            type="button"
                            onclick={() => {
                              replyingToCommentId = replyingToCommentId === comment.id ? null : comment.id;
                              replyText = '';
                            }}
                            class="text-[11px] font-bold text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer flex items-center gap-1"
                          >
                            <CornerDownRight class="w-3 h-3" />
                            <span>Відповісти</span>
                          </button>
                        </div>

                        <p class="text-xs text-slate-300 leading-relaxed pl-8">{@html renderPostContent(comment.content)}</p>

                        {#if comment.replies && comment.replies.length > 0}
                          <div class="pl-8 pt-2 space-y-2 border-l-2 border-cyan-500/20 ml-3">
                            {#each comment.replies as reply (reply.id)}
                              <div class="p-2.5 rounded-lg bg-[#03232c]/60 border border-cyan-900/30 space-y-1">
                                <div class="flex items-center gap-2">
                                  <img
                                    src={reply.author.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(reply.author.username)}`}
                                    alt={reply.author.username}
                                    class="w-5 h-5 rounded-full object-cover border border-cyan-500/20"
                                  />
                                  <span class="text-[11px] font-bold text-slate-200">{reply.author.username}</span>
                                  <span class="text-[9px] text-slate-500">{new Date(reply.createdAt).toLocaleDateString('uk-UA')}</span>
                                </div>
                                <p class="text-xs text-slate-300 pl-7">{@html renderPostContent(reply.content)}</p>
                              </div>
                            {/each}
                          </div>
                        {/if}

                        {#if replyingToCommentId === comment.id}
                          <div class="pl-8 pt-2 animate-in fade-in flex gap-2">
                            <input
                              type="text"
                              bind:value={replyText}
                              placeholder="Ваша відповідь..."
                              onkeydown={async (e) => {
                                if (e.key === 'Enter') {
                                  if (!replyText.trim()) return;
                                  isSubmittingReply = true;
                                  try {
                                    const r = await communityService.addReply(comment.id, replyText.trim());
                                    comment.replies = comment.replies || [];
                                    comment.replies.push(r);
                                    postCommentsMap[post.id] = [...postCommentsMap[post.id]];
                                    replyText = '';
                                    replyingToCommentId = null;
                                    post.stats.commentsCount++;
                                  } catch (err: any) {
                                    uiStore.addToast({ title: 'Помилка', message: err?.message || 'Не вдалося надіслати відповідь', type: 'error' });
                                  } finally {
                                    isSubmittingReply = false;
                                  }
                                }
                              }}
                              class="flex-1 bg-[#011015] border border-cyan-500/40 rounded-xl px-3.5 py-1.5 text-xs text-white placeholder:text-slate-500 outline-none"
                            />
                            <button
                              type="button"
                              onclick={async () => {
                                if (!replyText.trim()) return;
                                isSubmittingReply = true;
                                try {
                                  const r = await communityService.addReply(comment.id, replyText.trim());
                                  comment.replies = comment.replies || [];
                                  comment.replies.push(r);
                                  postCommentsMap[post.id] = [...postCommentsMap[post.id]];
                                  replyText = '';
                                  replyingToCommentId = null;
                                  post.stats.commentsCount++;
                                } catch (err: any) {
                                  uiStore.addToast({ title: 'Помилка', message: err?.message || 'Не вдалося надіслати відповідь', type: 'error' });
                                } finally {
                                  isSubmittingReply = false;
                                }
                              }}
                              disabled={isSubmittingReply || !replyText.trim()}
                              class="px-3 py-1.5 rounded-xl bg-cyan-500 hover:bg-cyan-400 text-black font-bold text-xs transition-colors cursor-pointer disabled:opacity-40 shrink-0"
                            >
                              Відповісти
                            </button>
                            <button
                              type="button"
                              onclick={() => { replyingToCommentId = null; replyText = ''; }}
                              class="p-1.5 rounded-xl text-slate-400 hover:text-white cursor-pointer"
                            >
                              <X class="w-3.5 h-3.5" />
                            </button>
                          </div>
                        {/if}
                      </div>
                    {/each}
                  </div>
                {/if}
              </div>
            {/if}
          </div>
        {/each}
      </div>
    {/if}
  </div>
</div>

{#if lightboxMedia}
  <MediaLightboxModal
    src={lightboxMedia.src}
    title={lightboxMedia.title}
    author={lightboxMedia.author}
    onClose={() => (lightboxMedia = null)}
  />
{/if}

<SelectGameModal
  isOpen={isSelectGameModalOpen}
  selectedGameId={selectedGame?.id}
  onSelect={(game) => {
    selectedGame = game;
    loadPosts();
  }}
  onClose={() => isSelectGameModalOpen = false}
/>
