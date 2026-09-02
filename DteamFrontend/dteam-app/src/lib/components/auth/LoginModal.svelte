<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { X, LogIn, Mail, Lock, Eye, EyeOff, Gamepad2, ArrowRight } from 'lucide-svelte';

  let identifier = $state('');
  let password = $state('');
  let rememberMe = $state(true);
  let showPassword = $state(false);
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  function closeModal() {
    uiStore.setLoginModal(false);
  }

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    if (!identifier.trim() || !password) {
      errorMessage = 'Пожалуйста, заполните все поля';
      return;
    }

    errorMessage = '';
    isSubmitting = true;

    try {
      await authStore.login(identifier, password);
      uiStore.addToast({
        title: 'Авторизация успешна',
        message: `Приветствуем, ${identifier}!`,
        type: 'success'
      });
      closeModal();
    } catch (err: any) {
      errorMessage = err.message || 'Ошибка входа. Проверьте введенные данные.';
    } finally {
      isSubmitting = false;
    }
  }

  function handleBackdropClick(e: MouseEvent) {
    if (e.target === e.currentTarget) {
      closeModal();
    }
  }
</script>

{#if $uiStore.isLoginModalOpen}
  
  <div 
    onclick={handleBackdropClick}
    role="presentation"
    class="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center p-4 animate-in fade-in duration-200"
  >
    <div class="relative w-full max-w-md max-h-[90vh] overflow-y-auto bg-[#09151e] border border-cyan-500/30 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/80 animate-in zoom-in-95 duration-200">
      
      <button
        onclick={closeModal}
        class="absolute top-4 right-4 p-2 text-slate-400 hover:text-white rounded-xl hover:bg-slate-800/60 transition-colors cursor-pointer"
      >
        <X class="w-5 h-5" />
      </button>

      <div class="text-center mb-6">
        <div class="inline-flex items-center justify-center w-12 h-12 rounded-2xl bg-gradient-to-br from-cyan-400 via-teal-500 to-emerald-400 shadow-md shadow-cyan-500/20 mb-3">
          <Gamepad2 class="w-6 h-6 text-black font-black" />
        </div>
        <h2 class="text-xl sm:text-2xl font-extrabold text-white font-display">
          Быстрый вход в <span class="text-cyan-400">DTEAM</span>
        </h2>
      </div>

      {#if errorMessage}
        <div class="mb-4 p-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-4">
        
        <div>
          <label for="modal-login-id" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Логин / Email
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Mail class="w-4 h-4" />
            </div>
            <input
              id="modal-login-id"
              type="text"
              bind:value={identifier}
              placeholder="user@example.com или логин"
              required
              class="w-full pl-10 pr-4 py-2.5 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
            />
          </div>
        </div>

        <div>
          <div class="flex items-center justify-between mb-1.5">
            <label for="modal-login-password" class="block text-xs font-bold text-slate-300 uppercase tracking-wider">
              Пароль
            </label>
            <button
              type="button"
              onclick={() => { closeModal(); uiStore.setTab('forgot-password'); }}
              class="text-xs font-semibold text-cyan-400 hover:text-cyan-300 cursor-pointer"
            >
              Забыли пароль?
            </button>
          </div>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Lock class="w-4 h-4" />
            </div>
            <input
              id="modal-login-password"
              type={showPassword ? 'text' : 'password'}
              bind:value={password}
              placeholder="••••••••••••"
              required
              class="w-full pl-10 pr-10 py-2.5 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
            />
            <button
              type="button"
              onclick={() => showPassword = !showPassword}
              class="absolute inset-y-0 right-0 pr-3 flex items-center text-slate-500 hover:text-slate-300 transition-colors cursor-pointer"
            >
              {#if showPassword}
                <EyeOff class="w-4 h-4" />
              {:else}
                <Eye class="w-4 h-4" />
              {/if}
            </button>
          </div>
        </div>

        <div class="flex items-center justify-between pt-1">
          <label class="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              bind:checked={rememberMe}
              class="w-4 h-4 rounded bg-[#030d12] border-slate-700 text-cyan-500 focus:ring-cyan-500/20 cursor-pointer"
            />
            <span class="text-xs text-slate-300">Запомнить меня</span>
          </label>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          class="w-full mt-2 py-3 px-4 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-sm tracking-wide shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
        >
          {#if isSubmitting}
            <div class="w-4 h-4 border-2 border-black border-t-transparent rounded-full animate-spin"></div>
            <span>Вход...</span>
          {:else}
            <LogIn class="w-4 h-4 text-black" />
            <span>Войти</span>
            <ArrowRight class="w-4 h-4 ml-1 text-black" />
          {/if}
        </button>
      </form>

      <div class="mt-6 pt-4 border-t border-cyan-950/80 text-center">
        <p class="text-xs text-slate-400">
          Нет аккаунта?
          <button
            onclick={() => { closeModal(); uiStore.setTab('register'); }}
            class="font-bold text-cyan-400 hover:text-cyan-300 ml-1 cursor-pointer"
          >
            Зарегистрироваться
          </button>
        </p>
      </div>
    </div>
  </div>
{/if}
