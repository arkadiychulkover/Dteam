<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import type { ToastMessage } from '../../stores/uiStore';
  import { CheckCircle2, Info, AlertTriangle, AlertCircle, X } from 'lucide-svelte';

  interface Props {
    toast: ToastMessage;
    onRemove: (id: string) => void;
  }

  let { toast, onRemove }: Props = $props();

  let isPaused = $state(false);
  let isExiting = $state(false);
  const totalMs = $derived(toast.duration || 7500);
  let remainingMs = $state(0);
  let progressPercent = $state(100);

  let animFrameId: number | null = null;
  let lastTimestamp = 0;

  const typeConfig = {
    success: {
      icon: CheckCircle2,
      tag: 'Успішно',
      tagColor: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/25',
      glow: 'shadow-[0_16px_40px_rgba(0,0,0,0.7),0_0_24px_rgba(16,185,129,0.18)]',
      border: 'border-emerald-500/35',
      bgGradient: 'from-emerald-950/40 via-[#08151c]/95 to-[#040e14]/98',
      pill: 'from-emerald-400 to-teal-400 shadow-[0_0_12px_rgba(16,185,129,0.7)]',
      iconBox: 'bg-emerald-500/15 border-emerald-500/30 text-emerald-400 shadow-[0_0_14px_rgba(16,185,129,0.25)]',
      progressBar: 'from-emerald-400 to-teal-400 shadow-[0_0_8px_rgba(16,185,129,0.8)]',
    },
    info: {
      icon: Info,
      tag: 'Сповіщення',
      tagColor: 'text-cyan-400 bg-cyan-500/10 border-cyan-500/25',
      glow: 'shadow-[0_16px_40px_rgba(0,0,0,0.7),0_0_24px_rgba(6,182,212,0.18)]',
      border: 'border-cyan-500/35',
      bgGradient: 'from-cyan-950/40 via-[#08151c]/95 to-[#040e14]/98',
      pill: 'from-cyan-400 to-sky-400 shadow-[0_0_12px_rgba(6,182,212,0.7)]',
      iconBox: 'bg-cyan-500/15 border-cyan-500/30 text-cyan-400 shadow-[0_0_14px_rgba(6,182,212,0.25)]',
      progressBar: 'from-cyan-400 to-sky-400 shadow-[0_0_8px_rgba(6,182,212,0.8)]',
    },
    warning: {
      icon: AlertTriangle,
      tag: 'Увага',
      tagColor: 'text-amber-400 bg-amber-500/10 border-amber-500/25',
      glow: 'shadow-[0_16px_40px_rgba(0,0,0,0.7),0_0_24px_rgba(245,158,11,0.18)]',
      border: 'border-amber-500/35',
      bgGradient: 'from-amber-950/40 via-[#08151c]/95 to-[#040e14]/98',
      pill: 'from-amber-400 to-orange-400 shadow-[0_0_12px_rgba(245,158,11,0.7)]',
      iconBox: 'bg-amber-500/15 border-amber-500/30 text-amber-400 shadow-[0_0_14px_rgba(245,158,11,0.25)]',
      progressBar: 'from-amber-400 to-orange-400 shadow-[0_0_8px_rgba(245,158,11,0.8)]',
    },
    error: {
      icon: AlertCircle,
      tag: 'Помилка',
      tagColor: 'text-rose-400 bg-rose-500/10 border-rose-500/25',
      glow: 'shadow-[0_16px_40px_rgba(0,0,0,0.7),0_0_24px_rgba(244,63,94,0.18)]',
      border: 'border-rose-500/35',
      bgGradient: 'from-rose-950/40 via-[#08151c]/95 to-[#040e14]/98',
      pill: 'from-rose-400 to-red-500 shadow-[0_0_12px_rgba(244,63,94,0.7)]',
      iconBox: 'bg-rose-500/15 border-rose-500/30 text-rose-400 shadow-[0_0_14px_rgba(244,63,94,0.25)]',
      progressBar: 'from-rose-400 to-red-500 shadow-[0_0_8px_rgba(244,63,94,0.8)]',
    },
  };

  const config = $derived(typeConfig[toast.type] || typeConfig.info);
  const IconComponent = $derived(config.icon);

  function triggerExit() {
    if (isExiting) return;
    isExiting = true;
    setTimeout(() => {
      onRemove(toast.id);
    }, 260);
  }

  function tick(timestamp: number) {
    if (!lastTimestamp) lastTimestamp = timestamp;
    const delta = timestamp - lastTimestamp;
    lastTimestamp = timestamp;

    if (!isPaused && !isExiting) {
      remainingMs -= delta;
      progressPercent = Math.max(0, (remainingMs / totalMs) * 100);

      if (remainingMs <= 0) {
        triggerExit();
        return;
      }
    }

    animFrameId = requestAnimationFrame(tick);
  }

  function handleMouseEnter() {
    isPaused = true;
  }

  function handleMouseLeave() {
    isPaused = false;
    lastTimestamp = performance.now();
  }

  onMount(() => {
    remainingMs = totalMs;
    lastTimestamp = performance.now();
    animFrameId = requestAnimationFrame(tick);
  });

  onDestroy(() => {
    if (animFrameId !== null) {
      cancelAnimationFrame(animFrameId);
    }
  });
