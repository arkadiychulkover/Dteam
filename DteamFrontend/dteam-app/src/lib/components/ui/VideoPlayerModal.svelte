<script lang="ts">
  import { onMount } from 'svelte';
  import {
    X,
    Play,
    Pause,
    Volume2,
    VolumeX,
    Maximize,
    Minimize,
    RotateCcw,
    ExternalLink,
    Loader2
  } from 'lucide-svelte';

  let {
    isOpen = false,
    videoUrl = '',
    title = '',
    gameTitle = '',
    authorUsername = '',
    authorAvatarUrl = '',
    createdAt = '',
    onClose = () => {}
  }: {
    isOpen: boolean;
    videoUrl?: string;
    title?: string;
    gameTitle?: string;
    authorUsername?: string;
    authorAvatarUrl?: string;
    createdAt?: string;
    onClose: () => void;
  } = $props();

  let videoElement = $state<HTMLVideoElement | null>(null);
  let containerElement = $state<HTMLDivElement | null>(null);

  let isPlaying = $state(false);
  let currentTime = $state(0);
  let duration = $state(0);
  let volume = $state(1);
  let isMuted = $state(false);
  let isFullscreen = $state(false);
  let isBuffering = $state(false);
  let showControls = $state(true);
  let controlsTimeout: any = null;

  // Resolve backend video URL or external URL
  const resolvedSrc = $derived.by(() => {
    if (!videoUrl) return '';
    if (
      videoUrl.startsWith('http://') ||
      videoUrl.startsWith('https://') ||
      videoUrl.startsWith('blob:') ||
      videoUrl.startsWith('data:')
    ) {
      return videoUrl;
    }
    return videoUrl.startsWith('/') ? videoUrl : `/${videoUrl}`;
  });

  const isYouTube = $derived.by(() => {
    if (!videoUrl) return false;
    return videoUrl.includes('youtube.com') || videoUrl.includes('youtu.be');
  });

  const youtubeEmbedUrl = $derived.by(() => {
    if (!isYouTube || !videoUrl) return '';
    try {
      if (videoUrl.includes('youtu.be/')) {
        const id = videoUrl.split('youtu.be/')[1]?.split('?')[0];
        return `https://www.youtube.com/embed/${id}?autoplay=1`;
      }
      const urlObj = new URL(videoUrl);
      const v = urlObj.searchParams.get('v');
      if (v) return `https://www.youtube.com/embed/${v}?autoplay=1`;
    } catch {
      // fallback
    }
    return videoUrl;
  });

  function formatTime(seconds: number): string {
    if (isNaN(seconds) || seconds < 0) return '0:00';
    const m = Math.floor(seconds / 60);
    const s = Math.floor(seconds % 60);
    return `${m}:${s < 10 ? '0' : ''}${s}`;
  }

  function togglePlay() {
    if (!videoElement) return;
    if (videoElement.paused) {
      videoElement.play();
      isPlaying = true;
    } else {
      videoElement.pause();
      isPlaying = false;
    }
  }

  function handleTimeUpdate() {
    if (!videoElement) return;
    currentTime = videoElement.currentTime;
    duration = videoElement.duration || 0;
  }

  function handleSeek(e: Event) {
    const input = e.target as HTMLInputElement;
    const targetTime = parseFloat(input.value);
    if (videoElement) {
      videoElement.currentTime = targetTime;
      currentTime = targetTime;
    }
  }

  function toggleMute() {
    if (!videoElement) return;
    isMuted = !isMuted;
    videoElement.muted = isMuted;
  }

  function handleVolumeChange(e: Event) {
    const input = e.target as HTMLInputElement;
    const val = parseFloat(input.value);
    volume = val;
    if (videoElement) {
      videoElement.volume = val;
      isMuted = val === 0;
      videoElement.muted = isMuted;
    }
  }

  function toggleFullscreen() {
    if (!containerElement) return;
    if (!document.fullscreenElement) {
      containerElement.requestFullscreen().catch(() => {});
      isFullscreen = true;
    } else {
      document.exitFullscreen().catch(() => {});
      isFullscreen = false;
    }
  }

  function handleKeyDown(e: KeyboardEvent) {
    if (!isOpen) return;
    if (e.key === 'Escape') {
      if (document.fullscreenElement) {
        document.exitFullscreen().catch(() => {});
        isFullscreen = false;
      } else {
        closeModal();
      }
    } else if (e.key === ' ' || e.key === 'k') {
      e.preventDefault();
      togglePlay();
    } else if (e.key === 'f') {
      e.preventDefault();
      toggleFullscreen();
    } else if (e.key === 'm') {
      e.preventDefault();
      toggleMute();
    } else if (e.key === 'ArrowRight' && videoElement) {
      e.preventDefault();
      videoElement.currentTime = Math.min(videoElement.duration, videoElement.currentTime + 5);
    } else if (e.key === 'ArrowLeft' && videoElement) {
      e.preventDefault();
      videoElement.currentTime = Math.max(0, videoElement.currentTime - 5);
    }
  }

  function handleMouseMove() {
    showControls = true;
    clearTimeout(controlsTimeout);
    controlsTimeout = setTimeout(() => {
      if (isPlaying) {
        showControls = false;
      }
    }, 2500);
  }

  function closeModal() {
    if (videoElement) {
      videoElement.pause();
    }
    if (document.fullscreenElement) {
      document.exitFullscreen().catch(() => {});
    }
    onClose();
  }

  $effect(() => {
    if (isOpen) {
      isPlaying = false;
      currentTime = 0;
      duration = 0;
      isBuffering = false;
      showControls = true;
      window.addEventListener('keydown', handleKeyDown);
      document.body.style.overflow = 'hidden';
    } else {
      window.removeEventListener('keydown', handleKeyDown);
      document.body.style.overflow = '';
      if (videoElement) {
        videoElement.pause();
      }
    }

    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      document.body.style.overflow = '';
    };
  });
