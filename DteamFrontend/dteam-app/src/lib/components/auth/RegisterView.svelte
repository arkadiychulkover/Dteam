<script lang="ts">
  import { onMount } from 'svelte';
  import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { UserPlus, User, Mail, Lock, Eye, EyeOff, Gamepad2, ArrowRight, Wallet, Check } from 'lucide-svelte';

  let username = $state('');
  let email = $state('');
  let password = $state('');
  let confirmPassword = $state('');
  let hardhatAddress = $state('');
  let isConnectingWallet = $state(false);
  let acceptTerms = $state(false);
  let showPassword = $state(false);
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  function handleAccountsChanged(accounts: string[]) {
    if (accounts && accounts.length > 0) {
      hardhatAddress = accounts[0];
      errorMessage = '';
    } else {
      hardhatAddress = '';
    }
  }

  onMount(() => {
    if (typeof window !== 'undefined' && (window as any).ethereum) {
      const eth = (window as any).ethereum;

      // Check if accounts are already connected in MetaMask
      eth.request({ method: 'eth_accounts' })
        .then((accounts: string[]) => {
          if (accounts && accounts.length > 0) {
            hardhatAddress = accounts[0];
          }
        })
        .catch(() => {});

      // Listen for account change events directly from MetaMask
      eth.on('accountsChanged', handleAccountsChanged);

      return () => {
        if (eth.removeListener) {
          eth.removeListener('accountsChanged', handleAccountsChanged);
        }
      };
    }
  });

  async function connectMetaMask() {
    errorMessage = '';
    if (typeof window === 'undefined' || !(window as any).ethereum) {
      errorMessage = 'MetaMask не обнаружен! Пожалуйста, установите расширение MetaMask для браузера.';
      return;
    }

    try {
      isConnectingWallet = true;
      const accounts = await (window as any).ethereum.request({
        method: 'eth_requestAccounts'
      });
      if (accounts && accounts.length > 0) {
        hardhatAddress = accounts[0];
      }
    } catch (err: any) {
      if (err.code === 4001) {
        errorMessage = 'Подключение MetaMask было отклонено пользователем.';
      } else {
        errorMessage = err.message || 'Не удалось подключиться к MetaMask.';
      }
    } finally {
      isConnectingWallet = false;
    }
  }

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();

    if (!username.trim() || !email.trim() || !password || !confirmPassword) {
      errorMessage = 'Заполните все обязательные поля';
      return;
    }

    if (!hardhatAddress.trim()) {
      errorMessage = 'Пожалуйста, подключите кошелек MetaMask (Hardhat-адрес обязателен для регистрации)';
      return;
    }

    if (!/^0x[a-fA-F0-9]{40}$/.test(hardhatAddress.trim())) {
      errorMessage = 'Некорректный формат Ethereum/Hardhat адреса кошелька';
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
      await authStore.register(email.trim(), username.trim(), password, hardhatAddress.trim());
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

        <!-- Web3 MetaMask Wallet Connection -->
        <div class="p-3.5 rounded-2xl bg-[#061820]/90 border border-cyan-500/30 space-y-2.5">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-1.5">
              <Wallet class="w-3.5 h-3.5 text-cyan-400" />
              <span class="text-xs font-bold text-slate-200 uppercase tracking-wider">Hardhat / Web3 Кошелек</span>
              <span class="text-rose-400 text-xs font-bold">*</span>
            </div>
            {#if hardhatAddress}
              <span class="inline-flex items-center gap-1 text-[11px] font-bold text-emerald-400 bg-emerald-500/10 px-2 py-0.5 rounded-full border border-emerald-500/30">
                <Check class="w-3 h-3" /> Подключен
              </span>
            {/if}
          </div>

          {#if hardhatAddress}
            <div class="flex items-center justify-between gap-2 p-2.5 rounded-xl bg-[#030d12] border border-cyan-500/20">
              <div class="min-w-0 flex items-center gap-2">
                <div class="w-7 h-7 rounded-lg bg-gradient-to-br from-amber-500 to-orange-500 flex items-center justify-center text-xs font-black text-white shrink-0 shadow-md">
                  🦊
                </div>
                <div class="min-w-0">
                  <p class="font-mono text-xs text-cyan-300 font-bold truncate">{hardhatAddress}</p>
                </div>
              </div>
              <span class="inline-flex items-center text-[10px] font-medium text-slate-400 shrink-0 bg-slate-800/40 px-2 py-1 rounded-lg border border-slate-700/40">
                Авто-синхронизация
              </span>
            </div>
          {:else}
            <button
              type="button"
              onclick={connectMetaMask}
              disabled={isConnectingWallet}
              class="w-full py-2.5 px-4 rounded-xl bg-gradient-to-r from-amber-500/20 to-orange-500/20 hover:from-amber-500/30 hover:to-orange-500/30 border border-amber-500/40 hover:border-amber-400 text-amber-300 font-bold text-xs flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50 shadow-sm"
            >
              {#if isConnectingWallet}
                <div class="w-3.5 h-3.5 border-2 border-amber-300 border-t-transparent rounded-full animate-spin"></div>
                <span>Подключение к MetaMask...</span>
              {:else}
                <span class="text-base">🦊</span>
                <span>Подключить кошелек MetaMask</span>
              {/if}
            </button>
            <p class="text-[10px] text-slate-400 leading-tight">
              Адрес кошелька будет привязан к вашему аккаунту для начисления и использования токенов DteamPoints.
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
              Я соглашаюсь с <a href="#terms" onclick={(e) => e.preventDefault()} class="text-cyan-400 hover:underline">Условиями использования</a> и <a href="#privacy" onclick={(e) => e.preventDefault()} class="text-cyan-400 hover:underline">Политикой конфиденциальности</a> DTEAM.
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
            <span>Создание...</span>
          {:else}
            <UserPlus class="w-4 h-4 text-black" />
            <span>Создать аккаунт</span>
            <ArrowRight class="w-4 h-4 ml-1 text-black" />
          {/if}
        </button>
      </form>

      <div class="mt-6 pt-5 border-t border-cyan-950/80 text-center">
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
