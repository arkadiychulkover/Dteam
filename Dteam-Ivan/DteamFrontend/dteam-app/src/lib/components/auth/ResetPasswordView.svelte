<script lang="ts">
import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { Lock, Eye, EyeOff, CheckCircle2, ArrowRight } from 'lucide-svelte';

  let newPassword = $state('');
  let confirmPassword = $state('');
  let showPassword = $state(false);
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();

    if (!newPassword || !confirmPassword) {
      errorMessage = 'Заполните оба поля';
      return;
    }

    if (newPassword !== confirmPassword) {
      errorMessage = 'Пароли не совпадают';
      return;
    }

    if (newPassword.length < 6) {
      errorMessage = 'Пароль должен быть не менее 6 символов';
      return;
    }

    errorMessage = '';
    isSubmitting = true;

    try {
      await authStore.resetPassword(newPassword);
      uiStore.addToast({
        title: 'Пароль изменен',
        message: 'Ваш пароль успешно обновлен. Войдите с новым паролем.',
        type: 'success'
      });
      uiStore.setTab('login');
    } catch (err: any) {
      errorMessage = err.message || 'Не удалось обновить пароль.';
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
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-cyan-500/10 border border-cyan-500/30 text-cyan-400 mb-4 shadow-lg">
          <Lock class="w-7 h-7" />
        </div>
        <h1 class="text-2xl sm:text-3xl font-black text-white font-display tracking-wide">
          Создание нового пароля
        </h1>
        <p class="text-xs sm:text-sm text-slate-400 mt-2">
          Придумайте надежный пароль для вашей учетной записи
        </p>
      </div>

      {#if errorMessage}
        <div class="mb-6 p-3.5 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-5">

        <div>
          <label for="reset-new-password" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-2">
            Новый пароль
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Lock class="w-4 h-4" />
            </div>
            <input
              id="reset-new-password"
              type={showPassword ? 'text' : 'password'}
              bind:value={newPassword}
              placeholder="••••••••••••"
              required
              class="w-full pl-10 pr-11 py-3 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
            />
            <button
              type="button"
              onclick={() => showPassword = !showPassword}
              class="absolute inset-y-0 right-0 pr-3.5 flex items-center text-slate-500 hover:text-slate-300 transition-colors cursor-pointer"
            >
              {#if showPassword}
                <EyeOff class="w-4 h-4" />
              {:else}
                <Eye class="w-4 h-4" />
              {/if}
            </button>
          </div>
        </div>

        <div>
          <label for="reset-confirm-password" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-2">
            Подтвердите новый пароль
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Lock class="w-4 h-4" />
            </div>
            <input
              id="reset-confirm-password"
              type={showPassword ? 'text' : 'password'}
              bind:value={confirmPassword}
              placeholder="••••••••••••"
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
            <span>Сохранение...</span>
          {:else}
            <CheckCircle2 class="w-4 h-4 text-black" />
            <span>Сохранить пароль</span>
            <ArrowRight class="w-4 h-4 ml-1 text-black" />
          {/if}
        </button>
      </form>
    </div>
  </div>
</div>

