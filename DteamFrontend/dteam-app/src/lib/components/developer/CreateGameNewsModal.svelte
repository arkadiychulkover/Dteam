<script lang="ts">
  import type { Game } from '../../types';
  import { developerService } from '../../services/developerService';
  import { uiStore } from '../../stores/uiStore';
  import { X, Newspaper, Upload, Image, Film, Loader2, Gamepad2, AlertCircle } from 'lucide-svelte';

  interface Props {
    game: Game;
    isOpen: boolean;
    onClose: () => void;
    onCreated?: () => void;
  }

  let { game, isOpen, onClose, onCreated }: Props = $props();

  let title = $state('');
  let content = $state('');
  let selectedFile = $state<File | null>(null);
  let mediaPreviewUrl = $state('');
  let isSubmitting = $state(false);
  let isDragging = $state(false);
  let fileInputEl: HTMLInputElement | undefined = $state();

  function resetForm() {
    title = '';
    content = '';
    if (mediaPreviewUrl) URL.revokeObjectURL(mediaPreviewUrl);
    mediaPreviewUrl = '';
    selectedFile = null;
    isDragging = false;
    isSubmitting = false;
  }

  function handleClose() {
    resetForm();
    onClose();
  }

  function handleFileSelected(file: File) {
    if (!file) return;
    const isImage = file.type.startsWith('image/');
    const isVideo = file.type.startsWith('video/');

    if (!isImage && !isVideo) {
      uiStore.addToast({
        title: 'Непідтримуваний формат',
        message: 'Оберіть зображення (.jpg, .png, .webp) або відео (.mp4, .webm).',
        type: 'warning'
      });
      return;
    }

    const maxSize = isImage ? 20 * 1024 * 1024 : 150 * 1024 * 1024;
    if (file.size > maxSize) {
      uiStore.addToast({
        title: 'Файл завеликий',
        message: `Максимальний розмір: ${Math.round(maxSize / (1024 * 1024))} МБ.`,
        type: 'warning'
      });
      return;
    }

    if (mediaPreviewUrl) URL.revokeObjectURL(mediaPreviewUrl);
    mediaPreviewUrl = URL.createObjectURL(file);
    selectedFile = file;
  }

  function handleFileInputChange(e: Event) {
    const input = e.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      handleFileSelected(input.files[0]);
    }
  }

  function handleDrop(e: DragEvent) {
    e.preventDefault();
    isDragging = false;
    if (e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileSelected(e.dataTransfer.files[0]);
    }
  }

  function removeMedia(e: MouseEvent) {
    e.stopPropagation();
    if (mediaPreviewUrl) URL.revokeObjectURL(mediaPreviewUrl);
    mediaPreviewUrl = '';
    selectedFile = null;
    if (fileInputEl) fileInputEl.value = '';
  }

  async function handleSubmit(e: Event) {
    e.preventDefault();
    if (!title.trim()) {
      uiStore.addToast({ title: 'Помилка', message: 'Вкажіть заголовок новини.', type: 'warning' });
      return;
    }
    if (!content.trim()) {
      uiStore.addToast({ title: 'Помилка', message: 'Введіть текст новини чи оновлення.', type: 'warning' });
      return;
    }

    isSubmitting = true;
    try {
      await developerService.createGameNews(game.id, {
        title: title.trim(),
        content: content.trim(),
        file: selectedFile || undefined,
        mediaType: selectedFile ? (selectedFile.type.startsWith('video/') ? 'video' : 'image') : 'none'
      });

      uiStore.addToast({
        title: 'Новину опубліковано',
        message: `Новина для '${game.title}' успішно додана до стрічки!`,
        type: 'success'
      });

      handleClose();
      onCreated?.();
    } catch (err: any) {
      uiStore.addToast({
        title: 'Помилка публікації',
        message: err?.message || 'Не вдалося створити новину.',
        type: 'error'
      });
    } finally {
      isSubmitting = false;
    }
  }
</script>

<svelte:window onkeydown={(e) => { if (e.key === 'Escape' && isOpen && !isSubmitting) handleClose(); }} />

