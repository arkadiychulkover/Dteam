<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { UserPlus, User, Mail, Lock, Eye, EyeOff, CheckSquare, ShieldCheck, Gamepad2, ArrowRight } from 'lucide-svelte';

  let username = $state('');
  let email = $state('');
  let password = $state('');
  let confirmPassword = $state('');
  let acceptTerms = $state(false);
  let showPassword = $state(false);
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();

    if (!username.trim() || !email.trim() || !password || !confirmPassword) {
      errorMessage = 'Заполните все обязательные поля';
      return;
    }

    if (password !== confirmPassword) {
      errorMessage = 'Пароли не совпадают';
      return;
    }

    if (password.length < 6) {
      errorMessage = 'Пароль должен быть не менее 6 символов';
      return;
    }

    if (!acceptTerms) {
      errorMessage = 'Необходимо согласиться с условиями использования';
      return;
    }

    errorMessage = '';
    isSubmitting = true;

    try {
      await authStore.register(email, username, password);
      uiStore.addToast({
        title: 'Регистрация успешна',
        message: `Добро пожаловать в DTEAM, ${username}!`,
        type: 'success'
      });
      uiStore.setTab('store');
    } catch (err: any) {
      errorMessage = err.message || 'Ошибка при создании аккаунта.';
    } finally {
      isSubmitting = false;
    }
  }
</script>

<div class="min-h-[85vh] flex items-center justify-center p-4 sm:p-6 lg:p-8">
  <div class="relative w-full max-w-lg">
    <!-- Glow Backdrops -->
    <div class="absolute -top-10 -right-10 w-72 h-72 bg-cyan-500/15 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-10 -left-10 w-72 h-72 bg-blue-600/15 rounded-full blur-3xl pointer-events-none"></div>

    <div class="relative bg-[#111422]/90 backdrop-blur-xl border border-slate-800 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/30">
      <!-- Header -->
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-gradient-to-br from-cyan-500 to-blue-600 shadow-lg shadow-cyan-500/30 mb-4">
          <Gamepad2 class="w-7 h-7 text-white" />
        </div>
        <h1 class="text-2xl sm:text-3xl font-extrabold text-white font-['Outfit'] tracking-wide">
          Создание аккаунта <span class="text-cyan-400">DTEAM</span>
        </h1>
        <p class="text-xs sm:text-sm text-slate-400 mt-2">
          Присоединяйтесь к игровой платформе нового поколения
        </p>
      </div>

      {#if errorMessage}
        <div class="mb-6 p-3.5 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-4">
        <!-- Username Input -->
        <div>
          <label for="reg-username" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Логин (Никнейм)
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <User class="w-4 h-4" />
            </div>
            <input
              id="reg-username"
              type="text"
              bind:value={username}
              placeholder="CyberHero"
              required
              class="w-full pl-10 pr-4 py-2.5 bg-slate-900/80 border border-slate-800 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500 focus:ring-1 focus:ring-cyan-500 transition-all"
            />
          </div>
        </div>

        <!-- Email Input -->
        <div>
          <label for="reg-email" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Email
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Mail class="w-4 h-4" />
            </div>
            <input
              id="reg-email"
              type="email"
              bind:value={email}
              placeholder="user@example.com"
              required
              class="w-full pl-10 pr-4 py-2.5 bg-slate-900/80 border border-slate-800 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500 focus:ring-1 focus:ring-cyan-500 transition-all"
            />
          </div>
        </div>

        <!-- Password Inputs Grid -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <!-- Password -->
          <div>
            <label for="reg-password" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Пароль
            </label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
                <Lock class="w-4 h-4" />
              </div>
              <input
                id="reg-password"
                type={showPassword ? 'text' : 'password'}
                bind:value={password}
                placeholder="••••••••••••"
                required
                class="w-full pl-10 pr-10 py-2.5 bg-slate-900/80 border border-slate-800 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500 focus:ring-1 focus:ring-cyan-500 transition-all"
              />
              <button
                type="button"
                onclick={() => showPassword = !showPassword}
                class="absolute inset-y-0 right-0 pr-3 flex items-center text-slate-500 hover:text-slate-300 transition-colors cursor-pointer"
              >
                {#if showPassword}
                  <EyeOff class="w-3.5 h-3.5" />
                {:else}
                  <Eye class="w-3.5 h-3.5" />
                {/if}
              </button>
            </div>
          </div>

          <!-- Confirm Password -->
          <div>
            <label for="reg-confirm-password" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Подтверждение
            </label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
                <Lock class="w-4 h-4" />
              </div>
              <input
                id="reg-confirm-password"
                type={showPassword ? 'text' : 'password'}
                bind:value={confirmPassword}
                placeholder="••••••••••••"
                required
                class="w-full pl-10 pr-4 py-2.5 bg-slate-900/80 border border-slate-800 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500 focus:ring-1 focus:ring-cyan-500 transition-all"
              />
            </div>
          </div>
        </div>

        <!-- Terms of Service Checkbox -->
        <div class="pt-2">
          <label class="flex items-start gap-2.5 cursor-pointer group">
            <input
              type="checkbox"
              bind:checked={acceptTerms}
              class="mt-0.5 w-4 h-4 rounded bg-slate-900 border-slate-700 text-cyan-500 focus:ring-cyan-500/20 focus:ring-offset-0 cursor-pointer"
            />
            <span class="text-xs text-slate-400 leading-snug group-hover:text-slate-300 transition-colors">
              Я соглашаюсь с <a href="#terms" onclick={(e) => e.preventDefault()} class="text-cyan-400 hover:underline">Условиями использования</a> и <a href="#privacy" onclick={(e) => e.preventDefault()} class="text-cyan-400 hover:underline">Политикой конфиденциальности</a> DTEAM.
            </span>
          </label>
        </div>

        <!-- Submit Button -->
        <button
          type="submit"
          disabled={isSubmitting}
          class="w-full mt-4 py-3.5 px-4 rounded-xl bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white font-bold text-sm tracking-wide shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
        >
          {#if isSubmitting}
            <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
            <span>Создание...</span>
          {:else}
            <UserPlus class="w-4 h-4" />
            <span>Создать аккаунт</span>
            <ArrowRight class="w-4 h-4 ml-1" />
          {/if}
        </button>
      </form>

      <!-- Footer Switch to Login -->
      <div class="mt-6 pt-5 border-t border-slate-800/80 text-center">
        <p class="text-xs text-slate-400">
          Уже есть аккаунт?
          <button
            onclick={() => uiStore.setTab('login')}
            class="font-bold text-cyan-400 hover:text-cyan-300 ml-1 transition-colors cursor-pointer"
          >
            Войти
          </button>
        </p>
      </div>
    </div>
  </div>
</div>
