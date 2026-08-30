<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { communityService, type CommunityPost } from '../../services/communityService';
  import { mediaService, ALLOWED_IMAGE_TYPES, ALLOWED_VIDEO_TYPES, MAX_IMAGE_SIZE_BYTES, MAX_VIDEO_SIZE_BYTES } from '../../services/mediaService';
  import { uiStore } from '../../stores/uiStore';
  import { profileStore } from '../../stores/profileStore';
  import { ThumbsUp, MessageSquare, Loader2 } from 'lucide-svelte';
  import { onlineHubService } from '../../services/onlineHubService';

  interface Props {
    gameId?: string | null;
    gameName?: string;
  }

  let {
    gameId = null,
    gameName = "Онлайн людей на сайті"
  }: Props = $props();

  let onlineCount = $state(onlineHubService.getOnlineCount());

  type TabType = 'discussion' | 'screenshot' | 'video' | 'guide';
  let activeTab = $state<TabType>('discussion');

  let title = $state('');
  let description = $state('');
  let content = $state('');
  let caption = $state('');
  let mediaUrl = $state('');
  let mediaThumbnailUrl = $state('');
  let isSubmitting = $state(false);

  let isUploadingMedia = $state(false);
  let isDraggingOver = $state(false);
  let fileInputEl: HTMLInputElement | undefined = $state();
  let selectedFile = $state<File | null>(null);

  // Ссылки на поля ввода текста для модификатора
  let contentTextareaEl: HTMLTextAreaElement | undefined = $state();

  function setTab(tab: TabType) {
    activeTab = tab;
    title = '';
    description = '';
    content = '';
    caption = '';
    mediaUrl = '';
    mediaThumbnailUrl = '';
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

  const feedCategoryLabels: Record<'all' | 'forum' | 'screenshots' | 'videos' | 'guides', string> = {
    all: 'Усі',
    forum: 'Дискусія',
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

  // Экранируем HTML, затем превращаем розмітку тулбара (**bold**, *italic*,
  // <u>underline</u>, ![alt](url)) у реальні теги. Раніше пост рендерився
  // як звичайний текст, тому вся розмітка показувалась "як є" — буквально
  // зі зірочками й тегами. Тепер вона реально застосовується.
  function escapeHtml(text: string): string {
    return text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function renderPostContent(raw: string): string {
    let safe = escapeHtml(raw);

    // ![опис](url) -> <img>
    safe = safe.replace(
      /!\[([^\]]*)\]\((https?:\/\/[^\s)]+)\)/g,
      '<img src="$2" alt="$1" class="my-2 rounded-lg max-h-80 max-w-full object-contain" />'
    );

    // **bold**
    safe = safe.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');

    // *italic* (одиночні зірочки, після того як bold вже "з'їв" подвійні)
    safe = safe.replace(/\*([^*]+)\*/g, '<em>$1</em>');

    // <u>underline</u> вище був заекранований у &lt;u&gt;...&lt;/u&gt; — повертаємо тег
    safe = safe.replace(/&lt;u&gt;([\s\S]*?)&lt;\/u&gt;/g, '<u>$1</u>');

    // зберігаємо переноси рядків
    safe = safe.replace(/\n/g, '<br />');

    return safe;
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
    
    // Если текст выделен — оборачиваем его, если нет — подставляем слово "текст"
    const replacement = `${chunk.start}${selectedText || 'текст'}${chunk.end}`;
    
    content = content.substring(0, start) + replacement + content.substring(end);

    // Возвращаем фокус и выделяем вставленный шаблон / текст
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
      screenshot: 'screenshots',
      video: 'videos',
      guide: 'guides'
    };

    const finalTitle = activeTab === 'screenshot' || activeTab === 'video' 
      ? (caption || `${activeTab.toUpperCase()} post`) 
      : title;

    const finalContent = activeTab === 'guide' 
      ? `${description}\n\n${content}` 
      : (content || caption || 'Без опису');

    const postPayload = {
      category: categoryMap[activeTab],
      title: finalTitle,
      content: finalContent,
      mediaType: mediaUrl ? (activeTab === 'video' ? 'video' : 'image') : 'none',
      mediaUrl,
      mediaThumbnailUrl: mediaThumbnailUrl || undefined,
      file: selectedFile || null
    };

    try {
      await communityService.createPost(gameId, postPayload);
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
        
        <div class="grid grid-cols-4 gap-2 mb-6">
          <button
            type="button"
            onclick={() => setTab('discussion')}
            class="py-2.5 rounded-xl font-bold transition-all duration-200 text-center text-xs md:text-sm cursor-pointer {activeTab === 'discussion' ? 'bg-[#0b4e63] text-white shadow-md' : 'text-slate-400 hover:text-white hover:bg-[#0b4e63]/20'}"
          >
            Обговорення
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
              {:else if mediaUrl}
                <img src={mediaUrl} alt="Uploaded" class="max-h-32 rounded-lg object-cover mb-2" />
                <p class="text-xs text-cyan-400">Файл успішно завантажено</p>
              {:else}
                <p class="text-sm text-slate-400 mb-3">Перетягніть файл сюди або</p>
                <span class="bg-[#0b4e63] hover:bg-[#0d6e8a] text-white font-bold text-xs px-5 py-2.5 rounded-xl transition-colors shadow-md">
                  Завантажити
                </span>
              {/if}
            </button>
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
              {:else if mediaUrl}
                <img src={mediaUrl} alt="Uploaded Screenshot" class="max-h-48 rounded-lg object-cover mb-2" />
                <p class="text-xs text-cyan-400">Зображення успішно завантажено</p>
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
              {:else if mediaUrl}
                <video src={mediaUrl} class="max-h-48 rounded-lg object-cover mb-2" controls></video>
                <p class="text-xs text-cyan-400">Відео успішно завантажено</p>
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
              <span class="ml-auto text-[11px] font-bold px-2.5 py-1 rounded-md bg-[#0b4e63]/50 text-cyan-300 uppercase tracking-wide">
                {feedCategoryLabels[post.category as keyof typeof feedCategoryLabels] ?? post.category}
              </span>
            </div>

            {#if post.title}
              <h3 class="text-base font-bold text-white mb-1.5">{post.title}</h3>
            {/if}

            <p class="text-sm text-slate-300 leading-relaxed">{@html renderPostContent(post.content)}</p>

            {#if post.media?.type === 'image' && post.media.url}
              <img src={post.media.url} alt="" class="mt-3 rounded-xl max-h-96 w-full object-cover" />
            {:else if post.media?.type === 'video' && post.media.url}
              <video src={post.media.url} class="mt-3 rounded-xl max-h-96 w-full" controls></video>
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
              <span class="flex items-center gap-1.5 text-xs font-bold text-slate-400">
                <MessageSquare class="w-3.5 h-3.5" />
                {post.stats.commentsCount}
              </span>
            </div>
          </div>
        {/each}
      </div>
    {/if}
  </div>
</div>