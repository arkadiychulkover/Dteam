<script lang="ts">
  import { onMount } from 'svelte';
  import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { UserPlus, User, Mail, Lock, Eye, EyeOff, Gamepad2, ArrowRight, Wallet, Check, RefreshCw } from 'lucide-svelte';

  let username = $state('');
  let email = $state('');
  let password = $state('');
  let confirmPassword = $state('');
  let hardhatAddress = $state('');
  let isConnectingWallet = $state(false);
  let isSyncingWallet = $state(false);
  let acceptTerms = $state(false);
  let showPassword = $state(false);
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  function getProviders(): any[] {
    if (typeof window === 'undefined') return [];
    const eth = (window as any).ethereum;
    if (!eth) return [];
    if (Array.isArray(eth.providers) && eth.providers.length > 0) {
      return eth.providers;
    }
    return [eth];
  }

  function getActiveMetaMaskProvider(): any {
    const providers = getProviders();
    if (providers.length === 0) return null;
    return providers.find((p: any) => p.isMetaMask) || providers[0];
  }

  function handleAccountsChanged(accounts: string[]) {
    console.log('[RegisterView] accountsChanged event received:', accounts);
    if (accounts && accounts.length > 0) {
      hardhatAddress = accounts[0];
      errorMessage = '';
    } else {
      hardhatAddress = '';
    }
  }

  async function syncAccounts() {
    const provider = getActiveMetaMaskProvider();
    if (!provider) return;

    try {
      isSyncingWallet = true;
      const accounts = await provider.request({ method: 'eth_accounts' });
      if (accounts && accounts.length > 0) {
        if (hardhatAddress !== accounts[0]) {
          console.log('[RegisterView] Account changed to:', accounts[0]);
          hardhatAddress = accounts[0];
          errorMessage = '';
        }
      }
    } catch (err) {
      console.warn('[RegisterView] Error syncing accounts:', err);
    } finally {
      setTimeout(() => {
        isSyncingWallet = false;
      }, 300);
    }
  }

  let cleanupListeners: (() => void)[] = [];

  function attachProviderListeners() {
    cleanupListeners.forEach(c => c());
    cleanupListeners = [];

    const providers = getProviders();
    providers.forEach(p => {
      if (p && typeof p.on === 'function') {
        p.on('accountsChanged', handleAccountsChanged);
        cleanupListeners.push(() => {
          if (typeof p.removeListener === 'function') {
            p.removeListener('accountsChanged', handleAccountsChanged);
          } else if (typeof p.off === 'function') {
            p.off('accountsChanged', handleAccountsChanged);
          }
        });
      }
    });
  }

  onMount(() => {
    attachProviderListeners();
    syncAccounts();

    const handleWindowFocus = () => {
      syncAccounts();
    };

    const handleVisibility = () => {
      if (!document.hidden) {
        syncAccounts();
      }
    };

    const handleInit = () => {
      attachProviderListeners();
      syncAccounts();
    };

    window.addEventListener('focus', handleWindowFocus);
    document.addEventListener('visibilitychange', handleVisibility);
    window.addEventListener('ethereum#initialized', handleInit, { once: true });

    // Fallback polling every 2s for instant UI feedback on MetaMask account changes
    const interval = setInterval(syncAccounts, 2000);

    return () => {
      clearInterval(interval);
      window.removeEventListener('focus', handleWindowFocus);
      document.removeEventListener('visibilitychange', handleVisibility);
      cleanupListeners.forEach(c => c());
    };
  });

  async function connectMetaMask(requestSwitch = false) {
    errorMessage = '';
    const provider = getActiveMetaMaskProvider();
    if (!provider) {
      errorMessage = 'MetaMask не виявлено! Будь ласка, встановіть розширення MetaMask для браузера.';
      return;
    }

    try {
      isConnectingWallet = true;
      attachProviderListeners();

      if (requestSwitch) {
        try {
          await provider.request({
            method: 'wallet_requestPermissions',
            params: [{ eth_accounts: {} }]
          });
        } catch (permErr: any) {
          console.warn('[RegisterView] wallet_requestPermissions not accepted or supported:', permErr);
        }
      }

      const accounts = await provider.request({
        method: 'eth_requestAccounts'
      });

      if (accounts && accounts.length > 0) {
        hardhatAddress = accounts[0];
        errorMessage = '';
      }
    } catch (err: any) {
      if (err.code === 4001) {
        errorMessage = 'Підключення MetaMask було відхилено користувачем.';
      } else {
        errorMessage = err.message || 'Не вдалося підключитися до MetaMask.';
      }
    } finally {
      isConnectingWallet = false;
    }
  }

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();

    if (!username.trim() || !email.trim() || !password || !confirmPassword) {
      errorMessage = 'Заповніть усі обов\'язкові поля';
      return;
    }

    if (!hardhatAddress.trim()) {
      errorMessage = 'Будь ласка, підключіть гаманець MetaMask (Hardhat-адреса обов\'язкова для реєстрації)';
      return;
    }

    if (!/^0x[a-fA-F0-9]{40}$/.test(hardhatAddress.trim())) {
      errorMessage = 'Некоректний формат Ethereum/Hardhat адреси гаманця';
      return;
    }

    if (password !== confirmPassword) {
      errorMessage = 'Паролі не співпадають';
      return;
    }

    if (password.length < 6) {
      errorMessage = 'Пароль має бути не менше 6 символів';
      return;
    }

    if (!acceptTerms) {
      errorMessage = 'Необхідно погодитися з умовами використання та політикою конфіденційності';
      return;
    }

    errorMessage = '';
    isSubmitting = true;

    try {
      await authStore.register(email.trim(), username.trim(), password, hardhatAddress.trim());
      uiStore.addToast({
        title: 'Реєстрація успішна',
        message: `Ласкаво просимо до DTEAM, ${username}!`,
        type: 'success'
      });
      uiStore.setTab('store');
    } catch (err: any) {
      errorMessage = err.message || 'Помилка під час створення акаунта.';
    } finally {
      isSubmitting = false;
    }
  }
