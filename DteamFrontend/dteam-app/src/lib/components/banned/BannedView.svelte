<script lang="ts">
import { authStore, currentUser } from '../../stores/authStore';
  import { uiStore } from '../../stores/uiStore';
  import { Ban, ShieldAlert, AlertTriangle, LogOut, RefreshCw, Mail } from 'lucide-svelte';

  interface Props {
    onRetry?: () => void;
  }

  let { onRetry }: Props = $props();
  let isChecking = $state(false);

  async function handleCheckAgain() {
    isChecking = true;
    if (onRetry) {
      await onRetry();
    }
    setTimeout(() => (isChecking = false), 1000);
  }

  async function handleLogout() {
    await authStore.logout();
    window.location.reload();
  }
</script>

<div class="min-h-[85vh] flex items-center justify-center p-4">
  <div class="relative w-full max-w-xl bg-[#140609]/90 border border-rose-500/40 rounded-3xl shadow-2xl shadow-rose-950/60 p-8 text-center overflow-hidden animate-in fade-in zoom-in-95">
    <div class="absolute -top-24 -left-24 w-60 h-60 bg-rose-600/15 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-24 -right-24 w-60 h-60 bg-rose-600/15 rounded-full blur-3xl pointer-events-none"></div>

    <div class="relative w-20 h-20 rounded-3xl bg-rose-950/80 border-2 border-rose-500/60 flex items-center justify-center text-rose-400 mx-auto mb-6 shadow-xl shadow-rose-950/80 animate-pulse">
      <Ban class="w-10 h-10 text-rose-400" />
      <span class="absolute -top-1 -right-1 w-5 h-5 rounded-full bg-rose-600 flex items-center justify-center text-white text-xs font-black">
        !
      </span>
    </div>

    <div class="inline-flex items-center gap-1.5 px-3.5 py-1 rounded-full bg-rose-950/90 border border-rose-500/40 text-rose-300 text-xs font-bold uppercase tracking-widest mb-3">
      <ShieldAlert class="w-3.5 h-3.5 text-rose-400" /> Доступ заблокирован
    </div>

    <h1 class="text-2xl sm:text-3xl font-black text-white tracking-tight leading-tight">
      Ваш аккаунт заблокирован
    </h1>

    <p class="text-xs sm:text-sm text-rose-200/80 mt-3 leading-relaxed max-w-md mx-auto">
      Администрация платформы ограничила доступ для пользователя
      <strong class="text-white font-mono bg-rose-950/60 px-1.5 py-0.5 rounded border border-rose-500/30">
        {$currentUser?.username || 'Gamer'}
      </strong>
      в связи с нарушением правил сообщества или блокировкой учетной записи.
    </p>

    <div class="my-6 p-4 rounded-2xl bg-[#1f090d] border border-rose-500/20 text-left text-xs space-y-2">
      <div class="flex justify-between text-rose-300/70">
        <span>Статус аккаунта:</span>
        <span class="text-rose-400 font-bold uppercase">IsBanned = True</span>
      </div>
      {#if $currentUser?.id}
        <div class="flex justify-between text-rose-300/70">
          <span>ID пользователя:</span>
          <span class="font-mono text-slate-300 text-[11px]">{$currentUser.id}</span>
        </div>
      {/if}
      <div class="flex justify-between text-rose-300/70">
        <span>Действие:</span>
        <span class="text-rose-300 font-medium">Все запросы к API отклоняются</span>
      </div>
    </div>

    <div class="flex flex-col sm:flex-row items-center justify-center gap-3">
      <button
        onclick={handleCheckAgain}
        disabled={isChecking}
        class="w-full sm:w-auto flex items-center justify-center gap-2 px-5 py-2.5 rounded-xl bg-slate-900 hover:bg-slate-800 border border-slate-700 text-xs font-semibold text-slate-200 transition-all cursor-pointer"
      >
        <RefreshCw class="w-4 h-4 {isChecking ? 'animate-spin text-rose-400' : ''}" />
        <span>Проверить статус разблокировки</span>
      </button>

      <button
        onclick={handleLogout}
        class="w-full sm:w-auto flex items-center justify-center gap-2 px-5 py-2.5 rounded-xl bg-rose-600 hover:bg-rose-500 text-white font-bold text-xs shadow-lg shadow-rose-600/30 transition-all cursor-pointer"
      >
        <LogOut class="w-4 h-4" />
        <span>Выйти из аккаунта</span>
      </button>
    </div>
  </div>
</div>