{#if isOpen}
  <div
    role="presentation"
    class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-fade-in"
    onclick={(e) => { if (e.target === e.currentTarget && !isSubmitting) handleClose(); }}
  >
    <div
      role="dialog"
      aria-modal="true"
      class="relative w-full max-w-2xl bg-[#061820] border border-cyan-500/30 rounded-3xl p-6 sm:p-7 shadow-2xl shadow-cyan-950/60 overflow-hidden max-h-[90vh] flex flex-col"
    >
      <!-- Background glow -->
      <div class="absolute -top-24 -right-24 w-56 h-56 bg-cyan-500/10 rounded-full blur-3xl pointer-events-none"></div>

      <!-- Header -->
      <div class="flex items-start justify-between pb-4 border-b border-cyan-900/40 shrink-0">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-cyan-500/20 border border-cyan-500/30 flex items-center justify-center text-cyan-400">
            <Newspaper class="w-5 h-5" />
          </div>
          <div>
            <h2 class="text-lg font-bold text-white flex items-center gap-2">
              Опублікувати новину
            </h2>
            <p class="text-xs text-slate-400">
              Офіційний патч-ноут або анонс для гравців
            </p>
          </div>
        </div>
        <button
          type="button"
          disabled={isSubmitting}
          onclick={handleClose}
          class="p-2 text-slate-400 hover:text-white rounded-xl hover:bg-slate-800/60 transition-colors cursor-pointer disabled:opacity-50"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Game Target Badge -->
      <div class="mt-4 p-3 rounded-2xl bg-[#021319] border border-cyan-900/40 flex items-center gap-3 shrink-0">
        <div class="w-12 h-12 rounded-xl overflow-hidden bg-slate-900 shrink-0 border border-cyan-500/20">
          {#if game.coverImageUrl || game.headerImageUrl}
            <img src={game.coverImageUrl || game.headerImageUrl} alt={game.title} class="w-full h-full object-cover" />
          {:else}
            <div class="w-full h-full flex items-center justify-center text-cyan-500">
              <Gamepad2 class="w-6 h-6" />
            </div>
          {/if}
        </div>
        <div class="min-w-0 flex-1">
          <span class="text-[10px] font-bold text-cyan-400 uppercase tracking-wider">Прив'язана гра</span>
          <h4 class="text-sm font-bold text-white truncate">{game.title}</h4>
        </div>
      </div>

      <!-- Form Body -->
      <form onsubmit={handleSubmit} class="flex-1 overflow-y-auto mt-4 space-y-4 pr-1">
        <!-- Title -->
        <div>
          <label for="news-title" class="block text-xs font-bold text-slate-300 mb-1.5">
            Заголовок новини <span class="text-cyan-400">*</span>
          </label>
          <input
            id="news-title"
            type="text"
            bind:value={title}
            disabled={isSubmitting}
            placeholder="Наприклад: Оновлення 1.2 — Нові локації та баланс зброї"
            class="w-full px-4 py-2.5 bg-[#02171d] border border-cyan-900/60 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 transition-colors disabled:opacity-60"
          />
        </div>

        <!-- Content -->
        <div>
          <label for="news-content" class="block text-xs font-bold text-slate-300 mb-1.5">
            Текст новини / Патч-ноут <span class="text-cyan-400">*</span>
          </label>
          <textarea
            id="news-content"
            rows="5"
            bind:value={content}
            disabled={isSubmitting}
            placeholder="Опишіть зміни, деталі оновлення або звернення до спільноти..."
            class="w-full px-4 py-3 bg-[#02171d] border border-cyan-900/60 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 transition-colors resize-none disabled:opacity-60"
          ></textarea>
        </div>

        <!-- Media Attachment -->
        <div>
          <span class="block text-xs font-bold text-slate-300 mb-1.5">Медіа (зображення або відео)</span>
          <input
            type="file"
            accept="image/*,video/*"
            bind:this={fileInputEl}
            onchange={handleFileInputChange}
            class="hidden"
          />

          {#if mediaPreviewUrl}
            <div class="relative rounded-2xl overflow-hidden border border-cyan-500/40 bg-black/40 group max-h-56">
              {#if selectedFile?.type.startsWith('video/')}
                <video src={mediaPreviewUrl} class="w-full max-h-56 object-cover" controls></video>
              {:else}
                <img src={mediaPreviewUrl} alt="Preview" class="w-full max-h-56 object-cover" />
              {/if}
              <button
                type="button"
                onclick={removeMedia}
                class="absolute top-2.5 right-2.5 p-1.5 bg-black/70 hover:bg-red-500 text-white rounded-full transition-colors cursor-pointer shadow-lg"
                title="Видалити медіа"
              >
                <X class="w-4 h-4" />
              </button>
            </div>
          {:else}
            <button
              type="button"
              onclick={() => fileInputEl?.click()}
              ondragover={(e) => { e.preventDefault(); isDragging = true; }}
              ondragleave={() => isDragging = false}
              ondrop={handleDrop}
              class="w-full border-2 border-dashed rounded-2xl p-6 flex flex-col items-center justify-center gap-2 bg-[#02171d]/50 hover:bg-[#02171d] transition-all cursor-pointer {isDragging ? 'border-cyan-400 bg-cyan-950/20' : 'border-cyan-900/60'}"
            >
              <div class="w-10 h-10 rounded-full bg-cyan-500/10 flex items-center justify-center text-cyan-400">
                <Upload class="w-5 h-5" />
              </div>
              <p class="text-xs font-medium text-slate-300">
                Перетягніть файл сюди або <span class="text-cyan-400 font-bold underline">оберіть з пристрою</span>
              </p>
              <span class="text-[10px] text-slate-500">
                Підтримуються JPG, PNG, WEBP (до 20 МБ) та MP4, WebM (до 150 МБ)
              </span>
            </button>
          {/if}
        </div>

        <!-- Actions -->
        <div class="flex items-center justify-end gap-3 pt-4 border-t border-cyan-900/40">
          <button
            type="button"
            disabled={isSubmitting}
            onclick={handleClose}
            class="px-5 py-2.5 rounded-xl border border-cyan-900/60 text-slate-300 hover:text-white hover:bg-cyan-950/30 text-xs font-bold transition-all cursor-pointer disabled:opacity-50"
          >
            Скасувати
          </button>

          <button
            type="submit"
            disabled={isSubmitting || !title.trim() || !content.trim()}
            class="px-6 py-2.5 rounded-xl bg-[#0df2c9] hover:bg-[#21fcd6] text-black text-xs font-extrabold transition-all cursor-pointer shadow-lg shadow-cyan-500/20 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
          >
            {#if isSubmitting}
              <Loader2 class="w-4 h-4 animate-spin text-black" />
              <span>Публікація...</span>
            {:else}
              <Newspaper class="w-4 h-4" />
              <span>Опублікувати</span>
            {/if}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}