</script>

<div
  role="alert"
  aria-live="polite"
  onmouseenter={handleMouseEnter}
  onmouseleave={handleMouseLeave}
  class="pointer-events-auto relative overflow-hidden rounded-2xl border backdrop-blur-2xl transition-all duration-300 w-full select-none
    {config.bgGradient} {config.border} {config.glow}
    {isExiting ? 'toast-exit' : 'toast-enter'}"
>

  <div class="pointer-events-none absolute -left-12 -top-12 w-36 h-36 rounded-full bg-white/5 blur-2xl"></div>

  <div class="relative p-4 flex items-start gap-3.5">

    <div class="w-1 self-stretch rounded-full bg-gradient-to-b {config.pill} shrink-0 my-0.5"></div>

    <div class="w-9 h-9 rounded-xl flex items-center justify-center shrink-0 border {config.iconBox} mt-0.5">
      <IconComponent class="w-5 h-5" />
    </div>

    <div class="flex-1 min-w-0 pr-1">
      <div class="flex items-center gap-2 flex-wrap">
        <span class="px-1.5 py-0.5 rounded text-[9px] font-black uppercase tracking-wider border font-mono {config.tagColor}">
          {config.tag}
        </span>
        {#if isPaused}
          <span class="text-[9px] font-mono text-slate-400/80 animate-pulse">
            • пауза
          </span>
        {/if}
      </div>

      <h4 class="text-sm font-bold text-white tracking-tight mt-1 leading-snug font-display">
        {toast.title}
      </h4>

      <p class="text-xs text-slate-300/90 leading-relaxed font-normal mt-1 break-words">
        {toast.message}
      </p>
    </div>

    <button
      type="button"
      onclick={triggerExit}
      class="text-slate-400 hover:text-white p-1 rounded-lg hover:bg-white/10 transition-colors shrink-0 -mr-1 -mt-1 cursor-pointer"
      title="Закрити"
    >
      <X class="w-4 h-4" />
    </button>
  </div>

  <div class="h-[2.5px] w-full bg-slate-800/60 overflow-hidden">
    <div
      class="h-full bg-gradient-to-r {config.progressBar} transition-all duration-75 ease-linear"
      style="width: {progressPercent}%;"
    ></div>
  </div>
</div>

<style>
  @keyframes toastEnter {
    0% {
      opacity: 0;
      transform: translate3d(32px, 8px, 0) scale(0.95);
      filter: blur(4px);
    }
    60% {
      opacity: 1;
      transform: translate3d(-3px, 0, 0) scale(1.005);
      filter: blur(0px);
    }
    100% {
      opacity: 1;
      transform: translate3d(0, 0, 0) scale(1);
      filter: blur(0px);
    }
  }

  @keyframes toastExit {
    0% {
      opacity: 1;
      transform: translate3d(0, 0, 0) scale(1);
      max-height: 160px;
      margin-bottom: 0px;
    }
    100% {
      opacity: 0;
      transform: translate3d(40px, 0, 0) scale(0.92);
      max-height: 0px;
      margin-bottom: -12px;
      padding-top: 0;
      padding-bottom: 0;
    }
  }

  .toast-enter {
    animation: toastEnter 340ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
  }

  .toast-exit {
    animation: toastExit 260ms cubic-bezier(0.4, 0, 1, 1) forwards;
  }
</style>