</script>

<div class="min-h-[80vh] flex items-center justify-center p-4 sm:p-6 lg:p-8">
  <div class="relative w-full max-w-lg">

    <div class="absolute -top-10 -right-10 w-72 h-72 bg-cyan-500/15 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-10 -left-10 w-72 h-72 bg-teal-500/15 rounded-full blur-3xl pointer-events-none"></div>

    <div class="relative bg-[#09151e]/90 backdrop-blur-xl border border-cyan-500/30 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/80">

      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-gradient-to-br from-cyan-400 via-teal-500 to-emerald-400 shadow-lg shadow-cyan-500/30 mb-4">
          <Gamepad2 class="w-7 h-7 text-black font-black" />
        </div>
        <h1 class="text-2xl sm:text-3xl font-black text-white font-display tracking-wide">
          Створення акаунта <span class="text-cyan-400">DTEAM</span>
        </h1>
        <p class="text-xs sm:text-sm text-slate-400 mt-2">
          Приєднуйтесь до ігрової платформи нового покоління
        </p>
      </div>

      {#if errorMessage}
        <div class="mb-6 p-3.5 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-4">

        <div>
          <label for="reg-username" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Логін (Нікнейм)
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
              class="w-full pl-10 pr-4 py-2.5 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
            />
          </div>
        </div>

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
              class="w-full pl-10 pr-4 py-2.5 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
            />
          </div>
        </div>

        <div class="p-3.5 rounded-2xl bg-[#061820]/90 border border-cyan-500/30 space-y-2.5">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-1.5">
              <Wallet class="w-3.5 h-3.5 text-cyan-400" />
              <span class="text-xs font-bold text-slate-200 uppercase tracking-wider">Hardhat / Web3 Гаманець</span>
              <span class="text-rose-400 text-xs font-bold">*</span>
            </div>
            {#if hardhatAddress}
              <span class="inline-flex items-center gap-1 text-[11px] font-bold text-emerald-400 bg-emerald-500/10 px-2 py-0.5 rounded-full border border-emerald-500/30">
                <Check class="w-3 h-3" /> Підключено
              </span>
            {/if}
          </div>

          {#if hardhatAddress}
            <div class="p-3 rounded-2xl bg-[#030d12] border border-cyan-500/30 space-y-2">
              <div class="flex items-center justify-between gap-2">
                <div class="min-w-0 flex items-center gap-2.5">
                  <div class="w-8 h-8 rounded-xl bg-gradient-to-br from-amber-500 to-orange-500 flex items-center justify-center text-sm font-black text-white shrink-0 shadow-md">
                    🦊
                  </div>
                  <div class="min-w-0">
                    <p class="text-[10px] text-slate-400 uppercase tracking-wider font-semibold">Активний гаманець MetaMask</p>
                    <p class="font-mono text-xs text-cyan-300 font-bold truncate">{hardhatAddress}</p>
                  </div>
                </div>
                <div class="flex items-center gap-1.5 shrink-0">
                  <button
                    type="button"
                    title="Синхронізувати адресу"
                    onclick={syncAccounts}
                    disabled={isSyncingWallet}
                    class="p-1.5 rounded-lg bg-slate-800/80 hover:bg-slate-700 text-slate-300 hover:text-cyan-400 border border-slate-700/60 transition-all cursor-pointer"
                  >
                    <RefreshCw class="w-3.5 h-3.5 {isSyncingWallet ? 'animate-spin text-cyan-400' : ''}" />
                  </button>
                  <button
                    type="button"
                    title="Вибрати інший акаунт у MetaMask"
                    onclick={() => connectMetaMask(true)}
                    disabled={isConnectingWallet}
                    class="px-2.5 py-1 rounded-lg bg-amber-500/20 hover:bg-amber-500/30 text-amber-300 hover:text-amber-200 border border-amber-500/40 text-xs font-semibold transition-all cursor-pointer"
                  >
                    Змінити
                  </button>
                </div>
              </div>
              <div class="flex items-center justify-between pt-1 border-t border-slate-800/60 text-[10px] text-slate-400">
                <span class="inline-flex items-center gap-1 text-emerald-400">
                  <span class="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse"></span>
                  Автоматичне відстеження активного акаунта
                </span>
                <span class="font-mono text-slate-500">
                  {hardhatAddress.slice(0, 6)}...{hardhatAddress.slice(-4)}
                </span>
              </div>
            </div>
          {:else}
            <button
              type="button"
              onclick={() => connectMetaMask(false)}
              disabled={isConnectingWallet}
              class="w-full py-2.5 px-4 rounded-xl bg-gradient-to-r from-amber-500/20 to-orange-500/20 hover:from-amber-500/30 hover:to-orange-500/30 border border-amber-500/40 hover:border-amber-400 text-amber-300 font-bold text-xs flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50 shadow-sm"
            >
              {#if isConnectingWallet}
                <div class="w-3.5 h-3.5 border-2 border-amber-300 border-t-transparent rounded-full animate-spin"></div>
                <span>Підключення до MetaMask...</span>
              {:else}
                <span class="text-base">🦊</span>
                <span>Підключити гаманець MetaMask</span>
              {/if}
            </button>
            <p class="text-[10px] text-slate-400 leading-tight">
              Адреса гаманця буде прив'язана до вашого акаунта для нарахування та використання токенів DteamPoints.
            </p>
          {/if}
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">

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
                class="w-full pl-10 pr-10 py-2.5 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
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

          <div>
            <label for="reg-confirm-password" class="block text-xs font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Підтвердження
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
                class="w-full pl-10 pr-4 py-2.5 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
              />
            </div>
          </div>
        </div>

        <div class="pt-2">
          <label class="flex items-start gap-2.5 cursor-pointer group">
            <input
              type="checkbox"
              bind:checked={acceptTerms}
              class="mt-0.5 w-4 h-4 rounded bg-[#030d12] border-slate-700 text-cyan-500 focus:ring-cyan-500/20 focus:ring-offset-0 cursor-pointer"
            />
            <span class="text-xs text-slate-400 leading-snug group-hover:text-slate-300 transition-colors">
              Я погоджуюся з <button type="button" onclick={() => uiStore.setTab('terms')} class="text-cyan-400 hover:underline font-semibold cursor-pointer">Умовами використання</button> та <button type="button" onclick={() => uiStore.setTab('privacy')} class="text-cyan-400 hover:underline font-semibold cursor-pointer">Політикою конфіденційності</button> DTEAM.
            </span>
          </label>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          class="w-full mt-4 py-3.5 px-4 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-sm tracking-wide shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
        >
          {#if isSubmitting}
            <div class="w-4 h-4 border-2 border-black border-t-transparent rounded-full animate-spin"></div>
            <span>Створення...</span>
          {:else}
            <UserPlus class="w-4 h-4 text-black" />
            <span>Створити акаунт</span>
            <ArrowRight class="w-4 h-4 ml-1 text-black" />
          {/if}
        </button>
      </form>

      <div class="mt-6 pt-5 border-t border-cyan-950/80 text-center">
        <p class="text-xs text-slate-400">
          Вже є акаунт?
          <button
            onclick={() => uiStore.setTab('login')}
            class="font-bold text-cyan-400 hover:text-cyan-300 ml-1 transition-colors cursor-pointer"
          >
            Увійти
          </button>
        </p>
      </div>
    </div>
  </div>
</div>
