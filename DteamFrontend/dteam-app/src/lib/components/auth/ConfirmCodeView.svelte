<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { ShieldCheck, ArrowLeft, RefreshCw, CheckCircle, Key } from 'lucide-svelte';
  import { onMount, onDestroy } from 'svelte';

  let resetCode = $state('');
  let isSubmitting = $state(false);
  let errorMessage = $state('');
  let resendCountdown = $state(60);
  let timer: any = null;

  onMount(() => {
    startCountdown();
  });

  onDestroy(() => {
    if (timer) clearInterval(timer);
  });

  function startCountdown() {
    resendCountdown = 60;
    if (timer) clearInterval(timer);
    timer = setInterval(() => {
      if (resendCountdown > 0) {
        resendCountdown -= 1;
      } else {
        clearInterval(timer);
      }
    }, 1000);
  }

  async function handleResend() {
    if (resendCountdown > 0) return;
    let currentEmail = '';
    const unsub = authStore.subscribe((s) => { currentEmail = s.resetEmail || ''; });
    unsub();

    if (!currentEmail) {
      uiStore.setTab('forgot-password');
      return;
    }

    try {
      const res = await authStore.requestPasswordReset(currentEmail);
      uiStore.addToast({
        title: 'Код переотправлен',
        message: res.debugCode ? `Новый GUID код: ${res.debugCode}` : 'Новый код отправлен на ваш адрес.',
        type: 'info'
      });
      startCountdown();
    } catch (err: any) {
      errorMessage = err.message || 'Не удалось повторно отправить код.';
    }
  }

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    const trimmedCode = resetCode.trim();

    if (!trimmedCode) {
      errorMessage = 'Введите код подтверждения (GUID)';
      return;
    }

    errorMessage = '';
    isSubmitting = true;

    try {
      await authStore.verifyResetCode(trimmedCode);
      uiStore.addToast({
        title: 'Код подтвержден',
        message: 'Теперь установите новый пароль.',
        type: 'success'
      });
      uiStore.setTab('reset-password');
    } catch (err: any) {
      errorMessage = err.message || 'Неверный или истекший код подтверждения.';
    } finally {
      isSubmitting = false;
    }
  }
</script>

<div class="min-h-[80vh] flex items-center justify-center p-4 sm:p-6 lg:p-8">
  <div class="relative w-full max-w-md">
    <div class="absolute -top-10 -right-10 w-72 h-72 bg-cyan-500/15 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-10 -left-10 w-72 h-72 bg-teal-500/15 rounded-full blur-3xl pointer-events-none"></div>

    <div class="relative bg-[#09151e]/90 backdrop-blur-xl border border-cyan-500/30 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/80">

      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-cyan-500/10 border border-cyan-500/30 text-cyan-400 mb-4 shadow-lg shadow-cyan-500/10">
          <ShieldCheck class="w-7 h-7" />
        </div>
        <h1 class="text-2xl sm:text-3xl font-black text-white font-display tracking-wide">
          Подтверждение кода
        </h1>
        <p class="text-xs sm:text-sm text-slate-400 mt-2">
          Введите код подтверждения из письма
        </p>
      </div>

      {#if errorMessage}
        <div class="mb-6 p-3.5 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-6">
        <div>
          <label for="code-input" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-2">
            Код подтверждения
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Key class="w-4 h-4" />
            </div>
            <input
              id="code-input"
              type="text"
              bind:value={resetCode}
              placeholder="3fa85f64-5717-4562-b3fc-2c963f66afa6"
              required
              class="w-full pl-10 pr-4 py-3 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm font-mono text-cyan-300 placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
            />
          </div>
        </div>

        <div class="text-center">
          {#if resendCountdown > 0}
            <p class="text-xs text-slate-500 flex items-center justify-center gap-1">
              <span>Отправить повторно через</span>
              <span class="font-mono text-cyan-400 font-bold">{resendCountdown}с</span>
            </p>
          {:else}
            <button
              type="button"
              onclick={handleResend}
              class="inline-flex items-center gap-1.5 text-xs font-semibold text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer"
            >
              <RefreshCw class="w-3.5 h-3.5" />
              <span>Отправить код повторно</span>
            </button>
          {/if}
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          class="w-full py-3.5 px-4 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-sm tracking-wide shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
        >
          {#if isSubmitting}
            <div class="w-4 h-4 border-2 border-black border-t-transparent rounded-full animate-spin"></div>
            <span>Проверка...</span>
          {:else}
            <CheckCircle class="w-4 h-4 text-black" />
            <span>Подтвердить код</span>
          {/if}
        </button>
      </form>

      <div class="mt-8 pt-6 border-t border-cyan-950/80 text-center">
        <button
          onclick={() => uiStore.setTab('login')}
          class="inline-flex items-center gap-2 text-xs font-bold text-slate-400 hover:text-white transition-colors cursor-pointer"
        >
          <ArrowLeft class="w-4 h-4" />
          <span>Вернуться ко входу</span>
        </button>
      </div>
    </div>
  </div>
</div>
