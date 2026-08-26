<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { ShieldCheck, ArrowLeft, RefreshCw, CheckCircle } from 'lucide-svelte';
  import { onMount, onDestroy } from 'svelte';

  let codeDigits = $state(['', '', '', '', '', '']);
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

  function handleInput(index: number, e: Event) {
    const target = e.target as HTMLInputElement;
    const val = target.value;
    
    // Only take the last character
    if (val.length > 0) {
      codeDigits[index] = val[val.length - 1];
      // Focus next input if available
      if (index < 5) {
        const nextInput = document.getElementById(`code-input-${index + 1}`) as HTMLInputElement;
        if (nextInput) nextInput.focus();
      }
    }
  }

  function handleKeyDown(index: number, e: KeyboardEvent) {
    if (e.key === 'Backspace' && !codeDigits[index] && index > 0) {
      const prevInput = document.getElementById(`code-input-${index - 1}`) as HTMLInputElement;
      if (prevInput) {
        prevInput.focus();
        codeDigits[index - 1] = '';
      }
    }
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
        message: res.debugCode ? `Новый код: ${res.debugCode}` : 'Новый код отправлен на ваш адрес.',
        type: 'info'
      });
      startCountdown();
    } catch (err: any) {
      errorMessage = err.message || 'Не удалось повторно отправить код.';
    }
  }

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    const fullCode = codeDigits.join('');

    if (fullCode.length < 6) {
      errorMessage = 'Введите полный 6-значный код';
      return;
    }

    errorMessage = '';
    isSubmitting = true;

    try {
      await authStore.verifyResetCode(fullCode);
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

<div class="min-h-[85vh] flex items-center justify-center p-4 sm:p-6 lg:p-8">
  <div class="relative w-full max-w-md">
    <div class="absolute -top-10 -right-10 w-72 h-72 bg-cyan-500/15 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-10 -left-10 w-72 h-72 bg-blue-600/15 rounded-full blur-3xl pointer-events-none"></div>

    <div class="relative bg-[#111422]/90 backdrop-blur-xl border border-slate-800 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/30">

      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-cyan-500/10 border border-cyan-500/30 text-cyan-400 mb-4 shadow-lg shadow-cyan-500/10">
          <ShieldCheck class="w-7 h-7" />
        </div>
        <h1 class="text-2xl sm:text-3xl font-extrabold text-white font-['Outfit'] tracking-wide">
          Подтверждение кода
        </h1>
        <p class="text-xs sm:text-sm text-slate-400 mt-2">
          Введите 6-значный код, отправленный на ваш email или логин
        </p>
      </div>

      {#if errorMessage}
        <div class="mb-6 p-3.5 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-6">
        <div class="flex justify-center gap-2 sm:gap-3">
          {#each codeDigits as digit, i}
            <input
              id="code-input-{i}"
              type="text"
              inputmode="numeric"
              maxlength="1"
              value={digit}
              oninput={(e) => handleInput(i, e)}
              onkeydown={(e) => handleKeyDown(i, e)}
              class="w-11 h-13 sm:w-12 sm:h-14 bg-slate-900/90 border border-slate-800 rounded-xl text-center text-xl font-mono font-extrabold text-cyan-400 focus:outline-none focus:border-cyan-400 focus:ring-2 focus:ring-cyan-500/30 transition-all shadow-inner"
            />
          {/each}
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
          class="w-full py-3.5 px-4 rounded-xl bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white font-bold text-sm tracking-wide shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
        >
          {#if isSubmitting}
            <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
            <span>Проверка...</span>
          {:else}
            <CheckCircle class="w-4 h-4" />
            <span>Подтвердить код</span>
          {/if}
        </button>
      </form>

      <div class="mt-8 pt-6 border-t border-slate-800/80 text-center">
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
