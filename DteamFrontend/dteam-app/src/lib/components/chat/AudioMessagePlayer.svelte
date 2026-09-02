<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { Play, Pause } from 'lucide-svelte';
  import { chatStore } from '../../stores/chatStore';

  interface Props {
    src: string;
    messageId: string;
    durationSeconds?: number | null;
    isOutgoing?: boolean;
    compact?: boolean;
  }

  let { src, messageId, durationSeconds = 10, isOutgoing = false, compact = false }: Props = $props();

  let audio: HTMLAudioElement | null = null;
  let isPlaying = $state(false);
  let currentTime = $state(0);
  let totalDuration = $state(10);
  let isDragging = $state(false);

  const resolvedSrc = $derived.by(() => {
    if (!src) return '';
    if (src.startsWith('http://') || src.startsWith('https://') || src.startsWith('blob:') || src.startsWith('data:')) {
      return src;
    }
    return src.startsWith('/') ? src : `/${src}`;
  });

  $effect(() => {
    if (durationSeconds && durationSeconds > 0) {
      totalDuration = durationSeconds;
    }
  });

  // Sync with global player: pause if another audio started
  $effect(() => {
    if ($chatStore.currentPlayingVoiceId !== messageId && isPlaying) {
      pauseAudio();
    }
  });

  $effect(() => {
    if (audio && resolvedSrc && audio.src !== resolvedSrc) {
      audio.src = resolvedSrc;
    }
  });

  onMount(() => {
    if (resolvedSrc) {
      audio = new Audio(resolvedSrc);
      setupAudioListeners();
    }

    return () => {
      if (audio) {
        audio.pause();
        audio = null;
      }
    };
  });

  function setupAudioListeners() {
    if (!audio) return;

    audio.addEventListener('loadedmetadata', () => {
      if (audio && audio.duration && !isNaN(audio.duration) && isFinite(audio.duration)) {
        totalDuration = Math.round(audio.duration);
      }
    });

    audio.addEventListener('timeupdate', () => {
      if (!isDragging && audio) {
        currentTime = audio.currentTime;
      }
    });

    audio.addEventListener('ended', () => {
      isPlaying = false;
      currentTime = 0;
      chatStore.setPlayingVoiceId(null);
    });

    audio.addEventListener('error', (e) => {
      console.warn('[AudioPlayer] Audio error:', e);
      isPlaying = false;
    });
  }

  function togglePlay() {
    if (!audio && resolvedSrc) {
      audio = new Audio(resolvedSrc);
      setupAudioListeners();
    }

    if (isPlaying) {
      pauseAudio();
    } else {
      chatStore.setPlayingVoiceId(messageId);
      audio.play().then(() => {
        isPlaying = true;
      }).catch(err => {
        console.warn('[AudioPlayer] Playback failed:', err);
        isPlaying = false;
      });
    }
  }

  function pauseAudio() {
    if (audio) {
      audio.pause();
    }
    isPlaying = false;
    if ($chatStore.currentPlayingVoiceId === messageId) {
      chatStore.setPlayingVoiceId(null);
    }
  }

  function handleSeek(e: Event) {
    const input = e.target as HTMLInputElement;
    const seekTo = parseFloat(input.value);
    currentTime = seekTo;
    if (audio) {
      audio.currentTime = seekTo;
    }
  }

  function formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }

  const progressPercent = $derived(
    totalDuration > 0 ? Math.min(100, Math.max(0, (currentTime / totalDuration) * 100)) : 0
  );
</script>

<div class="flex items-center gap-3 w-full {compact ? 'max-w-xs' : 'max-w-sm'} select-none">
  <!-- Play/Pause Button -->
  <button
    type="button"
    onclick={togglePlay}
    class="w-10 h-10 rounded-full flex items-center justify-center shrink-0 cursor-pointer transition-all duration-200 shadow-md
      {isPlaying 
        ? 'bg-cyan-400 text-black shadow-cyan-400/40 scale-105' 
        : 'bg-[#0df2c9] text-black hover:bg-[#00ffa3] hover:scale-105 shadow-[#0df2c9]/30'}"
    title={isPlaying ? 'Пауза' : 'Відтворити'}
  >
    {#if isPlaying}
      <Pause class="w-4 h-4 fill-black" />
    {:else}
      <Play class="w-4 h-4 fill-black translate-x-0.5" />
    {/if}
  </button>

  <!-- Progress Bar & Timer -->
  <div class="flex-1 flex flex-col gap-1">
    <div class="relative w-full h-4 flex items-center group cursor-pointer">
      <!-- Background track -->
      <div class="absolute inset-x-0 h-1.5 rounded-full bg-slate-700/60 overflow-hidden">
        <!-- Filled progress -->
        <div 
          class="h-full bg-gradient-to-r from-cyan-400 to-[#0df2c9] rounded-full transition-all duration-75"
          style="width: {progressPercent}%"
        ></div>
      </div>

      <!-- Range input on top for seeking -->
      <input
        type="range"
        min="0"
        max={totalDuration}
        step="0.1"
        value={currentTime}
        oninput={handleSeek}
        class="absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10"
      />

      <!-- Scrubber Dot thumb -->
      <div 
        class="absolute w-3.5 h-3.5 rounded-full bg-white shadow-[0_0_8px_rgba(13,242,201,0.8)] pointer-events-none -translate-x-1/2 transition-transform group-hover:scale-125"
        style="left: {progressPercent}%"
      ></div>
    </div>

    <!-- Duration display -->
    <div class="flex justify-between items-center text-[11px] font-mono font-medium text-slate-300/80">
      <span>{formatTime(isPlaying || currentTime > 0 ? currentTime : totalDuration)}</span>
    </div>
  </div>
</div>
