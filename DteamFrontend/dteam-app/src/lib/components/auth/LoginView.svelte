<script lang="ts">
import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { LogIn, Mail, Lock, Eye, EyeOff, ShieldCheck, Gamepad2, ArrowRight } from 'lucide-svelte';

  let identifier = $state('');
  let password = $state('');
  let rememberMe = $state(true);
  let showPassword = $state(false);
  let isSubmitting = $state(false);
  let errorMessage = $state('');

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
        title: 'Успешный вход',
        message: `С возвращением, ${identifier}!`,
        type: 'success'
      });
      uiStore.setTab('store');
    } catch (err: any) {
      errorMessage = err.message || 'Ошибка входа. Проверьте введенные данные.';
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
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-gradient-to-br from-cyan-400 via-teal-500 to-emerald-400 shadow-lg shadow-cyan-500/30 mb-4">
          <Gamepad2 class="w-7 h-7 text-black font-black" />
        </div>
        <h1 class="text-2xl sm:text-3xl font-black text-white font-display tracking-wide">
          Вход в <span class="text-cyan-400">DTEAM</span>
        </h1>
        <p class="text-xs sm:text-sm text-slate-400 mt-2">
          Вход в мир Web3 гейминга и цифровых коллекций
        </p>
      </div>

      {#if errorMessage}
        <div class="mb-6 p-3.5 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-5">

        <div>
          <label for="login-identifier" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-2">
            Логин или Email
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Mail class="w-4 h-4" />
            </div>
            <input
              id="login-identifier"
              type="text"
              bind:value={identifier}
              placeholder="username или user@example.com"
              required
              class="w-full pl-10 pr-4 py-3 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
            />
          </div>
        </div>

        <div>
          <div class="flex items-center justify-between mb-2">
            <label for="login-password" class="block text-xs font-bold text-slate-300 uppercase tracking-wider">
              Пароль
            </label>
            <button
              type="button"
              onclick={() => uiStore.setTab('forgot-password')}
              class="text-xs font-semibold text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer"
            >
              Забыли пароль?
            </button>
          </div>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Lock class="w-4 h-4" />
            </div>
            <input
              id="login-password"
              type={showPassword ? 'text' : 'password'}
              bind:value={password}
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

        <div class="flex items-center justify-between pt-1">
          <label class="flex items-center gap-2.5 cursor-pointer group">
            <input
              type="checkbox"
              bind:checked={rememberMe}
              class="w-4 h-4 rounded bg-[#030d12] border-slate-700 text-cyan-500 focus:ring-cyan-500/20 focus:ring-offset-0 cursor-pointer"
            />
            <span class="text-xs text-slate-300 group-hover:text-white transition-colors">
              Запомнить меня
            </span>
          </label>

          <span class="text-[11px] text-slate-500 flex items-center gap-1">
            <ShieldCheck class="w-3.5 h-3.5 text-cyan-400/80" /> TON Encrypted
          </span>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          class="w-full py-3.5 px-4 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-sm tracking-wide shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
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

      <div class="mt-8 pt-6 border-t border-cyan-950/80 text-center">
        <p class="text-xs text-slate-400">
          Еще нет аккаунта?
          <button
            onclick={() => uiStore.setTab('register')}
            class="font-bold text-cyan-400 hover:text-cyan-300 ml-1 transition-colors cursor-pointer"
          >
            Зарегистрироваться
          </button>
        </p>
      </div>
    </div>
  </div>
</div>

