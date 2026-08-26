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
      errorMessage = 'Введите логин или email';
      return;
    }

    errorMessage = '';
    isSubmitting = true;

    try {
      const res = await authStore.requestPasswordReset(identifier);
      uiStore.addToast({
        title: 'Код отправлен',
        message: res.debugCode 
          ? `Код отправлен! Тестовый код: ${res.debugCode}` 
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

<div class="min-h-[85vh] flex items-center justify-center p-4 sm:p-6 lg:p-8">
  <div class="relative w-full max-w-md">
    <!-- Glow Backdrops -->
    <div class="absolute -top-10 -left-10 w-72 h-72 bg-cyan-500/15 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-10 -right-10 w-72 h-72 bg-blue-600/15 rounded-full blur-3xl pointer-events-none"></div>

    <div class="relative bg-[#111422]/90 backdrop-blur-xl border border-slate-800 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/30">
      <!-- Header -->
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-slate-800/80 border border-slate-700 text-cyan-400 mb-4 shadow-lg">
          <KeyRound class="w-7 h-7" />
        </div>
        <h1 class="text-2xl sm:text-3xl font-extrabold text-white font-['Outfit'] tracking-wide">
          Восстановление пароля
        </h1>
        <p class="text-xs sm:text-sm text-slate-400 mt-2">
          Укажите ваш логин или почту для получения кода сброса пароля
        </p>
      </div>

      {#if errorMessage}
        <div class="mb-6 p-3.5 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-5">
        <!-- Email/Username Input -->
        <div>
          <label for="forgot-identifier" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-2">
            Логин или Email
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Mail class="w-4 h-4" />
            </div>
            <input
              id="forgot-identifier"
              type="text"
              bind:value={identifier}
              placeholder="user@example.com или логин"
              required
              class="w-full pl-10 pr-4 py-3 bg-slate-900/80 border border-slate-800 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500 focus:ring-1 focus:ring-cyan-500 transition-all"
            />
          </div>
        </div>

        <!-- Submit Button -->
        <button
          type="submit"
          disabled={isSubmitting}
          class="w-full py-3.5 px-4 rounded-xl bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white font-bold text-sm tracking-wide shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
        >
          {#if isSubmitting}
            <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
            <span>Отправка...</span>
          {:else}
            <Send class="w-4 h-4" />
            <span>Отправить код</span>
          {/if}
        </button>
      </form>

      <!-- Back to Login Link -->
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
