<script lang="ts">
import { uiStore } from '../../stores/uiStore';
  import { X, CheckCircle, Info, AlertTriangle, AlertCircle } from 'lucide-svelte';

  const icons = {
    success: CheckCircle,
    info: Info,
    warning: AlertTriangle,
    error: AlertCircle,
  };
</script>

<div class="fixed bottom-6 right-6 z-50 flex flex-col gap-2 pointer-events-none max-w-sm w-full">
  {#each $uiStore.toasts as toast (toast.id)}
    {@const Icon = icons[toast.type]}
    <div
      class="pointer-events-auto flex items-start gap-3 p-4 rounded-xl border shadow-2xl backdrop-blur-md transition-all duration-300 animate-slide-up
        {toast.type === 'success' ? 'bg-emerald-950/80 border-emerald-500/40 text-emerald-100' : ''}
        {toast.type === 'info' ? 'bg-cyan-950/80 border-cyan-500/40 text-cyan-100' : ''}
        {toast.type === 'warning' ? 'bg-amber-950/80 border-amber-500/40 text-amber-100' : ''}
        {toast.type === 'error' ? 'bg-rose-950/80 border-rose-500/40 text-rose-100' : ''}"
    >
      <Icon class="w-5 h-5 shrink-0 mt-0.5" />
      <div class="flex-1">
        <h4 class="text-sm font-semibold">{toast.title}</h4>
        <p class="text-xs opacity-90 mt-0.5 leading-relaxed">{toast.message}</p>
      </div>
      <button
        onclick={() => uiStore.removeToast(toast.id)}
        class="text-white/60 hover:text-white transition-colors"
      >
        <X class="w-4 h-4" />
      </button>
    </div>
  {/each}
</div>

