<script lang="ts">
  import { X, ZoomIn, ZoomOut, Download, ExternalLink } from 'lucide-svelte';

  let {
    src,
    alt = 'Зображення',
    title,
    author,
    onClose,
  }: {
    src: string;
    alt?: string;
    title?: string;
    author?: { username: string; avatarUrl?: string };
    onClose: () => void;
  } = $props();

  let isZoomed = $state(false);

  function handleKeydown(e: KeyboardEvent) {
    if (e.key === 'Escape') {
      onClose();
    }
  }
</script>

<svelte:window onkeydown={handleKeydown} />

<div
  class="fixed inset-0 z-[120] flex flex-col items-center justify-center bg-black/95 backdrop-blur-xl animate-in fade-in duration-200 select-none cursor-pointer"
  role="dialog"
  aria-modal="true"
  tabindex="-1"
  onclick={(e) => {
    if (e.target === e.currentTarget) onClose();
  }}
>

  <div
    class="absolute top-0 left-0 right-0 p-4 sm:p-5 flex items-center justify-between z-30 bg-gradient-to-b from-black/90 via-black/50 to-transparent pointer-events-auto"
    onclick={(e) => e.stopPropagation()}
  >
    <div class="flex items-center gap-3 min-w-0">
      {#if author}
        <img
          src={author.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(author.username)}`}
          alt={author.username}
          class="w-9 h-9 rounded-full border border-cyan-500/40 object-cover shrink-0"
        />
        <div class="min-w-0">
          <span class="text-xs font-bold text-white block truncate">{author.username}</span>
          {#if title}
            <span class="text-[11px] text-slate-400 truncate max-w-xs sm:max-w-md block">{title}</span>
          {/if}
        </div>
      {:else if title}
        <span class="text-sm font-bold text-white truncate max-w-sm sm:max-w-md">{title}</span>
      {/if}
    </div>

    <div class="flex items-center gap-2 shrink-0">
      <button
        type="button"
        onclick={(e) => { e.stopPropagation(); isZoomed = !isZoomed; }}
        class="p-2.5 rounded-xl bg-white/10 hover:bg-cyan-500/20 text-slate-300 hover:text-cyan-300 transition-colors cursor-pointer"
        title={isZoomed ? 'Зменшити' : 'Збільшити'}
      >
        {#if isZoomed}
          <ZoomOut class="w-4 h-4" />
        {:else}
          <ZoomIn class="w-4 h-4" />
        {/if}
      </button>

      <a
        href={src}
        target="_blank"
        rel="noopener noreferrer"
        class="p-2.5 rounded-xl bg-white/10 hover:bg-cyan-500/20 text-slate-300 hover:text-cyan-300 transition-colors cursor-pointer"
        title="Відкрити в новій вкладці"
        onclick={(e) => e.stopPropagation()}
      >
        <ExternalLink class="w-4 h-4" />
      </a>

      <button
        type="button"
        onclick={(e) => { e.stopPropagation(); onClose(); }}
        class="p-2.5 rounded-xl bg-white/15 hover:bg-red-500/30 text-white hover:text-red-300 border border-white/20 transition-all cursor-pointer shadow-lg"
        aria-label="Закрити"
        title="Закрити (Esc)"
      >
        <X class="w-5 h-5" />
      </button>
    </div>
  </div>

  <div
    class="w-full h-full p-4 sm:p-10 flex items-center justify-center overflow-auto cursor-pointer"
    onclick={(e) => {
      if (e.target === e.currentTarget) onClose();
    }}
  >
    <img
      {src}
      {alt}
      class="max-h-[85vh] max-w-[90vw] object-contain rounded-2xl shadow-2xl transition-transform duration-300 {isZoomed ? 'scale-125 cursor-zoom-out' : 'cursor-zoom-in'}"
      onclick={(e) => {
        e.stopPropagation();
        isZoomed = !isZoomed;
      }}
    />
  </div>

  <div
    class="absolute bottom-4 left-1/2 -translate-x-1/2 text-[11px] text-slate-400 bg-black/60 backdrop-blur-md px-3 py-1 rounded-full border border-white/10 pointer-events-none"
  >
    Натисніть у будь-якому місці за межами фото або ESC, щоб вийти
  </div>
</div>