</script>

{#if isOpen}
  <div
    class="fixed inset-0 z-[100] flex items-center justify-center bg-black/85 backdrop-blur-md p-2 md:p-6 animate-in fade-in duration-200"
    role="dialog"
    aria-modal="true"
    onclick={(e) => {
      if (e.target === e.currentTarget) closeModal();
    }}
  >
    <div
      bind:this={containerElement}
      onmousemove={handleMouseMove}
      class="relative w-full max-w-5xl bg-[#03151e] border border-cyan-500/30 rounded-2xl overflow-hidden shadow-2xl shadow-cyan-950/60 flex flex-col max-h-[92vh] group select-none"
    >
      <!-- Header bar with Title and Close button -->
      <div
        class="flex items-center justify-between px-5 py-3.5 bg-gradient-to-b from-[#021017]/90 to-[#021017]/40 border-b border-cyan-500/20 z-20 transition-opacity duration-300 {showControls
          ? 'opacity-100'
          : 'opacity-0 pointer-events-none'}"
      >
        <div class="flex items-center gap-3 min-w-0 pr-4">
          {#if authorAvatarUrl}
            <img
              src={authorAvatarUrl}
              alt={authorUsername}
              class="w-8 h-8 rounded-full object-cover border border-cyan-500/40 shrink-0"
            />
          {/if}
          <div class="min-w-0">
            <h3 class="text-sm md:text-base font-bold text-white truncate drop-shadow-sm">
              {title || 'Відео'}
            </h3>
            <div class="flex items-center gap-2 text-xs text-slate-400">
              {#if authorUsername}
                <span class="text-cyan-300 font-medium">@{authorUsername}</span>
              {/if}
              {#if gameTitle}
                <span>•</span>
                <span class="text-slate-300 bg-cyan-950/70 border border-cyan-800/40 px-2 py-0.5 rounded text-[11px] font-semibold">
                  {gameTitle}
                </span>
              {/if}
              {#if createdAt}
                <span>•</span>
                <span>{new Date(createdAt).toLocaleDateString('uk-UA')}</span>
              {/if}
            </div>
          </div>
        </div>

        <button
          type="button"
          onclick={closeModal}
          class="p-2 rounded-xl bg-cyan-950/40 hover:bg-rose-500/20 border border-cyan-800/40 hover:border-rose-500/50 text-slate-300 hover:text-rose-300 transition-all cursor-pointer shrink-0"
          title="Закрити (Esc)"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Video Player Area -->
      <div class="relative flex-1 bg-black flex items-center justify-center overflow-hidden min-h-[300px] md:min-h-[480px]">
        {#if isYouTube}
          <iframe
            src={youtubeEmbedUrl}
            title={title || 'YouTube video'}
            class="w-full h-full min-h-[400px] md:min-h-[500px]"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
            allowfullscreen
          ></iframe>
        {:else if resolvedSrc}
          <!-- Native HTML5 Video Player -->
          <video
            bind:this={videoElement}
            src={resolvedSrc}
            playsinline
            preload="metadata"
            class="w-full h-full max-h-[72vh] object-contain cursor-pointer"
            onclick={togglePlay}
            ontimeupdate={handleTimeUpdate}
            onloadedmetadata={() => {
              if (videoElement) duration = videoElement.duration || 0;
            }}
            onwaiting={() => (isBuffering = true)}
            onplaying={() => {
              isBuffering = false;
              isPlaying = true;
            }}
            onpause={() => (isPlaying = false)}
            onended={() => {
              isPlaying = false;
              showControls = true;
            }}
          >
            <track kind="captions" />
          </video>

          <!-- Buffering Spinner -->
          {#if isBuffering}
            <div class="absolute inset-0 flex items-center justify-center bg-black/40 pointer-events-none z-10">
              <Loader2 class="w-12 h-12 text-cyan-400 animate-spin" />
            </div>
          {/if}

          <!-- Center Big Play Button when paused -->
          {#if !isPlaying && !isBuffering}
            <button
              type="button"
              onclick={togglePlay}
              class="absolute inset-0 flex items-center justify-center bg-black/30 hover:bg-black/20 transition-all z-10 cursor-pointer group"
            >
              <div class="w-20 h-20 rounded-full bg-cyan-400 hover:bg-cyan-300 text-black flex items-center justify-center shadow-[0_0_30px_rgba(34,211,238,0.5)] transform group-hover:scale-110 transition-transform">
                <Play class="w-9 h-9 fill-current ml-1" />
              </div>
            </button>
          {/if}

          <!-- Custom Bottom Control Bar -->
          <div
            class="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/95 via-black/70 to-transparent p-4 z-20 flex flex-col gap-2 transition-opacity duration-300 {showControls
              ? 'opacity-100'
              : 'opacity-0 pointer-events-none'}"
          >
            <!-- Progress Timeline Bar -->
            <div class="relative w-full flex items-center group/scrubber">
              <input
                type="range"
                min="0"
                max={duration || 100}
                step="0.1"
                value={currentTime}
                oninput={handleSeek}
                class="w-full h-1.5 bg-white/20 hover:h-2.5 rounded-lg appearance-none cursor-pointer accent-cyan-400 transition-all"
              />
            </div>

            <!-- Controls Row -->
            <div class="flex items-center justify-between pt-1">
              <!-- Left: Play/Pause, Rewind, Time, Volume -->
              <div class="flex items-center gap-3">
                <button
                  type="button"
                  onclick={togglePlay}
                  class="p-2 rounded-lg text-white hover:text-cyan-300 hover:bg-white/10 transition-colors cursor-pointer"
                  title={isPlaying ? 'Пауза (Пробіл)' : 'Відтворити (Пробіл)'}
                >
                  {#if isPlaying}
                    <Pause class="w-5 h-5 fill-current" />
                  {:else}
                    <Play class="w-5 h-5 fill-current" />
                  {/if}
                </button>

                <button
                  type="button"
                  onclick={() => {
                    if (videoElement) videoElement.currentTime = 0;
                  }}
                  class="p-2 rounded-lg text-slate-300 hover:text-white hover:bg-white/10 transition-colors cursor-pointer"
                  title="Спочатку"
                >
                  <RotateCcw class="w-4 h-4" />
                </button>

                <!-- Time display -->
                <div class="text-xs font-medium text-slate-300 tabular-nums">
                  <span class="text-white">{formatTime(currentTime)}</span>
                  <span class="text-slate-500"> / </span>
                  <span>{formatTime(duration)}</span>
                </div>

                <!-- Volume control -->
                <div class="flex items-center gap-1.5 group/vol pl-2">
                  <button
                    type="button"
                    onclick={toggleMute}
                    class="p-1.5 rounded-lg text-slate-300 hover:text-white hover:bg-white/10 transition-colors cursor-pointer"
                    title={isMuted ? 'Увімкнути звук (M)' : 'Вимкнути звук (M)'}
                  >
                    {#if isMuted || volume === 0}
                      <VolumeX class="w-4 h-4 text-rose-400" />
                    {:else}
                      <Volume2 class="w-4 h-4" />
                    {/if}
                  </button>

                  <input
                    type="range"
                    min="0"
                    max="1"
                    step="0.05"
                    value={isMuted ? 0 : volume}
                    oninput={handleVolumeChange}
                    class="w-16 h-1 bg-white/20 rounded-lg appearance-none cursor-pointer accent-cyan-400 opacity-80 hover:opacity-100 transition-opacity"
                  />
                </div>
              </div>

              <!-- Right: Source Link & Fullscreen -->
              <div class="flex items-center gap-2">
                <a
                  href={resolvedSrc}
                  target="_blank"
                  rel="noreferrer"
                  class="p-2 rounded-lg text-slate-400 hover:text-cyan-300 hover:bg-white/10 transition-colors cursor-pointer"
                  title="Відкрити в новій вкладці"
                >
                  <ExternalLink class="w-4 h-4" />
                </a>

                <button
                  type="button"
                  onclick={toggleFullscreen}
                  class="p-2 rounded-lg text-white hover:text-cyan-300 hover:bg-white/10 transition-colors cursor-pointer"
                  title="Повноекранний режим (F)"
                >
                  {#if isFullscreen}
                    <Minimize class="w-5 h-5" />
                  {:else}
                    <Maximize class="w-5 h-5" />
                  {/if}
                </button>
              </div>
            </div>
          </div>
        {:else}
          <div class="text-center py-20 text-slate-500 text-sm">
            Відеофайл недоступний
          </div>
        {/if}
      </div>
    </div>
  </div>
{/if}
