<script lang="ts">
import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { KeyRound, Mail, ArrowLeft, Send } from 'lucide-svelte';

  let identifier = $state('');
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();

    if (!identifier.trim()) {
      errorMessage = 'Введите email или логин';
      return;
    }

    errorMessage = '';
    isSubmitting = true;

    try {
      const res = await authStore.requestPasswordReset(identifier);
      uiStore.addToast({
        title: 'Код отправлен',
        message: res.debugCode
          ? `Код отправлен! Тестовый GUID: ${res.debugCode}`
          : (res.message || 'Код подтверждения отправлен на вашу почту.'),
        type: 'info'
      });
      uiStore.setTab('confirm-code');
    } catch (err: any) {
      errorMessage = err.message || 'Не удалось отправить код восстановления.';
    } finally {
      isSubmitting = false;
    }
  }
</script>

<div class="min-h-[80vh] flex items-center justify-center p-4 sm:p-6 lg:p-8">
  <div class="relative w-full max-w-md">

    <div class="absolute -top-10 -left-10 w-72 h-72 bg-cyan-500/15 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-10 -right-10 w-72 h-72 bg-teal-500/15 rounded-full blur-3xl pointer-events-none"></div>

    <div class="relative bg-[#09151e]/90 backdrop-blur-xl border border-cyan-500/30 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/80">

      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-[#030d12] border border-cyan-500/30 text-cyan-400 mb-4 shadow-lg">
          <KeyRound class="w-7 h-7" />
        </div>
        <h1 class="text-2xl sm:text-3xl font-black text-white font-display tracking-wide">
          Сброс пароля
        </h1>
        <p class="text-xs sm:text-sm text-slate-400 mt-2">
          Укажите вашу почту для получения кода восстановления
        </p>
      </div>

      {#if errorMessage}
        <div class="mb-6 p-3.5 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-5">

        <div>
          <label for="forgot-identifier" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-2">
            Email
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Mail class="w-4 h-4" />
            </div>
            <input
              id="forgot-identifier"
              type="text"
              bind:value={identifier}
              placeholder="user@example.com"
              required
              class="w-full pl-10 pr-4 py-3 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
            />
          </div>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          class="w-full py-3.5 px-4 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-sm tracking-wide shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
        >
          {#if isSubmitting}
            <div class="w-4 h-4 border-2 border-black border-t-transparent rounded-full animate-spin"></div>
            <span>Отправка...</span>
          {:else}
            <Send class="w-4 h-4 text-black" />
            <span>Отправить код</span>
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

