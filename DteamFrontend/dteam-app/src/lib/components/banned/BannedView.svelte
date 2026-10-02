<script lang="ts">
  import { onMount } from 'svelte';
  import { authStore, currentUser } from '../../stores/authStore';
  import { uiStore } from '../../stores/uiStore';
  import { userService } from '../../services/userService';
  import { Ban, ShieldAlert, ShieldCheck, LogOut, RefreshCw } from 'lucide-svelte';

  interface Props { onRetry?: () => void; }

  let { onRetry }: Props = $props();
  let isChecking = $state(false);
  let isAdmin = $state(false);

  async function checkAdminStatus() {
    try {
      const res = await userService.checkIsAdmin($currentUser?.id);
      if (res && res.isAdmin) {
        isAdmin = true;
      } else if ($currentUser?.isAdmin) {
        isAdmin = true;
      } else {
        isAdmin = false;
      }
    } catch (e: any) {
      if (e?.isAdmin || $currentUser?.isAdmin) { isAdmin = true; } else { isAdmin = false; }
    }
  }

  onMount(() => { checkAdminStatus(); });

  async function handleCheckAgain() {
    isChecking = true;
    await checkAdminStatus();
    if (onRetry) { await onRetry(); }
    setTimeout(() => (isChecking = false), 1000);
  }

  async function handleLogout() { await authStore.logout(); window.location.reload(); }

  function handleGoToAdmin() { uiStore.setTab('admin'); }
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
        <span class="text-rose-300 font-medium">Все клиентские запросы к платформе отклоняются</span>
      </div>
      {#if isAdmin}
        <div class="flex justify-between items-center text-amber-300 font-bold bg-amber-500/10 p-2.5 rounded-xl border border-amber-500/30 mt-2">
          <span class="flex items-center gap-1.5 text-amber-300">
            <ShieldCheck class="w-4 h-4 text-amber-400" />
            Привилегии:
          </span>
          <span class="text-amber-400 uppercase text-[11px] font-mono">Администратор (доступ разрешен)</span>
        </div>
      {/if}
    </div>

    <div class="flex flex-col sm:flex-row items-center justify-center gap-3">
      {#if isAdmin}
        <button
          onclick={handleGoToAdmin}
          class="w-full sm:w-auto flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl bg-gradient-to-r from-amber-500 via-orange-500 to-amber-600 hover:from-amber-400 hover:via-orange-400 hover:to-amber-500 text-slate-950 font-black text-xs shadow-xl shadow-amber-500/25 transition-all transform hover:scale-[1.02] cursor-pointer"
        >
          <ShieldCheck class="w-4 h-4 text-slate-950" />
          <span>Перейти в админку</span>
        </button>
      {/if}

      <button
        onclick={handleCheckAgain}
        disabled={isChecking}
        class="w-full sm:w-auto flex items-center justify-center gap-2 px-5 py-2.5 rounded-xl bg-slate-900 hover:bg-slate-800 border border-slate-700 text-xs font-semibold text-slate-200 transition-all cursor-pointer"
      >
        <RefreshCw class="w-4 h-4 {isChecking ? 'animate-spin text-rose-400' : ''}" />
        <span>Проверить статус</span>
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
